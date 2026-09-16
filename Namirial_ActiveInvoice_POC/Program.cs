using Namirial_ActiveInvoice_POC;
using Namirial_ActiveInvoice_POC.ElectronicInvoice;
using Namirial_ActiveInvoice_POC.SolutionDOC_Hub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddValidatedOptionsForSection<InvoiceWsOptions, InvoiceWsOptionsValidator>(
    builder.Configuration.GetRequiredSection(InvoiceWsOptions.SectionName));

builder.Services.AddSingleton<ActiveInvoiceWsClientFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Namirial ActiveInvoice POC v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();


app.MapGet("/contattohub", async () =>
    {
        var clientService = app.Services.GetRequiredService<ActiveInvoiceWsClientFactory>();
        var client = clientService.CreateClient();

        var result = await client.ContattoHubAsync();

        return result;
    })
    .WithName("ContattoHub");

app.MapGet("/getVersionHub", async () =>
    {
        var clientService = app.Services.GetRequiredService<ActiveInvoiceWsClientFactory>();
        var client = clientService.CreateClient();

        var result = await client.GetVersionHubAsync();

        return result;
    })
    .WithName("GetVersionHub");

app.MapGet("/InsertDraftElectronicInvoice", async (CancellationToken ct) =>
    {
        var clientService = app.Services.GetRequiredService<ActiveInvoiceWsClientFactory>();
        var auth = clientService.CreateAuth();

        var xmlPath = Path.Combine(AppContext.BaseDirectory, "ElectronicInvoice", "IT59502613140_0000000003.xml");
        byte[] xmlBytes = await File.ReadAllBytesAsync(xmlPath);

        var request = new UploadElectronicInvoiceRequest
        {
            UploadingFiles =
            [
                new UploadElectronicInvoiceFileRequest
                {
                    StateInvoice = "Authorized",
                    FileName = "IT59502613140_0000000003.xml",
                    Content = xmlBytes
                }
            ]
        };

        var result = await clientService.ExecuteAsync(
            client => client.InsertDraftElectronicInvoiceAsync(auth, request),
            operationName: nameof(SolutionDOC_HubSoapClient.InsertDraftElectronicInvoiceAsync), ct: ct);

        return result;
    })
    .WithName("InsertDraftElectronicInvoice");

app.MapGet("/SendInvoice", async (string transactionId, CancellationToken ct) =>
    {
        var clientService = app.Services.GetRequiredService<ActiveInvoiceWsClientFactory>();
        var auth = clientService.CreateAuth();

       

        var result = await clientService.ExecuteAsync(
            client => client.SendElectronicInvoiceAsync(auth, new SendInvoice()
            {
                IdTransaction = transactionId,
                ToSign = true,
                Filename = "IT59502613140_0000000003.xml"
            }),
            operationName: nameof(SolutionDOC_HubSoapClient.InsertDraftElectronicInvoiceAsync), ct: ct);

        return result;
    })
    .WithName("SendInvoice");

app.Run();