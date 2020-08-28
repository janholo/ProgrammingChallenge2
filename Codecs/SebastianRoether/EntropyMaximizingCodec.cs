using System.Linq;
using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.SebastianRoether
{
    /*
    // Stateful (!) Codec. Der State ist offensichtlich nicht geshared, sonst könnte ich ja einfach das Device-Objekt drin speichern ;-)
    // Der Encoder und der Decoder haben jeweils einen eigenen State, ich hab Encoder/Decoder nur aus Bequemlichkeit in einer Klasse
    */
    public class EntropyMaximizingCodec : IEncoder, IDecoder
    {
        private StatelessCodec _decoder = new StatelessCodec();
        private StatelessCodec _encoder = new StatelessCodec();
        
        public CompressionBuffer _encodingBuffer;
        public CompressionBuffer _decodingBuffer;
        private IotDevice _lastEncodedDevice;
        private IotDevice _lastDecodedDevice;
        private bool _debug;
        private bool sent;

        public EntropyMaximizingCodec(bool debug = false)
        {
            _debug = debug;
            _encodingBuffer = new CompressionBuffer(debug);
            _decodingBuffer = new CompressionBuffer(debug);
        }

        public byte[] Encode(IotDevice device)
        {
            var clone = _encodingBuffer.Clone();
            Compress(_encodingBuffer, _lastEncodedDevice, device);

            var x = Decompress(clone, _lastEncodedDevice);
            _lastEncodedDevice = device;
            // Wenn beim Komprimieren Rundungsfehler auftreten, dann nehmen wir einfach den Stateless Codec
            if(x != null && IotDevice.AreEquals(device, x, _debug))
            {
                _encodingBuffer = clone;
                return new byte[0];
            }
            else
            {
                var bytes = _encoder.Encode(device, sent ? _lastEncodedDevice : null);
                sent = true;
                return bytes;
            }
        }

        private void Compress(CompressionBuffer predictor, IotDevice lastDevice, IotDevice device)
        {
            predictor.CompressAndFold(4 + ((lastDevice == null || lastDevice.StatusMessage == null || lastDevice.StatusMessage != device.StatusMessage) ? 10 : 0));
            predictor.CompressAndFold(device.Pressure.Value, 5);
            predictor.CompressAndFold(device.Temperature.Value, 100);
            predictor.CompressAndFold(device.Distance.Value, 1000);
        }

        private IotDevice Decompress(CompressionBuffer predictor, IotDevice lastDevice)
        {
            try 
            {
                var uptime = (lastDevice?.UptimeInSeconds ?? 0) + (ulong)(predictor.Decompress().Value * 100.0);
                string msg = lastDevice?.StatusMessage;
                if (lastDevice == null || predictor.Decompress().Value < 0.1)
                {
                    msg = CreateRandomString(3, predictor) + " " + CreateRandomString(4, predictor) + " " + CreateRandomString(3, predictor);
                }

                return new IotDevice(
                    lastDevice.Name,
                    lastDevice.Id,
                    msg,
                    predictor.Decompress().Value > 0.5,
                    predictor.Decompress().Value > 0.5,
                    uptime,
                    new PhysicalValue(predictor.Decompress().Value * 5.0, "bar"),
                    new PhysicalValue(predictor.Decompress().Value * 100.0, "°C"),
                    new PhysicalValue(predictor.Decompress().Value * 1000.0, "m"));
            }
            catch(InvalidOperationException ioe)
            {
                if(_debug) Console.WriteLine("Decompression Error" + ioe.ToString());
                return null;
            }
        }

        public IotDevice Decode(byte[] data)
        {
            try
            {
                var device = _decoder.Decode(data, _lastDecodedDevice);
                Compress(_decodingBuffer, _lastDecodedDevice, device);
                _lastDecodedDevice = device;
                return device;
            }
            catch
            {
                var device = Decompress(_decodingBuffer, _lastDecodedDevice);
                _lastDecodedDevice = device;
                return device;
            }
        }

        private string CreateRandomString(int length, CompressionBuffer predictor)
        {
            return new string(Enumerable.Repeat('a', length).Select(s => (char)((int)s + predictor.DecompressSimple().Value)).ToArray());
        }
    }
}