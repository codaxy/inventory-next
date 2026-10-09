using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Setup;

public static class OptionsValidation
{
    /// <summary>
    /// Validates by what the options say is wrong with them — their <c>Problem</c>, null when valid —
    /// so the reason lives beside the rule and a new rule brings its own message.
    /// </summary>
    public static OptionsBuilder<T> ValidateBy<T>(
        this OptionsBuilder<T> builder,
        Func<T, string?> problem
    )
        where T : class
    {
        builder.Services.AddSingleton<IValidateOptions<T>>(
            new ProblemValidation<T>(builder.Name, problem)
        );
        return builder;
    }

    private sealed class ProblemValidation<T>(string? name, Func<T, string?> problem)
        : IValidateOptions<T>
        where T : class
    {
        public ValidateOptionsResult Validate(string? optionsName, T options) =>
            name is not null && name != optionsName ? ValidateOptionsResult.Skip
            : problem(options) is { } reason ? ValidateOptionsResult.Fail(reason)
            : ValidateOptionsResult.Success;
    }
}
