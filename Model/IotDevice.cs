
using System;

namespace ProgrammingChallenge2.Model
{
    // The model of the iot device, this is the state which needs to be send/synchronized from the device to the cloud
    public class IotDevice
    {
        public IotDevice(string name, string id, string statusMessage, bool drivesEnabled, bool selfCheckPassed, bool serviceModeEnabled, ulong uptimeInSeconds, PhysicalValue pressure, PhysicalValue temperature, PhysicalValue distance, PhysicalValue force, PhysicalValue acceleration)
        {
            Name = name;
            Id = id;
            StatusMessage = statusMessage;
            DrivesEnabled = drivesEnabled;
            SelfCheckPassed = selfCheckPassed;
            ServiceModeEnabled = serviceModeEnabled;
            UptimeInSeconds = uptimeInSeconds;
            Pressure = pressure;
            Temperature = temperature;
            Distance = distance;
            Force = force;
            Acceleration = acceleration;
        }

        public string Name { get; }
        public string Id { get; }
        public string StatusMessage { get; }
        public bool DrivesEnabled { get; }
        public bool SelfCheckPassed { get; }
        public bool ServiceModeEnabled { get; }
        public ulong UptimeInSeconds { get; }
        public PhysicalValue Pressure { get; }
        public PhysicalValue Temperature { get; }
        public PhysicalValue Distance { get; }
        public PhysicalValue Force { get; }
        public PhysicalValue Acceleration { get; }

        public override string ToString()
        {
            return 
                $"Name: {Name}" + Environment.NewLine +
                $"Id: {Id}" + Environment.NewLine +
                $"StatusMessage: {StatusMessage}" + Environment.NewLine +
                $"DrivesEnabled: {DrivesEnabled}" + Environment.NewLine +
                $"SelfCheckPassed: {SelfCheckPassed}" + Environment.NewLine;

        }
    }
}