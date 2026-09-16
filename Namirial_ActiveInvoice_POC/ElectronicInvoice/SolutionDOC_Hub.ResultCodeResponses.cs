using Namirial_ActiveInvoice_POC.ElectronicInvoice;

// Hand written extensions to the svcutil generated types in
// "Connected Services/SolutionDOC_Hub/SolutionDOC_Hub.cs": they map each response's own error member
// onto the common IResultCodeResponse contract used by ActiveInvoiceWsClientFactory.
//
// The namespace must match the generated one (partial types cannot span namespaces), but the file
// deliberately lives here, outside "Connected Services", so that regenerating the service reference
// never touches or deletes it.
namespace Namirial_ActiveInvoice_POC.SolutionDOC_Hub;

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
