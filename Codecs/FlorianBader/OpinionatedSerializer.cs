using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class OpinionatedSerializer
    {
        public byte[] Serialize(IotDevice obj)
        {
            var bytes = SerializeType(obj);
            return bytes;
        }

        public IotDevice Deserialize(byte[] data)
        {
            var span = new ReadOnlySpan<byte>(data);

            var instance = CreateInstance(span);
            return instance;
        }

        private IotDevice CreateInstance(in ReadOnlySpan<byte> data)
        {
            var index = 0;
            var name = DeserializeString(data, ref index);
            var id = DeserializeString(data, ref index);
            var statusMessage = DeserializeString(data, ref index);

            var selfCheckPassed = DeserializeBoolean(data, ref index, 0);
            index -= 1;

            var serviceModeEnabled = DeserializeBoolean(data, ref index, 1);

            var uptimeInSeconds = DeserializeUInt64(data, ref index);
            var pressure = DeserializeDouble(data, ref index);
            var temperature = DeserializeDouble(data, ref index);
            var distance = DeserializeDouble(data, ref index);

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

        private bool DeserializeBoolean(in ReadOnlySpan<byte> data, ref int index, int subindex)
        {
            var b = data.Slice(index, 1)[0];
            var value = (b & (1 << subindex)) != 0;
            index += 1;
            return value;
        }

        private double DeserializeDouble(in ReadOnlySpan<byte> data, ref int index)
        {
            var value = BitConverter.ToDouble(data.Slice(index, 8));
            index += 8;
            return value;
        }

        private ulong DeserializeUInt64(in ReadOnlySpan<byte> data, ref int index)
        {
            var value = BitConverter.ToUInt64(data.Slice(index, 8));
            index += 8;
            return value;
        }

        private string DeserializeString(in ReadOnlySpan<byte> data, ref int index)
        {
            var bytes = data.Slice(index);
            var length = bytes.IndexOf((byte)0);

            var value = Encoding.UTF8.GetString(bytes.Slice(0, length));
            index += length + 1; // skip the terminator

            return value;
        }

        private byte[] SerializeType(IotDevice obj)
        {
            using var stream = new MemoryStream();

            SerializeType(stream, obj.Name);
            SerializeType(stream, obj.Id);
            SerializeType(stream, obj.StatusMessage);
            SerializeType(stream, obj.SelfCheckPassed, obj.ServiceModeEnabled);
            SerializeType(stream, obj.UptimeInSeconds);
            SerializeType(stream, obj.Pressure.Value);
            SerializeType(stream, obj.Temperature.Value);
            SerializeType(stream, obj.Distance.Value);

            var bytes = stream.ToArray();
            return bytes;
        }

        private void SerializeType(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);

            // null terminator
            stream.WriteByte((byte)0);
        }

        private void SerializeType(Stream stream, params bool[] values)
        {
            if (values.Length > 8)
            {
                throw new ArgumentException("Only eight values allowed");
            }

            var value = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    value |= 1 << i;
                }
            }

            stream.WriteByte((byte)value);
        }

        private void SerializeType(Stream stream, object value)
        {
            var bytes = value switch
            {
                bool b => BitConverter.GetBytes(b),
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

            stream.Write(bytes, 0, bytes.Length);
        }
    }
}