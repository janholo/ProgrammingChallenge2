
using ProgrammingChallenge2.Model;
using Google.Protobuf;

namespace ProgrammingChallenge2.Codecs.Matthid
{
    public class StupidProtobufCodecFactory : ICodecFactory
    {
        public string Name => "Stupid Protobuf Codec by Matthias";

        public IDecoder CreateDecoder() => new StupidProtobufCodec();

        public IEncoder CreateEncoder() => new StupidProtobufCodec();
    }

    public class StupidProtobufCodec : IEncoder, IDecoder {
        
        public StupidProtobufCodec()
        {
        }

        private ProgrammingChallenge2.Codecs.Matthid.Protobuf.StupidPhysicalValue EncodePhysical(PhysicalValue p) {
            var pressureProto = new ProgrammingChallenge2.Codecs.Matthid.Protobuf.StupidPhysicalValue();
            pressureProto.Unit = p.Unit;
            pressureProto.Value = p.Value;
            return pressureProto;
        }

        public byte[] Encode(IotDevice device) {
            var protoMsg = new ProgrammingChallenge2.Codecs.Matthid.Protobuf.StupidIoTDevice();

            protoMsg.Name = device.Name;
            protoMsg.Id = device.Id;
            protoMsg.StatusMessage = device.StatusMessage;
            protoMsg.SelfCheckPassed = device.SelfCheckPassed;
            protoMsg.ServiceModeEnabled = device.ServiceModeEnabled;
            protoMsg.UptimeInSeconds = device.UptimeInSeconds;
            
            protoMsg.Pressure = EncodePhysical(device.Pressure);
            protoMsg.Temperature = EncodePhysical(device.Temperature);
            protoMsg.Distance = EncodePhysical(device.Distance);

            return protoMsg.ToByteArray();
        }

        private PhysicalValue DecodePhysical(ProgrammingChallenge2.Codecs.Matthid.Protobuf.StupidPhysicalValue p) {
            return new PhysicalValue(p.Value, p.Unit);
        }

        public IotDevice Decode(byte[] data) {
            var protoMsg = ProgrammingChallenge2.Codecs.Matthid.Protobuf.StupidIoTDevice.Parser.ParseFrom(data);
            return new IotDevice(
                protoMsg.Name, protoMsg.Id, protoMsg.StatusMessage,
                protoMsg.SelfCheckPassed, protoMsg.ServiceModeEnabled, protoMsg.UptimeInSeconds,
                DecodePhysical(protoMsg.Pressure), DecodePhysical(protoMsg.Temperature), DecodePhysical(protoMsg.Distance));
        }
    }
}