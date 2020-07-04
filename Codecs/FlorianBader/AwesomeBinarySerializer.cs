using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class AwesomeBinarySerializer
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _serializableTypes = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public byte[] Serialize<T>(T obj)
        {
            var type = typeof(T);

            using var resultStream = new MemoryStream();

            CacheProperties(type);

            SerializeType(resultStream, type, obj);

            var bytes = resultStream.ToArray();
            return bytes;
        }

        public T Deserialize<T>(byte[] data) where T : class
        {
            var type = typeof(T);

            CacheProperties(type);

            var span = new ReadOnlySpan<byte>(data);

            var instance = CreateInstance(span, typeof(T)) as T;
            return instance;
        }

        private object CreateInstance(in ReadOnlySpan<byte> data, Type type)
        {
            var index = 0;
            return CreateInstance(data, type, ref index);
        }

        private object CreateInstance(in ReadOnlySpan<byte> data, Type type, ref int index)
        {
            // hacky, better solution would be to find the correct constructor
            var instance = FormatterServices.GetUninitializedObject(type);
            var properties = _serializableTypes[type];

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var bytes = data.Slice(index);
                    var length = bytes.IndexOf((byte)0);

                    var value = Encoding.UTF8.GetString(bytes.Slice(0, length));
                    index += length + 1; // skip the terminator

                    SetPropertyValue(property, instance, value);
                }
                else if (property.PropertyType.IsClass)
                {
                    var propertyInstance = CreateInstance(data, property.PropertyType, ref index);
                    SetPropertyValue(property, instance, propertyInstance);
                }
                else
                {
                    object value;

                    if (property.PropertyType == typeof(bool))
                    {
                        value = BitConverter.ToBoolean(data.Slice(index, 1));
                        index += 1;
                    }
                    else if (property.PropertyType == typeof(char))
                    {
                        value = BitConverter.ToChar(data.Slice(index, 2));
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        value = BitConverter.ToDouble(data.Slice(index, 8));
                        index += 8;
                    }
                    else if (property.PropertyType == typeof(short))
                    {
                        value = BitConverter.ToInt16(data.Slice(index, 2));
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        value = BitConverter.ToInt32(data.Slice(index, 4));
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        value = BitConverter.ToInt64(data.Slice(index, 8));
                        index += 8;
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        value = BitConverter.ToSingle(data.Slice(index, 4));
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        value = BitConverter.ToUInt16(data.Slice(index, 2));
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(uint))
                    {
                        value = BitConverter.ToUInt32(data.Slice(index, 4));
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(ulong))
                    {
                        value = BitConverter.ToUInt64(data.Slice(index, 8));
                        index += 8;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Type of {property.PropertyType.FullName} not supported.");
                    }

                    SetPropertyValue(property, instance, value);
                }
            }

            return instance;
        }

        private void SetPropertyValue(PropertyInfo property, object instance, object value)
        {
            if (property.CanWrite)
            {
                property.SetValue(instance, value);
            }
            else
            {
                // hacky, better solution would be to find the correct constructor
                var backingField = property.DeclaringType.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                if (backingField is object)
                {
                    backingField.SetValue(instance, value);
                }
            }
        }

        private void CacheProperties(Type type)
        {
            if (!_serializableTypes.ContainsKey(type))
            {
                var properties =
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(t => t.Name)
                    .ToArray();

                foreach (var property in properties)
                {
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !_serializableTypes.ContainsKey(property.PropertyType))
                    {
                        CacheProperties(property.PropertyType);
                    }
                }

                _serializableTypes.AddOrUpdate(type, properties, (t, p) => p);
            }
        }

        private void SerializeType(Stream stream, Type type, object obj)
        {
            var properties = _serializableTypes[type];
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(obj) as string;
                    var bytes = Encoding.UTF8.GetBytes(value);

                    stream.Write(bytes, 0, bytes.Length);
                    stream.WriteByte((byte)0);
                }
                else if (property.PropertyType.IsClass)
                {
                    var propertyObj = property.GetValue(obj);
                    SerializeType(stream, property.PropertyType, propertyObj);
                }
                else
                {
                    var value = property.GetValue(obj);

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
                        _ => throw new InvalidOperationException($"Type of {property.PropertyType.FullName} not supported.")
                    };

                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}