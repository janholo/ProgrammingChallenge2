using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public interface IDataSource
    {
        IotDevice GetNextDataPoint();
    }
}