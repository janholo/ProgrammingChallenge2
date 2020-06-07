
using System;

namespace ProgrammingChallenge2.Model
{
    // The model of the iot device, this is the state which needs to be send/synchronized from the device to the cloud
    public class IotDevice
    {
        public IotDevice(string name, string id, string statusMessage, bool selfCheckPassed, bool serviceModeEnabled, ulong uptimeInSeconds, PhysicalValue pressure, PhysicalValue temperature, PhysicalValue distance)
        {
            Name = name;
            Id = id;
            StatusMessage = statusMessage;
            SelfCheckPassed = selfCheckPassed;
            ServiceModeEnabled = serviceModeEnabled;
            UptimeInSeconds = uptimeInSeconds;
            Pressure = pressure;
            Temperature = temperature;
            Distance = distance;
        }

        public string Name { get; }
        public string Id { get; }
        public string StatusMessage { get; }
        public bool SelfCheckPassed { get; }
        public bool ServiceModeEnabled { get; }
        public ulong UptimeInSeconds { get; }
        public PhysicalValue Pressure { get; }
        public PhysicalValue Temperature { get; }
        public PhysicalValue Distance { get; }

        public override string ToString()
        {
            return 
                $"Name: {Name}" + Environment.NewLine +
                $"Id: {Id}" + Environment.NewLine +
                $"StatusMessage: {StatusMessage}" + Environment.NewLine +
                $"SelfCheckPassed: {SelfCheckPassed}" + Environment.NewLine +
                $"ServiceModeEnabled: {ServiceModeEnabled}" + Environment.NewLine +
                $"UptimeInSeconds: {UptimeInSeconds}" + Environment.NewLine +
                $"Pressure: {Pressure}" + Environment.NewLine +
                $"Temperature: {Temperature}" + Environment.NewLine +
                $"Distance: {Distance}";
        }

        public static bool AreEquals(IotDevice lhs, IotDevice rhs, bool debug)
        {
            _ = lhs ?? throw new ArgumentNullException(nameof(lhs));
            _ = rhs ?? throw new ArgumentNullException(nameof(rhs));

            if(!CheckString(lhs.Name, rhs.Name, "Name", debug)) 
            {
                return false;
            }

            if(!CheckString(lhs.Id, rhs.Id, "Id", debug)) 
            {
                return false;
            }

            if(!CheckString(lhs.StatusMessage, rhs.StatusMessage, "StatusMessage", debug)) 
            {
                return false;
            }

            if(!CheckBool(lhs.SelfCheckPassed, rhs.SelfCheckPassed, "SelfCheckPassed", debug)) 
            {
                return false;
            }

            if(!CheckBool(lhs.ServiceModeEnabled, rhs.ServiceModeEnabled, "ServiceModeEnabled", debug)) 
            {
                return false;
            }

            if(!CheckUlong(lhs.UptimeInSeconds, rhs.UptimeInSeconds, "UptimeInSeconds", debug)) 
            {
                return false;
            }

            if(!CheckPhysicalValue(lhs.Pressure, rhs.Pressure, "Pressure", debug)) 
            {
                return false;
            }

            if(!CheckPhysicalValue(lhs.Temperature, rhs.Temperature, "Temperature", debug)) 
            {
                return false;
            }

            if(!CheckPhysicalValue(lhs.Distance, rhs.Distance, "Distance", debug)) 
            {
                return false;
            }

            return true;
        }

        private static bool CheckString(string lhs, string rhs, string propertyName, bool debug)
        {
            if(string.Equals(lhs, rhs, StringComparison.Ordinal)) 
            {
                return true;
            }  

            if(debug) 
            {
                Console.WriteLine($"Property '{propertyName}' is not equal: {lhs} != {rhs}");
            }

            return false;          
        }

        private static bool CheckBool(bool lhs, bool rhs, string propertyName, bool debug)
        {
            if(lhs == rhs)
            {
                return true;
            }  

            if(debug) 
            {
                Console.WriteLine($"Property '{propertyName}' is not equal: {lhs} != {rhs}");
            }

            return false;          
        }

        private static bool CheckUlong(ulong lhs, ulong rhs, string propertyName, bool debug)
        {
            if(lhs == rhs)
            {
                return true;
            }

            if(debug) 
            {
                Console.WriteLine($"Property '{propertyName}' is not equal: {lhs} != {rhs}");
            }

            return false;          
        }

        private static bool CheckPhysicalValue(PhysicalValue lhs, PhysicalValue rhs, string propertyName, bool debug)
        {
            double difference = Math.Abs(lhs.Value * .000001);

            if(string.Equals(lhs.Unit, rhs.Unit, StringComparison.Ordinal) && Math.Abs(lhs.Value - rhs.Value) <= difference)
            {
                return true;
            }  

            if(debug) 
            {
                Console.WriteLine($"Property '{propertyName}' is not equal: {lhs} != {rhs}");
            }

            return false;          
        }
    }
}