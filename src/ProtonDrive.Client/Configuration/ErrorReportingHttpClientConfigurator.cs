using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using ProtonDrive.Shared.Net.Http;

namespace ProtonDrive.Client.Configuration;

internal sealed class ErrorReportingHttpClientConfigurator : IErrorReportingHttpClientConfigurator
{
    private readonly IServiceProvider _provider;
    private readonly string _locale;

    public ErrorReportingHttpClientConfigurator(IServiceProvider provider, string locale)
    {
        _provider = provider;
        _locale = locale;
    }

    public HttpClientHandler CreateHttpMessageHandler()
    {
        return new HttpClientHandler()
            .AddAutomaticDecompression()
            .ConfigureCookies(_provider)
            .AddTlsPinning(ApiClientConfigurator.ErrorReportHttpClientName, _provider);
    }

    public void ConfigureHttpClient(HttpClient httpClient)
    {
        var config = _provider.GetRequiredService<DriveApiConfig>();

        httpClient.DefaultRequestHeaders.AddApiRequestHeaders(config, _locale);
    }
}
