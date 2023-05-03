namespace orleans_workqueue_poc;

public class TopicGrain : Grain, ITopicGrain, IWorkReportingGrain
{
    private readonly ILogger<TopicGrain> _logger;
    private readonly Dictionary<int, int> _workInProgress = new();

    public TopicGrain(ILogger<TopicGrain> logger)
    {
        _logger = logger;
    }

    
    public async Task AddWorkAndWaitWithReporting(int workId, int workDuration)
    {
        _logger.LogInformation("Deadlock grain received work {WorkId} for {WorkDuration} ms", workId, workDuration);
        _workInProgress[workId] = workDuration;

        // this deadlocks the grain because IWorkerGrain tries to call this grain with work status
        await GrainFactory.GetGrain<IWorkerGrain>(Guid.NewGuid()).DoWorkWithReporting(workId, workDuration, this);
    }

    public async Task AddWorkAndWaitNoReporting(int workId, int workDuration)
    {
        //_logger.LogInformation("Deadlock grain received work {WorkId} for {WorkDuration} ms", workId, workDuration);
        _workInProgress[workId] = workDuration;

        // this will not deadlock, but it will prevent new messages from being processed until the work item has been processed
        await GrainFactory.GetGrain<IWorkerGrain>(Guid.NewGuid()).DoWorkNoReporting(workId, workDuration);
    }

    public async Task AddWorkAndIgnoreWithReporting(int workId, int workDuration)
    {
        //_logger.LogInformation("Deadlock grain received work {WorkId} for {WorkDuration} ms", workId, workDuration);
        _workInProgress[workId] = workDuration;

        // this will not deadlock, and will allow this grain to receive and start more work as soon as it can
        GrainFactory.GetGrain<IWorkerGrain>(Guid.NewGuid()).DoWorkWithReporting(workId, workDuration, this).Ignore();
    }


    /// <inheritdoc />
    public async Task ReportWorkCompleted(int workId)
    {
        //_logger.LogInformation("Deadlock grain completed work {WorkId}", workId);
        if (_workInProgress.ContainsKey(workId))
        {
            _workInProgress.Remove(workId);
        }
    }
}

public interface ITopicGrain : IGrainWithStringKey
{
    Task AddWorkAndWaitWithReporting(int workId, int workDuration);
    Task AddWorkAndWaitNoReporting(int workId, int workDuration);
    Task AddWorkAndIgnoreWithReporting(int workId, int workDuration);
}