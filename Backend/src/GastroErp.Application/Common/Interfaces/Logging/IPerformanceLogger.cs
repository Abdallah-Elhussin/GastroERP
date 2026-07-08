namespace GastroErp.Application.Common.Interfaces.Logging;

public interface IPerformanceLogger
{
    void LogSlowQuery(string queryName, long elapsedMilliseconds);
    void LogSlowRequest(string requestName, long elapsedMilliseconds);
}
