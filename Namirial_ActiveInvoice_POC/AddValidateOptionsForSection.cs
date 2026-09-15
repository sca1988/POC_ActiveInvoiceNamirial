using Microsoft.Extensions.Options;

namespace Namirial_ActiveInvoice_POC;

public static class ValidateOptionsForSection
{
    public static IServiceCollection AddValidatedOptionsForSection<TOptions, TValidator>(
        this IServiceCollection services, IConfigurationSection configSection)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        services.AddOptions<TOptions>()
            .Bind(configSection)
            .ValidateOnStart();

        return services;
    }
}