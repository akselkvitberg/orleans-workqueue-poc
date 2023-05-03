namespace orleans_workqueue_poc;

public class Worker : BackgroundService
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IGrainFactory grainFactory, ILogger<Worker> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var deadlockGrain = _grainFactory.GetGrain<ITopicGrainWithRateLimit>("foobar");

        var counter = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var workDuration = Random.Shared.Next(0, 5000);
            
            await deadlockGrain.AddWork(counter, workDuration); // Simulates many requests coming in

            counter++;
        }
    }
}
