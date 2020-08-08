using System;
using System.Linq;
using System.Text;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class DiffSerializer
    {
        private string _name;
        private Guid? _id;
        private string _statusMessage;
        private bool _selfCheckPassed;
        private bool _serviceModeEnabled;
        private ulong _uptimeInSeconds;
        private double _pressure;
        private double _temperature;
        private double _distance;

        public byte[] Serialize(IotDevice obj)
        {
            var bytes = SerializeType(obj);
            return bytes;
        }

        public IotDevice Deserialize(byte[] data)
        {
            var instance = DeserializeType(data);
            return instance;
        }

        private IotDevice DeserializeType(byte[] data)
        {
            var bitReader = new BitReader(data);

            var statusMessageChanged = bitReader.ReadBit();
            var pressureChanged = bitReader.ReadBit();
            var temperatureChanged = bitReader.ReadBit();
            var distanceChanged = bitReader.ReadBit();

            _name = _name is object ? _name : DeserializeString(bitReader, length: 12);
            _id = _id is object ? _id : DeserializeGuid(bitReader);
            _statusMessage = statusMessageChanged ? _statusMessage : DeserializeString(bitReader, length: 10);
            _selfCheckPassed = DeserializeBoolean(bitReader);
            _serviceModeEnabled = DeserializeBoolean(bitReader);
            _uptimeInSeconds = _uptimeInSeconds + DeserializeUInt64(bitReader, bitLength: 7);
            _pressure = pressureChanged ? _pressure : DeserializeDouble(bitReader);
            _temperature = temperatureChanged ? _temperature : DeserializeDouble(bitReader);
            _distance = distanceChanged ? _distance : DeserializeDouble(bitReader);

            if (!_statusMessage.Contains(' '))
            {
                _statusMessage = _statusMessage.Substring(0, 3) + " " + _statusMessage.Substring(3, 4) + " " + _statusMessage.Substring(7, 3);
            }

            var instance = new IotDevice(
                _name,
                _id?.ToString("D") ?? string.Empty,
                _statusMessage,
                _selfCheckPassed,
                _serviceModeEnabled,
                _uptimeInSeconds,
                new PhysicalValue(_pressure, "bar"),
                new PhysicalValue(_temperature, "°C"),
                new PhysicalValue(_distance, "m")
            );

            return instance;
        }

        private Guid? DeserializeGuid(BitReader bitReader)
        {
            var value = bitReader.ReadBytes(16);
            return new Guid(value);
        }

        private bool DeserializeBoolean(BitReader bitReader)
        {
            var value = bitReader.ReadBit();
            return value;
        }

        private double DeserializeDouble(BitReader bitReader)
        {
            var bytes = bitReader.ReadBytes(4);
            var value = (double)BitConverter.ToSingle(bytes);
            return value;
        }

        private ulong DeserializeUInt64(BitReader bitReader, int bitLength)
        {
            var bytes = bitReader.ReadBits(bitLength);
            var bytes2 = new byte[4];

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                var x = (bytes.Length - 1 - i);
                bytes2[x] = bytes[i];
            }

            var value = (ulong)BitConverter.ToUInt32(bytes2);
            return value;
        }

        private string DeserializeString(BitReader bitReader, int length, bool onlyCharacters = true)
        {
            var bytes = bitReader.ReadBytes(length, bitLength: onlyCharacters ? 5 : 6);
            var value = FloEncoding.GetString(bytes, onlyCharacters);
            return value;
        }

        private byte[] SerializeType(IotDevice obj)
        {
            var bitWriter = new BitWriter();

            var id = Guid.Parse(obj.Id);
            var statusMessage = obj.StatusMessage.Replace(" ", string.Empty);

            bitWriter.WriteBit(string.Equals(statusMessage, _statusMessage, StringComparison.Ordinal));
            bitWriter.WriteBit(IsEqual(_pressure, obj.Pressure.Value));
            bitWriter.WriteBit(IsEqual(_temperature, obj.Temperature.Value));
            bitWriter.WriteBit(IsEqual(_distance, obj.Distance.Value));

            if (!string.Equals(obj.Name, _name, StringComparison.Ordinal))
            {
                SerializeType(bitWriter, obj.Name);
                _name = obj.Name;
            }

            if (id != _id)
            {
                SerializeType(bitWriter, id);
                _id = id;
            }

            if (!string.Equals(statusMessage, _statusMessage, StringComparison.Ordinal))
            {
                SerializeType(bitWriter, statusMessage);
                _statusMessage = statusMessage;
            }

            SerializeType(bitWriter, obj.SelfCheckPassed);

            SerializeType(bitWriter, obj.ServiceModeEnabled);

            SerializeType(bitWriter, obj.UptimeInSeconds - _uptimeInSeconds, bitLength: 7);
            _uptimeInSeconds = obj.UptimeInSeconds;

            if (!IsEqual(_pressure, obj.Pressure.Value))
            {
                SerializeType(bitWriter, obj.Pressure.Value);
                _pressure = obj.Pressure.Value;
            }

            if (!IsEqual(_temperature, obj.Temperature.Value))
            {
                SerializeType(bitWriter, obj.Temperature.Value);
                _temperature = obj.Temperature.Value;
            }

            if (!IsEqual(_distance, obj.Distance.Value))
            {
                SerializeType(bitWriter, obj.Distance.Value);
                _distance = obj.Distance.Value;
            }

            var bytes = bitWriter.ToArray();
            return bytes;
        }

        private bool IsEqual(double lhs, double rhs)
        {
            double difference = Math.Abs(lhs * .000001);
            return Math.Abs(lhs - rhs) <= difference;
        }

        private void SerializeType(BitWriter bitWriter, string value)
        {
            var bytes = FloEncoding.GetBytes(value, onlyCharacters: true);
            bitWriter.WriteBits(bytes, value.Length * 5);
        }

        private void SerializeType(BitWriter bitWriter, bool value)
        {
            bitWriter.WriteBit(value);
        }

        private void SerializeType(BitWriter bitWriter, double value)
        {
            var bytes = BitConverter.GetBytes((float)value);
            bitWriter.WriteBytes(bytes);
        }

        private void SerializeType(BitWriter bitWriter, ulong value, int bitLength)
        {
            var bytes = BitConverter.GetBytes((uint)value).Reverse().ToArray();
            bitWriter.WriteBits(bytes, bitLength, offset: 32 - bitLength);
        }

        private void SerializeType(BitWriter bitWriter, Guid value)
        {
            var bytes = value.ToByteArray();
            bitWriter.WriteBytes(bytes);
        }
    }
}