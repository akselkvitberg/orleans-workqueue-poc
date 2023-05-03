using orleans_workqueue_poc;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOrleans(builder => builder
            .UseDashboard()
            .AddMemoryGrainStorageAsDefault()
            .UseLocalhostClustering());
        services.AddHostedService<Worker>();

    })
    .Build();

host.Run();
