using System;
using System.Linq;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public class DataSource : IDataSource
    {
        private string name;
        private string id;
        private ulong uptimeInSeconds;
        private string msg;
        private Random random;

        public DataSource()
        {
            name = "Test";
            id = Guid.NewGuid().ToString("D");
            random = new Random();
        }

        public IotDevice GetNextDataPoint()
        {
            uptimeInSeconds += (ulong)(random.NextDouble() * 100.0);

            if(msg == null || random.NextDouble() < 0.1) {
                msg = CreateRandomString();
            }

            return new IotDevice(
                name,
                id,
                msg,
                random.NextDouble() > 0.5,
                random.NextDouble() > 0.5,
                uptimeInSeconds,
                new PhysicalValue(random.NextDouble() * 5.0, "bar"),
                new PhysicalValue(random.NextDouble() * 100.0, "°C"),
                new PhysicalValue(random.NextDouble() * 1000.0, "m")
            );
        }

        private string CreateRandomString()
        {
            return new string(Enumerable.Repeat('a', 12).Select(s => (char)((int)s + random.Next(24))).ToArray());
        }
    }
}