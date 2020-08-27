using System.Linq;
using System;
using Newtonsoft.Json;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.SebastianRoether
{
    /*
    // Stateful (!) Codec. Der State ist offensichtlich nicht geshared, sonst könnte ich ja einfach das Device-Objekt drin speichern ;-)
    // Der Encoder und der Decoder haben jeweils einen eigenen State, ich hab Encoder/Decoder nur aus Bequemlichkeit in einer Klasse
    */
    public class EntropyMaximizingCodec : IEncoder, IDecoder
    {
        private StatelessCodec _codec = new StatelessCodec();
        private CompressionBuffer _encodingPredictor;
        private CompressionBuffer _decodingPredictor;
        private IotDevice _lastEncodedDevice;
        private IotDevice _lastDecodedDevice;
        private bool _debug;

        public EntropyMaximizingCodec(bool debug = false)
        {
            _debug = debug;
            _encodingPredictor = new CompressionBuffer(debug);
            _decodingPredictor = new CompressionBuffer(debug);
        }

        public byte[] Encode(IotDevice device)
        {
            var clone = _encodingPredictor.Clone();
            compress(_encodingPredictor, _lastEncodedDevice, device);

            if(_debug) Console.WriteLine(_encodingPredictor.P());

            var x = decompress(clone, _lastEncodedDevice, true);

            _lastEncodedDevice = device;
            // Wenn beim Komprimieren Rundungsfehler auftreten, dann nehmen wir einfach den Stateless Codec
            if(x != null && IotDevice.AreEquals(device, x, _debug))
            {
                return new byte[0];
            }
            else
            {
                if(_debug)
                {
                    var text2 = JsonConvert.SerializeObject(x);
                    Console.WriteLine(text2);

                    var text3 = JsonConvert.SerializeObject(device);
                    Console.WriteLine(text3);
                }

                return _codec.Encode(device);
            }
        }

        private void compress(CompressionBuffer predictor, IotDevice lastDevice, IotDevice device)
        {
            predictor.miss();
            if (lastDevice == null || lastDevice.StatusMessage == null || lastDevice.StatusMessage != device.StatusMessage)
            {
                predictor.miss(); // observe true
                for (int i = 0; i < 10; i++)
                {
                    predictor.miss();
                }
            }
            else
            {
                predictor.miss(); // observe false
            }
            predictor.miss();
            predictor.miss();

            predictor.observe(device.Pressure.Value / 5.0);
            predictor.observe(device.Temperature.Value / 100.0);
            //predictor.observe(device.Distance.Value / 1000.0);
            predictor.miss();
        }

        private IotDevice decompress(CompressionBuffer predictor, IotDevice lastDevice, bool doMessage)
        {
            try 
            {
                var uptime = (lastDevice?.UptimeInSeconds ?? 0) + (ulong)(predictor.NextDouble().Value * 100.0);
                string msg = lastDevice?.StatusMessage;
                if (lastDevice == null || predictor.NextDouble().Value < 0.1 && doMessage)
                {
                    msg = CreateRandomString(3, predictor) + " " + CreateRandomString(4, predictor) + " " + CreateRandomString(3, predictor);
                }

                return new IotDevice(
                    lastDevice.Name,
                    lastDevice.Id,
                    msg,
                    predictor.NextDouble().Value > 0.5,
                    predictor.NextDouble().Value > 0.5,
                    uptime,
                    new PhysicalValue(predictor.NextDouble().Value * 5.0, "bar"),
                    new PhysicalValue(predictor.NextDouble().Value * 100.0, "°C"),
                    new PhysicalValue(predictor.NextDouble().Value * 1000.0, "m"));
            }
            catch(InvalidOperationException ioe)
            {
                if(_debug) Console.WriteLine("Prediction Error" + ioe.ToString());
                return null;
            }
        }

        public IotDevice Decode(byte[] data)
        {
            try
            {
                var device = _codec.Decode(data);
                compress(_decodingPredictor, _lastDecodedDevice, device);
                _lastDecodedDevice = device;
                return device;
            }
            catch
            {
                var device = decompress(_decodingPredictor, _lastDecodedDevice, true);
                _lastDecodedDevice = device;
                return device;
            }
        }

        private string CreateRandomString(int length, CompressionBuffer predictor)
        {
            return new string(Enumerable.Repeat('a', length).Select(s => (char)((int)s + predictor.Next(25).Value)).ToArray());
        }
    }

}

