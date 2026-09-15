using System.ServiceModel;
using System.Text;
using Microsoft.Extensions.Options;
using Namirial_ActiveInvoice_POC.SolutionDOC_Hub;

namespace Namirial_ActiveInvoice_POC.ElectronicInvoice;

/// <summary>
/// Creates clients and credentials for the active invoice SOAP service (Namirial SolutionDOC Hub)
/// and executes operations against it with a retry policy.
/// </summary>
public class ActiveInvoiceWsClientFactory(
    IOptions<InvoiceWsOptions> options,
    ILogger<ActiveInvoiceWsClientFactory> logger)
{
    /// <summary>Total number of attempts (first call included) performed by <c>ExecuteAsync</c>.</summary>
    public const int DefaultMaxAttempts = 3;

    /// <summary>Delay before the first retry; it is doubled on every further retry.</summary>
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);

    public SolutionDOC_HubSoapClient CreateClient()
        => new(
            SolutionDOC_HubSoapClient.EndpointConfiguration.SolutionDOC_HubSoap,
            new EndpointAddress(options.Value.ActiveInvoiceBaseUrl));

    public Auth CreateAuth()
        => new()
        {
            CustomerCode = options.Value.CustomerCode,
            Password = Encoding.ASCII.GetBytes(options.Value.Password)
        };

    /// <summary>
    /// Runs a SOAP operation whose response carries a <see cref="ResultCode"/>, retrying while the
    /// service answers <see cref="ResultCode.Failure"/> or the call fails with a transient error.
    /// </summary>
    /// <param name="operation">The call to perform, on a client created for the attempt.</param>
    /// <param name="operationName">Name used in the logs; defaults to the calling member.</param>
    /// <param name="maxAttempts">Total attempts, first call included.</param>
    public Task<TResponse> ExecuteAsync<TResponse>(
        Func<SolutionDOC_HubSoapClient, Task<TResponse>> operation,
        [System.Runtime.CompilerServices.CallerMemberName] string operationName = "",
        int maxAttempts = DefaultMaxAttempts,
        CancellationToken ct = default)
        where TResponse : IResultCodeResponse
        => ExecuteAsync(
            operation,
            response => response.ResultCode == ResultCode.Success,
            response => response.ErrorDescription,
            operationName,
            maxAttempts,
            ct);

    /// <summary>
    /// Runs a SOAP operation, retrying while <paramref name="isSuccess"/> rejects the response or the
    /// call fails with a transient error. Use this overload for responses without a <see cref="ResultCode"/>.
    /// </summary>
    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<SolutionDOC_HubSoapClient, Task<TResponse>> operation,
        Func<TResponse, bool> isSuccess,
        Func<TResponse, string?>? describeFailure = null,
        [System.Runtime.CompilerServices.CallerMemberName] string operationName = "",
        int maxAttempts = DefaultMaxAttempts,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        var delay = InitialRetryDelay;

        for (var attempt = 1; ; attempt++)
        {
            var isLastAttempt = attempt == maxAttempts;
            string? failureReason;

            try
            {
                var response = await InvokeAsync(operation, cancellationToken);

                if (isSuccess(response))
                {
                    if (attempt > 1)
                        logger.LogInformation("{Operation} succeeded on attempt {Attempt}/{MaxAttempts}.",
                            operationName, attempt, maxAttempts);

                    return response;
                }

                if (isLastAttempt)
                {
                    logger.LogError("{Operation} failed on all {MaxAttempts} attempts: {Reason}",
                        operationName, maxAttempts, describeFailure?.Invoke(response));

                    return response;
                }

                failureReason = describeFailure?.Invoke(response);
            }
            catch (Exception ex) when (IsTransient(ex) && !isLastAttempt && !cancellationToken.IsCancellationRequested)
            {
                failureReason = ex.Message;
            }

            logger.LogWarning("{Operation} failed on attempt {Attempt}/{MaxAttempts} ({Reason}); retrying in {Delay}.",
                operationName, attempt, maxAttempts, failureReason, delay);

            await Task.Delay(delay, cancellationToken);
            delay *= 2;
        }
    }

    /// <summary>
    /// Performs a single call on a dedicated client: a faulted WCF channel cannot be reused,
    /// so every attempt gets a fresh one and the channel is always closed or aborted afterwards.
    /// </summary>
    private async Task<TResponse> InvokeAsync<TResponse>(
        Func<SolutionDOC_HubSoapClient, Task<TResponse>> operation,
        CancellationToken cancellationToken)
    {
        var client = CreateClient();

        try
        {
            var response = await operation(client);
            await client.CloseAsync().WaitAsync(cancellationToken);
            return response;
        }
        catch
        {
            client.Abort();
            throw;
        }
    }

    private static bool IsTransient(Exception ex)
        => ex is CommunicationException or TimeoutException or HttpRequestException or IOException;
}
