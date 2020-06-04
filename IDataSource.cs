using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    interface IDataSource
    {
        IotDevice GetNextDataPoint();
    }
}