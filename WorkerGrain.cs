namespace orleans_workqueue_poc;

public class WorkerGrain : Grain, IWorkerGrain
{
    /// <inheritdoc />
    public async Task DoWorkWithReporting(int workId, int workDuration, IWorkReportingGrain reportingGrain)
    {
        await Task.Delay(workDuration); // simulate slow http request
        await reportingGrain.ReportWorkCompleted(workId);
        DeactivateOnIdle();
    }

    /// <inheritdoc />
    public async Task DoWorkNoReporting(int workId, int workDuration)
    {
        await Task.Delay(workDuration);
        DeactivateOnIdle();
    }
}

public interface IWorkerGrain : IGrainWithGuidKey
{
    Task DoWorkWithReporting(int workId, int workDuration, IWorkReportingGrain reportingGrain);
    Task DoWorkNoReporting(int workId, int workDuration);
}