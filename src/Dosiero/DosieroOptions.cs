using Microsoft.Extensions.Options;

namespace Dosiero;

internal sealed class DosieroOptions
{
    public const string Dosiero = nameof(Dosiero);

    public required string ConfigFolder { get; set; }
}

internal sealed class DosieroOptionsValidator : IValidateOptions<DosieroOptions>
{
    public ValidateOptionsResult Validate(string? name, DosieroOptions options)
    {
        if (!Path.Exists(options.ConfigFolder))
        {
            return ValidateOptionsResult.Fail($"The config folder '{options.ConfigFolder}' does not exist.");
        }

        return ValidateOptionsResult.Success;
    }
}