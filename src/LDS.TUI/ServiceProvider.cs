using Microsoft.Extensions.DependencyInjection;

public static class ServiceProvider
{
    public static IServiceProvider Services { get; private set; } = new ServiceCollection().BuildServiceProvider();
    public static void Initializer(IServiceProvider services) => Services = services;
}