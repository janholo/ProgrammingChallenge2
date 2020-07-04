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
            var selfCheckPassed = DeserializeBoolean(data, ref index);
            var serviceModeEnabled = DeserializeBoolean(data, ref index);
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

        private bool DeserializeBoolean(in ReadOnlySpan<byte> data, ref int index)
        {
            var value = BitConverter.ToBoolean(data.Slice(index, 1));
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
            var length = BitConverter.ToUInt16(data.Slice(index, 2));
            index += 2;

            var value = Encoding.UTF8.GetString(data.Slice(index, length));
            index += length;

            return value;
        }

        private byte[] SerializeType(IotDevice obj)
        {
            using var stream = new MemoryStream();

            SerializeType(stream, obj.Name);
            SerializeType(stream, obj.Id);
            SerializeType(stream, obj.StatusMessage);
            SerializeType(stream, obj.SelfCheckPassed);
            SerializeType(stream, obj.ServiceModeEnabled);
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

            var lengthBytes = BitConverter.GetBytes((ushort)bytes.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);

            stream.Write(bytes, 0, bytes.Length);
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