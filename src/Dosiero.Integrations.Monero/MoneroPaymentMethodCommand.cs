using Dosiero.Abstractions.Payments;
using Dosiero.Configuration;

using Microsoft.Extensions.Options;

using Monero.WalletRpc;

using System.CommandLine;

namespace Dosiero.Integrations.Monero;

public sealed class MoneroPaymentMethodCommand : IPaymentMethodCommand<MoneroPaymentOptions>
{
    public static string Ticker => "monero";

    public static string Description => "Add and configure support for monero payments.";

    private static Argument<Uri> Url { get; } = Argument<Uri>.Create(() =>
    {
        var uri = new Argument<Uri>("url")
        {
            Description = "The URL of the wallet RPC server to use.",
            CustomParser = arg =>
            {
                if (!Uri.TryCreate(arg.Tokens.Single().Value, UriKind.Absolute, out var uri))
                {
                    arg.AddError($"Could not parse '{uri}' as a URI");
                }

                return uri;
            }
        };
        return uri;
    });

    private static Option<string> Username { get; } = new("--username")
    {
        Description = "The username to login to the RPC server with."
    };

    private static Option<string> Password { get; } = new("--password")
    {
        Description = "The password to login to the RPC server with."
    };

    private static Option<bool> AcceptSelfSignedSsl { get; } = new("--accept-self-signed-ssl")
    {
        Description = "Whether or not to accept a self signed SSL certificate from the wallet RPC server.",
        DefaultValueFactory = _ => true,
    };

    public static IServiceCollection AddPaymentMethod(IServiceCollection services)
    {
        services
            .AddWalletRpc(services =>
            {
                var options = services.GetRequiredService<IOptions<MoneroPaymentOptions>>().Value;
                return options.ToWalletRpcOptions();
            });
        services.AddPaymentIntegration<MoneroPaymentIntegration, SignedMoneroPayment>();
        return services;
    }

    public static void BindConfiguration(MoneroPaymentOptions options, ParseResult result)
    {
        options.Url = result.GetRequiredValue(Url);
        options.Username = result.GetValue(Username);
        options.Password = result.GetValue(Password);
        options.AcceptSelfSignedCerts = result.GetValue(AcceptSelfSignedSsl);
    }

    public static void ConfigureCommand(Command command)
    {
        command.Arguments.Add(Url);
        command.Options.Add(Username);
        command.Options.Add(Password);
        command.Options.Add(AcceptSelfSignedSsl);
    }
}
