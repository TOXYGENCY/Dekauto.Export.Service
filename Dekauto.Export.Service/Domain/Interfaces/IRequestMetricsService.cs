namespace Dekauto.Export.Service.Domain.Interfaces
{
    public interface IRequestMetricsService
    {
        void Increment();
        List<int> GetRecentCounters();
    }
}
