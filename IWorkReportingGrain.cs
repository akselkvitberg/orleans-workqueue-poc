namespace orleans_workqueue_poc;

public interface IWorkReportingGrain : IGrain
{
    Task ReportWorkCompleted(int workId);
}