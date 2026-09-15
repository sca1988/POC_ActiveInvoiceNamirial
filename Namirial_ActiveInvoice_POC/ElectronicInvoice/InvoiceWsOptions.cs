using Microsoft.Extensions.Options;

namespace Namirial_ActiveInvoice_POC.ElectronicInvoice;

public class InvoiceWsOptions
{
    public const string SectionName = "InvoiceWebService";
    public string CustomerCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ActiveInvoiceBaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="InvoiceWsOptions"/>.
/// </summary>
public class InvoiceWsOptionsValidator() : IValidateOptions<InvoiceWsOptions>
{
    public ValidateOptionsResult Validate(string? name, InvoiceWsOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.CustomerCode))
            errors.Add("InvoiceWebService.CustomerCode must not be empty.");
        if (string.IsNullOrWhiteSpace(options.Password))
            errors.Add("InvoiceWebService.Password must not be empty.");
        if (string.IsNullOrWhiteSpace(options.ActiveInvoiceBaseUrl))
            errors.Add("InvoiceWebService.ActiveInvoiceBaseUrl must not be empty.");
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
