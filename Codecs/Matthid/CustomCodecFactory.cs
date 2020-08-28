
using ProgrammingChallenge2.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProgrammingChallenge2.Codecs.Matthid
{
    public class CustomEncodingOptions {
        public CustomEncodingOptions(bool remeberLastData = true, bool optimizeString = true, bool removeSpaceFromMsg = true, bool optimizeUptime = true){
            RememberLastData = remeberLastData;
            OptimizeStrings = optimizeString;
            RemoveSpaces = removeSpaceFromMsg;
            OptimizeUptime = optimizeUptime;
        }

        // I consider this cheating / problem with the data source
        public bool RememberLastData { get; }

        // Use a custom encoder instead of UTF8
        public bool OptimizeStrings { get; }

        // Remove the spaces from the status message
        public bool RemoveSpaces { get; }

        // Send only increment.
        public bool OptimizeUptime { get; }
    }

    // Add some optimizations:
    // - Do not send unit strings
    // - Send float instead of double
    // - Guid as bytes
    // - (optional) Send uptime increment as int (2 bytes for low numbers)
    // - (optional) Msg & ID as always the same, so only send the first time and null otherwise.
    // - (optional) Improve string encoding
    // - (optional) Remove spaces
    public class CustomCodecFactory : ICodecFactory
    {
        CustomEncodingOptions options;
        public CustomCodecFactory(CustomEncodingOptions options = null) {
            this.options = options ?? new CustomEncodingOptions();
        }

        public string Name => "Custom Codec by Matthias";

        public IDecoder CreateDecoder() => new CustomDecoder(options);

        public IEncoder CreateEncoder() => new CustomEncoder(options);
    }

    public class CustomEncoder : IEncoder {
        
        CustomEncodingOptions options;
        string lastName = null;
        string lastId = null;
        string lastMsg = null;
        ulong lastUptime = 0;
        public CustomEncoder(CustomEncodingOptions options)
        {
            this.options = options;
        }

        internal void WriteString(BinaryWriter writer, string data) {
            if (options.OptimizeStrings){
                ImprovedLowerCaseStringEncoder.WriteString(writer, data);
            } else {
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                writer.Write(bytes);
            }
        }

        public byte[] Encode(IotDevice device) {
            var mem = new MemoryStream();
            var writer = new BinaryWriter(mem);

            bool writeNameAndId = true, writeMsg = true;            
            var statusMessage = options.RemoveSpaces ? device.StatusMessage?.Replace(" ", "") : device.StatusMessage;
            if (options.RememberLastData) {
                writeNameAndId = false;
                if (lastName == null || lastName != device.Name) {
                    lastName = device.Name;
                    writeNameAndId = true;
                }
                
                if (lastId == null || lastId != device.Id) {
                    lastId = device.Id;
                    writeNameAndId = true;
                }

                if (lastMsg == null || lastMsg != statusMessage) {
                    lastMsg = statusMessage;
                } else {
                    writeMsg = false;
                }

                writer.Write(writeNameAndId);
                writer.Write(writeMsg);
            }

            if (writeNameAndId) {
                Debug.Assert(device.Name.Length == 12);
                WriteString(writer, device.Name);

                var idGuid = System.Guid.ParseExact(device.Id, "D");
                writer.Write(idGuid.ToByteArray());
            }

            if (writeMsg){
                Debug.Assert(statusMessage.Length == (options.RemoveSpaces ? 10 : 12), $"Length was {device.StatusMessage.Length}");
                WriteString(writer, statusMessage);
            }

            writer.Write(device.SelfCheckPassed);
            writer.Write(device.ServiceModeEnabled);

            if (options.OptimizeUptime) {
                var diff = device.UptimeInSeconds - lastUptime;
                Debug.Assert(diff >= 0 && diff < byte.MaxValue, $"expected diff to be smaller than a byte, but was '{diff}'");
                writer.Write((byte)(diff));
                lastUptime = device.UptimeInSeconds;
            } else {
                writer.Write(device.UptimeInSeconds);
            }
            
            writer.Write((float)device.Pressure.Value);
            writer.Write((float)device.Temperature.Value);
            writer.Write((float)device.Distance.Value);

            writer.Flush();
            return mem.ToArray();
        }
    }

    public class CustomDecoder : IDecoder {
        
        CustomEncodingOptions options;
        string lastName = null;
        string lastId = null;
        string lastMsg = null;
        ulong lastUptimeTotal = 0;
        public CustomDecoder(CustomEncodingOptions options)
        {
            this.options = options;
        }

        internal string ReadString(BinaryReader reader, int count){
            if (options.OptimizeStrings) {
                return ImprovedLowerCaseStringEncoder.ReadString(reader, count);
            } else {
                var bytes = reader.ReadBytes(count);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        public IotDevice Decode(byte[] data) {
            
            var mem = new MemoryStream(data);
            var reader = new BinaryReader(mem);

            var msgLength = options.RemoveSpaces ? 10 : 12;
            String name, id, msg;
            if (options.RememberLastData) {
                var readNameAndId = reader.ReadBoolean();
                var readMsg = reader.ReadBoolean();
                if (readNameAndId){
                    lastName = ReadString(reader, 12);
                    var guidBytes = reader.ReadBytes(16);
                    lastId = new System.Guid(guidBytes).ToString("D");
                }

                if (readMsg){
                    lastMsg = ReadString(reader, msgLength);
                }

                name = lastName;
                id = lastId;
                msg = lastMsg;
            } else {
                name = ReadString(reader, 12);
                var guidBytes = reader.ReadBytes(16);
                id = new System.Guid(guidBytes).ToString("D");
                msg = ReadString(reader, msgLength);
            }
            
            if (options.RemoveSpaces) {
                // add them here
                msg = $"{msg.Substring(0, 3)} {msg.Substring(3, 4)} {msg.Substring(7)}";
            }

            var selfCheckPassed = reader.ReadBoolean();
            var serviceModeEnabled = reader.ReadBoolean();

            if (options.OptimizeUptime) {
                var uptimeDiff = (ulong)reader.ReadByte();
                lastUptimeTotal += uptimeDiff;
            } else {
                lastUptimeTotal = reader.ReadUInt64();
            }
            
            var pressure = reader.ReadSingle();
            var temperature = reader.ReadSingle();
            var distance = reader.ReadSingle();

            return new IotDevice(
                name, id, msg,
                selfCheckPassed, serviceModeEnabled, lastUptimeTotal,
                new PhysicalValue(pressure, "bar"),
                new PhysicalValue(temperature, "°C"),
                new PhysicalValue(distance, "m"));
        }
    }


}