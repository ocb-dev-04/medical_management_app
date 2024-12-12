using Shared.Consul.Configuration;
using Service.Diagnoses.Persistence;
using Service.Diagnoses.Api.Extensions;
using Shared.Global.Sources.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServices();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseConsultServiceRegistry();
app.UseCustomHealthChecks();
app.UseResponseCompression();

app.UseRouting();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
