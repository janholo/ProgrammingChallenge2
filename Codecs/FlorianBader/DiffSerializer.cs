using System;
using System.Linq;
using System.Text;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class DiffSerializer
    {
        private string _name;
        private string _id;
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

            var statusByte = bitReader.ReadBytes(1)[0];

            _name = BitOperations.GetBit(statusByte, 0) ? _name : DeserializeString(bitReader, length: 12, onlyCharacters: true);
            _id = BitOperations.GetBit(statusByte, 0) ? _id : DeserializeString(bitReader, length: 32);
            _statusMessage = BitOperations.GetBit(statusByte, 1) ? _statusMessage : DeserializeString(bitReader, length: 10, onlyCharacters: true);
            _selfCheckPassed = BitOperations.GetBit(statusByte, 2) ? _selfCheckPassed : DeserializeBoolean(bitReader);
            _serviceModeEnabled = BitOperations.GetBit(statusByte, 3) ? _serviceModeEnabled : DeserializeBoolean(bitReader);
            _uptimeInSeconds = BitOperations.GetBit(statusByte, 4) ? _uptimeInSeconds : DeserializeUInt64(bitReader, bitLength: 23);
            _pressure = BitOperations.GetBit(statusByte, 5) ? _pressure : DeserializeDouble(bitReader, bitLength: 24);
            _temperature = BitOperations.GetBit(statusByte, 6) ? _temperature : DeserializeDouble(bitReader, bitLength: 27);
            _distance = BitOperations.GetBit(statusByte, 7) ? _distance : DeserializeDouble(bitReader, bitLength: 30);

            _id = Guid.Parse(_id).ToString("D");

            if (!_statusMessage.Contains(' '))
            {
                _statusMessage = _statusMessage.Substring(0, 3) + " " + _statusMessage.Substring(3, 4) + " " + _statusMessage.Substring(7, 3);
            }

            var instance = new IotDevice(
                _name,
                _id,
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

        private bool DeserializeBoolean(BitReader bitReader)
        {
            var value = bitReader.ReadBit();
            return value;
        }

        private double DeserializeDouble(BitReader bitReader, int bitLength)
        {
            var bytes = bitReader.ReadBits(bitLength);
            var bytes2 = new byte[4];

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                var x = (bytes.Length - 1 - i);
                bytes2[x] = bytes[i];
            }

            var value = (double)BitConverter.ToInt32(bytes2) / 1_000_000;
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

        private string DeserializeString(BitReader bitReader, int length, bool onlyCharacters = false)
        {
            var bytes = bitReader.ReadBytes(length, bitLength: onlyCharacters ? 5 : 6);
            var value = FloEncoding.GetString(bytes, onlyCharacters);
            return value;
        }

        private byte[] SerializeType(IotDevice obj)
        {
            var bitWriter = new BitWriter();

            var id = Guid.Parse(obj.Id).ToString("N");
            var statusMessage = obj.StatusMessage.Replace(" ", string.Empty);

            bitWriter.WriteBit(string.Equals(obj.Name, _name, StringComparison.Ordinal) && string.Equals(id, _id, StringComparison.Ordinal));
            bitWriter.WriteBit(string.Equals(statusMessage, _statusMessage, StringComparison.Ordinal));
            bitWriter.WriteBit(_selfCheckPassed == obj.SelfCheckPassed);
            bitWriter.WriteBit(_serviceModeEnabled == obj.ServiceModeEnabled);
            bitWriter.WriteBit(_uptimeInSeconds == obj.UptimeInSeconds);
            bitWriter.WriteBit(IsEqual(_pressure, obj.Pressure.Value));
            bitWriter.WriteBit(IsEqual(_temperature, obj.Temperature.Value));
            bitWriter.WriteBit(IsEqual(_distance, obj.Distance.Value));

            if (!string.Equals(obj.Name, _name, StringComparison.Ordinal))
            {
                SerializeType(bitWriter, obj.Name, onlyCharacters: true);
                _name = obj.Name;
            }

            if (!string.Equals(id, _id, StringComparison.Ordinal))
            {
                SerializeType(bitWriter, id);
                _id = id;
            }

            if (!string.Equals(statusMessage, _statusMessage, StringComparison.Ordinal))
            {
                SerializeType(bitWriter, statusMessage, onlyCharacters: true);
                _statusMessage = statusMessage;
            }

            if (_selfCheckPassed != obj.SelfCheckPassed)
            {
                SerializeType(bitWriter, obj.SelfCheckPassed);
                _selfCheckPassed = obj.SelfCheckPassed;
            }

            if (_serviceModeEnabled != obj.ServiceModeEnabled)
            {
                SerializeType(bitWriter, obj.ServiceModeEnabled);
                _serviceModeEnabled = obj.ServiceModeEnabled;
            }

            if (_uptimeInSeconds != obj.UptimeInSeconds)
            {
                SerializeType(bitWriter, obj.UptimeInSeconds, bitLength: 23);
                _uptimeInSeconds = obj.UptimeInSeconds;
            }

            if (!IsEqual(_pressure, obj.Pressure.Value))
            {
                SerializeType(bitWriter, obj.Pressure.Value, bitLength: 24);
                _pressure = obj.Pressure.Value;
            }

            if (!IsEqual(_temperature, obj.Temperature.Value))
            {
                SerializeType(bitWriter, obj.Temperature.Value, bitLength: 27);
                _temperature = obj.Temperature.Value;
            }

            if (!IsEqual(_distance, obj.Distance.Value))
            {
                SerializeType(bitWriter, obj.Distance.Value, bitLength: 30);
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

        private void SerializeType(BitWriter bitWriter, string value, bool onlyCharacters = false)
        {
            var bytes = FloEncoding.GetBytes(value, onlyCharacters);
            bitWriter.WriteBits(bytes, value.Length * (onlyCharacters ? 5 : 6));
        }

        private void SerializeType(BitWriter bitWriter, bool value)
        {
            bitWriter.WriteBit(value);
        }

        private void SerializeType(BitWriter bitWriter, double value, int bitLength)
        {
            var number = (int)(value * 1_000_000);
            var bytes = BitConverter.GetBytes(number).Reverse().ToArray();
            bitWriter.WriteBits(bytes, bitLength, offset: 32 - bitLength);
        }

        private void SerializeType(BitWriter bitWriter, ulong value, int bitLength)
        {
            var bytes = BitConverter.GetBytes((uint)value).Reverse().ToArray();
            bitWriter.WriteBits(bytes, bitLength, offset: 32 - bitLength);
        }
    }
}