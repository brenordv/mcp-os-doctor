using McpOsDoctor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace McpOsDoctor.Platform.Windows.Extensions;

/// <summary>
/// Extension methods for registering Windows platform providers with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Windows platform provider implementations as singletons
    /// against their abstraction interfaces.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWindowsPlatform(this IServiceCollection services)
    {
        services.AddSingleton<IPrivilegeChecker, WindowsPrivilegeChecker>();
        services.AddSingleton<IOutputSanitizer, OutputSanitizer>();
        services.AddSingleton<ISystemLogReader, WindowsEventLogReader>();
        services.AddSingleton<IServiceInspector, WindowsServiceInspector>();
        services.AddSingleton<IProcessInspector, WindowsProcessInspector>();
        services.AddSingleton<ISystemInfoProvider, WindowsSystemInfoProvider>();
        services.AddSingleton<IPerformanceReader, WindowsPerformanceReader>();
        services.AddSingleton<IBootHistoryProvider, WindowsBootHistoryProvider>();
        services.AddSingleton<IGpuInfoProvider, NvidiaSmiGpuInfoProvider>();
        services.AddSingleton<IDirectXInfoProvider, DxDiagDirectXInfoProvider>();
        services.AddSingleton<ISensorMonitor, LibreHardwareSensorMonitor>();

        return services;
    }
}