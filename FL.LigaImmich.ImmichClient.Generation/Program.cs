using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Tapio.StripeConnector.Inbound.Adapters.Api.Extensions;

namespace Tapio.StripeConnector.ApiClient.Generation;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseDefaultServiceProvider(x => x.ValidateScopes = false);
        builder.Services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(Inbound.Adapters.Api.AssemblyReference.Assembly));
        builder.Services.AddTapioSwaggerGen();

        var app = builder.Build();
        app.UseSwagger();
        app.Run();
    }
}
