using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtonDrive.App.Services;
using ProtonDrive.Shared.Configuration;

namespace ProtonDrive.App;

internal class HostedApp : IHostedService
{
    private readonly AppConfig _config;
    private readonly AppArguments _appArguments;
    private readonly IEnumerable<IStartableService> _startableServices;
    private readonly IEnumerable<IStoppableService> _stoppableServices;
    private readonly ILogger<HostedApp> _logger;

    public HostedApp(
        AppConfig config,
        AppArguments appArguments,
        IEnumerable<IStartableService> startableServices,
        IEnumerable<IStoppableService> stoppableServices,
        ILogger<HostedApp> logger)
    {
        _config = config;
        _appArguments = appArguments;
        _startableServices = startableServices;
        _stoppableServices = stoppableServices;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=========================================================");
        _logger.LogInformation("{AppName} v{AppVersion} ({ProcessArchitecture}) started", _config.AppName, _config.AppVersion, RuntimeInformation.ProcessArchitecture);
        _logger.LogInformation("OS: {OsVersion} ({OsArchitecture})", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture);
        _logger.LogInformation("Launch mode: {LaunchMode}", _appArguments.LaunchMode);

        foreach (var startable in _startableServices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await startable.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // All stoppable app services are stopped concurrently
        await Task.WhenAll(_stoppableServices.Select(s => s.StopAsync(cancellationToken))).ConfigureAwait(false);

        _logger.LogInformation("{AppName} v{AppVersion} ({ProcessArchitecture}) exited", _config.AppName, _config.AppVersion, RuntimeInformation.ProcessArchitecture);
    }
}
