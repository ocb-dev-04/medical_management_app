using Services.Auth.Persistence;
using Shared.Consul.Configuration;
using Services.Auth.Api.Extensions;
using Shared.Global.Sources.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServices();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.CheckMigrations();
}

app.Warnup();

app.UseConsultServiceRegistry();
app.UseCustomHealthChecks();
app.UseResponseCompression();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
