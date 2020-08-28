
using ProgrammingChallenge2.Model;
using Google.Protobuf;
using System;
using System.IO;
using System.Text;

namespace ProgrammingChallenge2.Codecs.Matthid
{
    public class ProtobufEncodingOptions {
        public ProtobufEncodingOptions(bool remeberLastData = true, bool optimizeString = true, bool removeSpaceFromMsg = true){
            RememberLastData = remeberLastData;
            OptimizeStrings = optimizeString;
            RemoveSpaces = removeSpaceFromMsg;
        }

        // I consider this cheating / problem with the data source
        public bool RememberLastData { get; }

        // Use a custom encoder instead of UTF8
        public bool OptimizeStrings { get; }

        // Remove the spaces from the status message
        public bool RemoveSpaces { get; }
    }

    // Add some optimizations:
    // - Do not send unit strings
    // - Send float instead of double
    // - Guid as bytes
    // - Send uptime increment as int (2 bytes for low numbers)
    // - (optional) Msg & ID as always the same, so only send the first time and null otherwise.
    // - (optional) Improve string encoding
    // - (optional) Remove spaces
    public class ProtobufCodecFactory : ICodecFactory
    {
        ProtobufEncodingOptions options;
        public ProtobufCodecFactory(ProtobufEncodingOptions options = null) {
            this.options = options ?? new ProtobufEncodingOptions();
        }

        public string Name => "Protobuf Codec by Matthias";

        public IDecoder CreateDecoder() => new ProtobufDecoder(options);

        public IEncoder CreateEncoder() => new ProtobufEncoder(options);
    }

    public class ProtobufEncoder : IEncoder {
        
        ProtobufEncodingOptions options;
        string lastName = null;
        string lastId = null;
        string lastMsg = null;
        ulong lastUptime = 0;
        public ProtobufEncoder(ProtobufEncodingOptions options)
        {
            this.options = options;
        }

        public ByteString EncodeString(string data) {
            if (options.OptimizeStrings){
                var bytes = ImprovedLowerCaseStringEncoder.EncodeString(data);
                return ByteString.CopyFrom(bytes);
            } else {
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                return ByteString.CopyFrom(bytes);
            }
        }

        public byte[] Encode(IotDevice device) {
            var protoMsg = new ProgrammingChallenge2.Codecs.Matthid.Protobuf.OptimizedIoTDevice();

            var statusMessage = options.RemoveSpaces ? device.StatusMessage?.Replace(" ", "") : device.StatusMessage;
            if (options.RememberLastData) {
                if (lastName == null || lastName != device.Name) {
                    lastName = device.Name;
                    protoMsg.Name = EncodeString(device.Name);
                }
                
                if (lastId == null || lastId != device.Id) {
                    lastId = device.Id;
                    var idGuid = System.Guid.ParseExact(device.Id, "D");
                    protoMsg.Id = ByteString.CopyFrom(idGuid.ToByteArray());
                }

                if (lastMsg == null || lastMsg != statusMessage) {
                    lastMsg = statusMessage;
                    protoMsg.StatusMessage = EncodeString(statusMessage);
                }
            } else {
                protoMsg.Name = EncodeString(device.Name);
                var idGuid = System.Guid.ParseExact(device.Id, "D");
                protoMsg.Id = ByteString.CopyFrom(idGuid.ToByteArray());
                protoMsg.StatusMessage = EncodeString(statusMessage);
            }

            protoMsg.SelfCheckPassed = device.SelfCheckPassed;
            protoMsg.ServiceModeEnabled = device.ServiceModeEnabled;


            protoMsg.AdditionalUptimeInSeconds = (uint)(device.UptimeInSeconds - lastUptime);
            lastUptime = device.UptimeInSeconds;
            
            protoMsg.Pressure = (float)device.Pressure.Value;
            protoMsg.Temperature = (float)device.Temperature.Value;
            protoMsg.Distance = (float)device.Distance.Value;

            return protoMsg.ToByteArray();
        }
    }

    public class ProtobufDecoder : IDecoder {
        
        ProtobufEncodingOptions options;
        string lastName = null;
        string lastId = null;
        string lastMsg = null;
        ulong lastUptimeTotal = 0;
        public ProtobufDecoder(ProtobufEncodingOptions options)
        {
            this.options = options;
        }

        public string DecodeString(ByteString data){
            if (options.OptimizeStrings) {
                return ImprovedLowerCaseStringEncoder.DecodeBytes(data.ToByteArray());
            } else {
                return System.Text.Encoding.UTF8.GetString(data.ToByteArray());
            }
        }

        public IotDevice Decode(byte[] data) {
            var protoMsg = ProgrammingChallenge2.Codecs.Matthid.Protobuf.OptimizedIoTDevice.Parser.ParseFrom(data);
            
            String name, id, msg;
            if (options.RememberLastData) {
                if (protoMsg.Name != null && protoMsg.Name.Length > 0){
                    lastName = DecodeString(protoMsg.Name);
                }
                
                if (protoMsg.Id != null && protoMsg.Id.Length == 16){
                    lastId = new System.Guid(protoMsg.Id.ToByteArray()).ToString("D");
                }

                if (protoMsg.StatusMessage != null && protoMsg.StatusMessage.Length > 0){
                    lastMsg = DecodeString(protoMsg.StatusMessage);
                }

                name = lastName;
                id = lastId;
                msg = lastMsg;
            } else {
                name = DecodeString(protoMsg.Name);
                id = new System.Guid(protoMsg.Id.ToByteArray()).ToString("D");
                msg = DecodeString(protoMsg.StatusMessage);
            }
            
            if (options.RemoveSpaces) {
                // add them here
                msg = $"{msg.Substring(0, 3)} {msg.Substring(3, 4)} {msg.Substring(7)}";
            }

            lastUptimeTotal += protoMsg.AdditionalUptimeInSeconds;

            return new IotDevice(
                name, id, msg,
                protoMsg.SelfCheckPassed, protoMsg.ServiceModeEnabled, lastUptimeTotal,
                new PhysicalValue(protoMsg.Pressure, "bar"),
                new PhysicalValue(protoMsg.Temperature, "°C"),
                new PhysicalValue(protoMsg.Distance, "m"));
        }
    }


}