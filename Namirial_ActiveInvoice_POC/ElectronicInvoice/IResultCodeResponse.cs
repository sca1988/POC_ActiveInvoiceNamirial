namespace Namirial_ActiveInvoice_POC.SolutionDOC_Hub;

/// <summary>
/// Implemented by the generated SOAP responses that report their outcome through a <see cref="SolutionDOC_Hub.ResultCode"/>,
/// so they can be handled generically (e.g. by the retry logic in
/// <see cref="Namirial_ActiveInvoice_POC.ElectronicInvoice.ActiveInvoiceWsClientFactory"/>).
/// </summary>
public interface IResultCodeResponse
{
    ResultCode ResultCode { get; }

    /// <summary>Human readable description of a failure, if the service provided one.</summary>
    string? ErrorDescription { get; }
}

public partial class UploadElectronicInvoiceResponse : IResultCodeResponse
{
    string? IResultCodeResponse.ErrorDescription => ErrorMessage;
}

public partial class StandardResponse : IResultCodeResponse
{
    string? IResultCodeResponse.ErrorDescription => ResultMessage;
}

public partial class StandardListResponse : IResultCodeResponse
{
    string? IResultCodeResponse.ErrorDescription =>
        ResultMessage is { Length: > 0 } messages ? string.Join("; ", messages) : null;
}
