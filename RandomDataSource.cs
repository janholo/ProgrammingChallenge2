using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public class RandomDataSource : IDataSource
    {
        private string name;
        private string id;

        public RandomDataSource()
        {
            name = "Test";
            id = Guid.NewGuid().ToString("D");
        }

        public IotDevice GetNextDataPoint()
        {
            return new IotDevice(
                name,
                id,
                "Msg",
                true,
                false,
                true,
                12,
                new PhysicalValue(12.3, "bar"),
                new PhysicalValue(83.2, "°C"),
                new PhysicalValue(0.234, "m"),
                new PhysicalValue(1245423, "N"),
                new PhysicalValue(9.81, "m/s^2")
            );
        }
    }
}