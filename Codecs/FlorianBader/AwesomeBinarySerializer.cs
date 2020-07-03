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
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _properties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public AwesomeBinarySerializer()
        {
        }

        public byte[] Serialize<T>(T obj)
        {
            var type = typeof(T);

            using var resultStream = new MemoryStream();

            if (!_properties.ContainsKey(type))
            {
                CacheProperties(type);
            }

            SerializeType(resultStream, type, obj);

            var bytes = resultStream.ToArray();
            return bytes;
        }

        public T Deserialize<T>(byte[] compressedData) where T : class
        {
            var type = typeof(T);

            using var inputStream = new MemoryStream(compressedData);

            var data = inputStream.ToArray();

            if (!_properties.ContainsKey(type))
            {
                CacheProperties(type);
            }

            return CreateInstance(ref data, typeof(T)) as T;
        }

        private object CreateInstance(ref byte[] data, Type type)
        {
            var index = 0;
            return CreateInstance(ref data, type, ref index);
        }

        private object CreateInstance(ref byte[] data, Type type, ref int index)
        {
            var instance = FormatterServices.GetUninitializedObject(type);
            var properties = _properties[type];

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var length = BitConverter.ToUInt16(data, index);
                    index += 2;

                    var value = Encoding.UTF8.GetString(data, index, length);
                    index += length;

                    SetPropertyValue(property, instance, value);
                }
                else if (property.PropertyType.IsClass)
                {
                    var propertyInstance = CreateInstance(ref data, property.PropertyType, ref index);
                    SetPropertyValue(property, instance, propertyInstance);
                }
                else
                {
                    object value;

                    if (property.PropertyType == typeof(bool))
                    {
                        value = BitConverter.ToBoolean(data, index);
                        index += 1;
                    }
                    else if (property.PropertyType == typeof(char))
                    {
                        value = BitConverter.ToChar(data, index);
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        value = BitConverter.ToDouble(data, index);
                        index += 8;
                    }
                    else if (property.PropertyType == typeof(short))
                    {
                        value = BitConverter.ToInt16(data, index);
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        value = BitConverter.ToInt32(data, index);
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        value = BitConverter.ToInt64(data, index);
                        index += 8;
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        value = BitConverter.ToSingle(data, index);
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        value = BitConverter.ToUInt16(data, index);
                        index += 2;
                    }
                    else if (property.PropertyType == typeof(uint))
                    {
                        value = BitConverter.ToUInt32(data, index);
                        index += 4;
                    }
                    else if (property.PropertyType == typeof(ulong))
                    {
                        value = BitConverter.ToUInt64(data, index);
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
                // hacky
                var backingField = property.DeclaringType.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                if (backingField is object)
                {
                    backingField.SetValue(instance, value);
                }
            }
        }

        private void CacheProperties(Type type)
        {
            var properties =
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(t => t.Name)
                .ToArray();

            foreach (var property in properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !_properties.ContainsKey(property.PropertyType))
                {
                    CacheProperties(property.PropertyType);
                }
            }

            _properties.AddOrUpdate(type, properties, (t, p) => p);
        }

        private void SerializeType(Stream stream, Type type, object obj)
        {
            var properties = _properties[type];
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(obj) as string;
                    var bytes = Encoding.UTF8.GetBytes(value);

                    var lengthBytes = BitConverter.GetBytes((ushort)bytes.Length);
                    stream.Write(lengthBytes, 0, lengthBytes.Length);

                    stream.Write(bytes, 0, bytes.Length);
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