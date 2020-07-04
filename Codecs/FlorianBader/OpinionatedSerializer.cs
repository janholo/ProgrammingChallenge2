using System;
using System.Collections;
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
            var bytes = bitReader.ReadBytes(8);
            var value = BitConverter.ToDouble(bytes);
            return value;
        }

        private ulong DeserializeUInt64(BitReader bitReader)
        {
            var bytes = bitReader.ReadBytes(8);
            var value = BitConverter.ToUInt64(bytes);
            return value;
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