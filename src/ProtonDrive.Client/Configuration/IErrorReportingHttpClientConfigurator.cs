using System.Net.Http;

namespace ProtonDrive.Client.Configuration;

public interface IErrorReportingHttpClientConfigurator
{
    HttpClientHandler CreateHttpMessageHandler();

    void ConfigureHttpClient(HttpClient httpClient);
}
