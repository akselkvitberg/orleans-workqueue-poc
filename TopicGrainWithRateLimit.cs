namespace orleans_workqueue_poc;

public class TopicGrainWithRateLimit : Grain, ITopicGrainWithRateLimit, IWorkReportingGrain
{
    private readonly ILogger<TopicGrainWithRateLimit> _logger;
    private readonly Dictionary<int, DateTime> _workInProgress = new();
    private readonly Queue<(int, int)> _workQueue = new();

    public TopicGrainWithRateLimit(ILogger<TopicGrainWithRateLimit> logger)
    {
        _logger = logger;
    }

    public async Task AddWork(int workId, int workDuration)
    {
        _workQueue.Enqueue((workId, workDuration));
        
        ManageSendTimer();
    }

    private float _rateLimitPerSecond = 10f;
    private IDisposable? _sendTimer;

    public async Task ProcessSending()
    {
        if (_workQueue.TryDequeue(out (int workId, int workDuration) work))
        {
            GrainFactory
                .GetGrain<IWorkerGrain>(Guid.NewGuid())
                .DoWorkWithReporting(work.workId, work.workDuration, this.AsReference<IWorkReportingGrain>())
                .Ignore();

            _workInProgress[work.workId] = DateTime.Now; // keep track of when it's sent
        }

        ManageSendTimer();
    }

    private void ManageSendTimer()
    {
        if (_workQueue.Count > 0 && _sendTimer == null)
        {
            var sendInterval = TimeSpan.FromMilliseconds(1000 / _rateLimitPerSecond);
            _sendTimer = RegisterTimer(_ => ProcessSending(), null, sendInterval, sendInterval);
        }
        else if(_workQueue.Count == 0 && _sendTimer != null)
        {
            _sendTimer?.Dispose();
            _sendTimer = null;
        }
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

public interface ITopicGrainWithRateLimit : IGrainWithStringKey
{
    Task AddWork(int workId, int workDuration);

    Task ProcessSending();

}