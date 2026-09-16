using Namirial_ActiveInvoice_POC.SolutionDOC_Hub;

namespace Namirial_ActiveInvoice_POC.ElectronicInvoice;

/// <summary>
/// Implemented by the generated SOAP responses that report their outcome through a <see cref="SolutionDOC_Hub.ResultCode"/>,
/// so they can be handled generically by the retry logic in <see cref="ActiveInvoiceWsClientFactory"/>.
/// The implementations live next to the generated code, in
/// <c>ElectronicInvoice/SolutionDOC_Hub.ResultCodeResponses.cs</c>.
/// </summary>
public interface IResultCodeResponse
{
    ResultCode ResultCode { get; }

    /// <summary>Human readable description of a failure, if the service provided one.</summary>
    string? ErrorDescription { get; }
}
