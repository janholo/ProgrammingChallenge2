using System;
using System.Text;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    /// <summary>
    /// This is an opinionated serializer.
    /// We do some crazy optimizations here because we know exactly what type we want to serialize.
    /// 1. We don't serialize the physical value unit because they are fixed
    /// 2. We optimize boolean values to only use a bit instead of a whole byte
    /// 3. We reduce the precision of double and ulong
    /// </summary>
    public class OpinionatedSerializer
    {
        private readonly bool _extremeOptimization;

        public OpinionatedSerializer(bool extremeOptimization = true)
        {
            // if extreme optimizations is turned on it will convert
            // double to float (8 bytes -> 4 bytes)
            // ulong to uint (8 bytes -> 4 bytes)
            // we can only do that because we know the maximum of those values
            // and don't care about the precision
            _extremeOptimization = extremeOptimization;
        }

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

            var name = DeserializeString(bitReader);
            var id = DeserializeString(bitReader);
            var statusMessage = DeserializeString(bitReader);
            var selfCheckPassed = DeserializeBoolean(bitReader);
            var serviceModeEnabled = DeserializeBoolean(bitReader);
            var uptimeInSeconds = DeserializeUInt64(bitReader);
            var pressure = DeserializeDouble(bitReader);
            var temperature = DeserializeDouble(bitReader);
            var distance = DeserializeDouble(bitReader);

            var instance = new IotDevice(
                name,
                id,
                statusMessage,
                selfCheckPassed,
                serviceModeEnabled,
                uptimeInSeconds,
                new PhysicalValue(pressure, "bar"),
                new PhysicalValue(temperature, "°C"),
                new PhysicalValue(distance, "m")
            );

            return instance;
        }

        private bool DeserializeBoolean(BitReader bitReader)
        {
            var value = bitReader.ReadBit();
            return value;
        }

        private double DeserializeDouble(BitReader bitReader)
        {
            if (!_extremeOptimization)
            {
                var bytes = bitReader.ReadBytes(8);
                var value = BitConverter.ToDouble(bytes);
                return value;
            }
            else
            {
                var bytes = bitReader.ReadBytes(4);
                var value = (double)BitConverter.ToSingle(bytes);
                return value;
            }
        }

        private ulong DeserializeUInt64(BitReader bitReader)
        {
            if (!_extremeOptimization)
            {
                var bytes = bitReader.ReadBytes(8);
                var value = BitConverter.ToUInt64(bytes);
                return value;
            }
            else
            {
                var bytes = bitReader.ReadBytes(4);
                var value = BitConverter.ToUInt32(bytes);
                return value;

            }
        }

        private string DeserializeString(BitReader bitReader)
        {
            var length = bitReader.IndexOf((byte)0);
            var bytes = bitReader.ReadBytes(length);
            bitReader.ReadBytes(1); // skip the terminator

            var value = Encoding.UTF8.GetString(bytes);
            return value;
        }

        private byte[] SerializeType(IotDevice obj)
        {
            var bitWriter = new BitWriter();

            SerializeType(bitWriter, obj.Name);
            SerializeType(bitWriter, obj.Id);
            SerializeType(bitWriter, obj.StatusMessage);
            SerializeType(bitWriter, obj.SelfCheckPassed);
            SerializeType(bitWriter, obj.ServiceModeEnabled);
            SerializeType(bitWriter, obj.UptimeInSeconds);
            SerializeType(bitWriter, obj.Pressure.Value);
            SerializeType(bitWriter, obj.Temperature.Value);
            SerializeType(bitWriter, obj.Distance.Value);

            var bytes = bitWriter.ToArray();
            return bytes;
        }

        private void SerializeType(BitWriter bitWriter, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            bitWriter.WriteBytes(bytes);

            var x = new BitReader(bitWriter.ToArray());
            var y = x.ReadBytes(bytes.Length);
            var z = Encoding.UTF8.GetString(y);

            // null terminator
            bitWriter.WriteByte((byte)0);
        }

        private void SerializeType(BitWriter bitWriter, bool value)
        {
            bitWriter.WriteBit(value);
        }

        private void SerializeType(BitWriter bitWriter, double value)
        {
            if (!_extremeOptimization)
            {
                SerializeType(bitWriter, (object)value);
            }
            else
            {
                SerializeType(bitWriter, (object)((float)value));
            }
        }

        private void SerializeType(BitWriter bitWriter, ulong value)
        {
            if (!_extremeOptimization)
            {
                SerializeType(bitWriter, (object)value);
            }
            else
            {
                SerializeType(bitWriter, (object)((uint)value));
            }
        }

        private void SerializeType(BitWriter bitWriter, object value)
        {
            var bytes = value switch
            {
                char c => BitConverter.GetBytes(c),
                double d => BitConverter.GetBytes(d),
                short s => BitConverter.GetBytes(s),
                int i => BitConverter.GetBytes(i),
                long l => BitConverter.GetBytes(l),
                float f => BitConverter.GetBytes(f),
                ushort s => BitConverter.GetBytes(s),
                uint i => BitConverter.GetBytes(i),
                ulong l => BitConverter.GetBytes(l),
                _ => throw new InvalidOperationException($"Type of {value.GetType()} not supported.")
            };

            bitWriter.WriteBytes(bytes);
        }
    }
}