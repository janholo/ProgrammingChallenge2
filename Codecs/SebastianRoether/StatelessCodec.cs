using System;
using System.IO;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    public class StatelessCodec : IEncoder, IDecoder
    {
        private enum MessageFlags: byte {
            None = 0,
            SelfCheckPassed = 1,
            ServiceModeEnabled = 2,
            UseDefaultPressureUnit = 4,
            UseDefaultTemperatureUnit = 8,
            UseDefaultDistanceUnit = 16,
            IncludeMessage = 32
        }

        public byte[] Encode(IotDevice device) {
            return Encode(device, null);
        }

        public byte[] Encode(IotDevice device, IotDevice lastState = null)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            var messageFlags = 
                (device.SelfCheckPassed ? MessageFlags.SelfCheckPassed : MessageFlags.None) |
                (device.ServiceModeEnabled ? MessageFlags.ServiceModeEnabled : MessageFlags.None) |
                (device.Pressure.Unit == "bar" ? MessageFlags.UseDefaultPressureUnit : MessageFlags.None) |
                (device.Temperature.Unit == "°C" ? MessageFlags.UseDefaultTemperatureUnit : MessageFlags.None) |
                (device.Distance.Unit == "m" ? MessageFlags.UseDefaultDistanceUnit : MessageFlags.None) |
                (lastState?.StatusMessage != device.StatusMessage ? MessageFlags.IncludeMessage : MessageFlags.None);

            writer.Write((byte)messageFlags);
            if(lastState == null) writer.Write(device.Name);
            if(lastState == null) writer.Write(Guid.Parse(device.Id).ToByteArray());

            // Ja, man braucht nur ein 5 Bit Alphabet. Und die Leerzeichen kann man auch weglassen wenn man Annahmen trifft.
            if(true || CheckFlag(MessageFlags.IncludeMessage, messageFlags)) writer.Write(device.StatusMessage); // Matthid.ImprovedLowerCaseStringEncoder.EncodeString(device.StatusMessage.Replace(" ", "")));
            writer.Write(device.UptimeInSeconds);

            // Leider kann man nicht einfach den double in einen float oder sogar einen 2-Byte Wert konvertieren ohne große Annahmen zu treffen
            // Der Fehler in IoTDevice.AreEquals ist relativ definiert, je kleiner der Wert ist, desto kleiner darf der Fehler höchstens sein.
            // Die Konvertierung geht nur, wenn man weiß dass zufällige Werte niemals kleiner sind als eine bestimmte Grenze, wäre eine Messung
            // stattdessen z.B: double.Epsilon wäre jede Konvertierung unzulässig
            writer.Write((double)device.Pressure.Value);
            if(!CheckFlag(MessageFlags.UseDefaultPressureUnit, messageFlags)) writer.Write(device.Pressure.Unit);
            writer.Write((double)device.Temperature.Value);
            if(!CheckFlag(MessageFlags.UseDefaultTemperatureUnit, messageFlags)) writer.Write(device.Temperature.Unit);
            writer.Write((double)device.Distance.Value);
            if(!CheckFlag(MessageFlags.UseDefaultDistanceUnit, messageFlags)) writer.Write(device.Distance.Unit);

            return memoryStream.ToArray();
        }

        public IotDevice Decode(byte[] data) {
            return Decode(data, null);
        }

        public IotDevice Decode(byte[] data, IotDevice lastState = null)
        {
            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);
            
            var messageFlags = (MessageFlags)reader.ReadByte();

            var iotDevice = new IotDevice(
                lastState == null ? reader.ReadString() : lastState.Name,
                lastState == null ? new Guid(reader.ReadBytes(16)).ToString() : lastState.Id,
                /*CheckFlag(MessageFlags.IncludeMessage, messageFlags) ?*/ reader.ReadString(),/* : lastState.StatusMessage,*/ // Beautify(Matthid.ImprovedLowerCaseStringEncoder.DecodeBytes(reader.ReadBytes(7))): lastState.StatusMessage,
                CheckFlag(MessageFlags.SelfCheckPassed, messageFlags),
                CheckFlag(MessageFlags.ServiceModeEnabled, messageFlags),
                reader.ReadUInt64(),
                new PhysicalValue(reader.ReadDouble(), !CheckFlag(MessageFlags.UseDefaultPressureUnit, messageFlags) ? reader.ReadString() : "bar"),
                new PhysicalValue(reader.ReadDouble(), !CheckFlag(MessageFlags.UseDefaultTemperatureUnit, messageFlags) ? reader.ReadString() : "°C"),
                new PhysicalValue(reader.ReadDouble(), !CheckFlag(MessageFlags.UseDefaultDistanceUnit, messageFlags) ? reader.ReadString() : "m")
                );
            return iotDevice;
        }

        private bool CheckFlag(MessageFlags flag, MessageFlags messageHeader)
        {
            return (messageHeader & flag) == flag;
        }

        private string Beautify(string msg)
        {
            return $"{msg.Substring(0, 3)} {msg.Substring(3, 4)} {msg.Substring(7)}";
        }
    }
}