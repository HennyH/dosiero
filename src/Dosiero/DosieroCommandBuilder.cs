using Dosiero.Abstractions.Payments;
using Dosiero.Configuration;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics.CodeAnalysis;

namespace Dosiero;

file sealed class PriceCommand
{
    private readonly IFilePricer _pricer;

    public PriceCommand(IFilePricer pricer)
    {
        _pricer = pricer;

        Command = new Command("/price", "Set a price for a file");
        Command.Arguments.Add(Pattern);
        Command.Arguments.Add(Amount);
        Command.SetAction(HandlePriceCommandInvoked);

        Amount.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<decimal>() is < 0)
            {
                result.AddError("Amount must be >= 0.");
            }
        });
    }

    public readonly Argument<LikeString> Pattern = new("pattern")
    {
        Description = "The pattern to match files against.",
        CustomParser = result => new LikeString(Path.ExpandUnixPath(result.Tokens.Single().Value)),
    };

    public readonly Argument<decimal> Amount = new("amount")
    {
        Description = "The quantity of currency to pay.",
    };

    public Command Command { get; init; }

    private void HandlePriceCommandInvoked(ParseResult result)
    {
        var pattern = result.GetRequiredValue(Pattern);
        var amount = result.GetRequiredValue(Amount);
        _pricer.SetPrice(
            pattern: pattern,
            price: amount switch
            {
                0 => new FilePrice.Free(),
                > 0 => new FilePrice.Paid(amount),
                < 0 => throw new ArgumentException("Price cannot be negative")
            });
    }
}

file sealed class ReadmeCommand
{
    private readonly IFileReadmeProvider _readmes;

    public ReadmeCommand(IFileReadmeProvider readmes)
    {
        _readmes = readmes;

        Command = new Command("/readme", "Set a readme for a file");
        Command.Arguments.Add(Pattern);
        Command.Arguments.Add(Readme);
        Command.SetAction(HandleReadmeCommandInvoked);
    }

    public readonly Argument<LikeString> Pattern = new("pattern")
    {
        Description = "The pattern to match files against.",
        CustomParser = result => new LikeString(Path.ExpandUnixPath(result.Tokens.Single().Value)),
    };

    public readonly Argument<string> Readme = new("readme")
    {
        Description = "The path to the readme file.",
        CustomParser = result => Path.ExpandUnixPath(result.Tokens.Single().Value),
    };

    public Command Command { get; init; }

    private void HandleReadmeCommandInvoked(ParseResult result)
    {
        var pattern = result.GetRequiredValue(Pattern);
        var readme = result.GetRequiredValue(Readme);
        _readmes.AddFileReadme(pattern, readme);
    }
}

file sealed class ProviderCommand
{
    public readonly Command Command = new("/provider", "Add a file provider");
}

file sealed class PaymentCommand
{
    public readonly Command Command = new("/payment", "Add a payment method");

    public readonly List<string> Tickers = [];
}

file sealed class LateConfigurationBinder<TOptions> : IConfigureOptions<TOptions>
    where TOptions : class
{
    public Action<TOptions>? Bind { get; set; }

    public void Configure(TOptions options) => Bind?.Invoke(options);
}

public interface IDosieroCommandBuilder
{
    public IDosieroCommandBuilder WithFileProvider<TCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>()
        where TCommand : IFileProviderCommand<TOptions>
        where TOptions : class;

    public IDosieroCommandBuilder WithPaymentMethod<TCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>()
        where TCommand : IPaymentMethodCommand<TOptions>
        where TOptions : class;
}

file sealed class DosieroCommandBuilder(ProviderCommand provider, PaymentCommand payment, IServiceCollection services) : IDosieroCommandBuilder
{
    public IDosieroCommandBuilder WithFileProvider<TCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>()
        where TCommand : IFileProviderCommand<TOptions>
        where TOptions : class
    {
        var command = new Command(TCommand.Scheme, TCommand.Description);
        TCommand.ConfigureCommand(command);

        provider.Command.Subcommands.Add(command);

        var binder = new LateConfigurationBinder<TOptions>();
        command.SetAction(result => binder.Bind = options => TCommand.BindConfiguration(options, result));

        TCommand.AddFileProvider(services);
        services
            .AddOptionsWithValidateOnStart<TOptions>()
            .Configure(options => binder.Bind?.Invoke(options));

        return this;
    }

    public IDosieroCommandBuilder WithPaymentMethod<TCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>()
        where TCommand : IPaymentMethodCommand<TOptions>
        where TOptions : class
    {
        var command = new Command(TCommand.Ticker, TCommand.Description);
        TCommand.ConfigureCommand(command);

        payment.Command.Subcommands.Add(command);
        payment.Tickers.Add(TCommand.Ticker);

        var binder = new LateConfigurationBinder<TOptions>();
        command.SetAction(result => binder.Bind = options => TCommand.BindConfiguration(options, result));

        TCommand.AddPaymentMethod(services);
        services
            .AddOptionsWithValidateOnStart<TOptions>()
            .Configure(options => binder.Bind?.Invoke(options));

        return this;
    }
}

internal enum DosieroCommandResult
{
    Halt,
    Continue
}

internal interface IDosieroCommand
{
    public Task<DosieroCommandResult> InvokeAsync(string[] args, CancellationToken token = default);
}

file sealed class DosieroCommand(RootCommand root) : IDosieroCommand
{
    private readonly string[] _names = [.. root.Subcommands.Select(c => c.Name)];
    private readonly HelpOption _help = new();

    public async Task<DosieroCommandResult> InvokeAsync(string[] args, CancellationToken token)
    {
        int start = 0;
        for (var i = 0; i < args.Length; i++)
        {
            var isSubCommandToken = _names.Contains(args[i]);

            if (i != start && isSubCommandToken)
            {
                if (await InvokeSubcommandAsync(args[start..i], token) is DosieroCommandResult.Halt)
                {
                    return DosieroCommandResult.Halt;
                }

                start = i;
            }
        }

        if (start < args.Length)
        {
            if (await InvokeSubcommandAsync(args[start..], token) is DosieroCommandResult.Halt)
            {
                return DosieroCommandResult.Halt;
            }
        }

        return DosieroCommandResult.Continue;
    }

    private async Task<DosieroCommandResult> InvokeSubcommandAsync(string[] args, CancellationToken token)
    {
        var result = root.Parse(args);

        await result.InvokeAsync(cancellationToken: token);

        if (result.Errors is not [])
        {
            return DosieroCommandResult.Halt;
        }

        if (args is [.., var last] && (_help.Name == last || _help.Aliases.Contains(last)))
        {
            return DosieroCommandResult.Halt;
        }

        return DosieroCommandResult.Continue;
    }
}

internal static class DosieroCommandBuilderServiceCollectionExtensions
{
    public static IDosieroCommandBuilder AddDoserioCommand(this IServiceCollection services)
    {
        var provider = new ProviderCommand();
        var payment = new PaymentCommand();
        var builder = new DosieroCommandBuilder(provider, payment, services);

        services.AddSingleton<IDosieroCommand>(services =>
        {
            var price = new PriceCommand(services.GetRequiredService<IFilePricer>());
            var readme = new ReadmeCommand(services.GetRequiredService<IFileReadmeProvider>());

            var root = new RootCommand("dosiero");
            root.Subcommands.Add(provider.Command);
            root.Subcommands.Add(price.Command);
            root.Subcommands.Add(payment.Command);
            root.Subcommands.Add(readme.Command);
            root.SetAction(result =>
            {
                result.Errors.Any();
            });

            return new DosieroCommand(root);
        });

        return builder;
    }
}