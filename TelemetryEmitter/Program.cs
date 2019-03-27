using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using TelemetryAPI;

namespace TelemetryEmitter
{
    class Program
    {
        static string _clientId = Guid.NewGuid().ToString();
        static string _sessionId = Guid.NewGuid().ToString();

        static WebClient _webClient = new WebClient();

        const string INGEST_URL = "http://localhost:7071/api/ingest";

        static uint _sequence = 1;

        static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        };

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback((sender, cert, chain, errors) => true);

            bool run = true;
            while (run)
            {
                // Create a simple batch with some test data
                string payload = CreateBatch(
                    CreateHeader(),
                    CreateEvent("Test1"),
                    CreateEvent("Test2"),
                    CreateEvent("Test3")
                );

                Console.WriteLine(payload);
                Task.Factory.StartNew(() => PostTelemetry(payload));

                run = ShouldContinue();
            }
        }

        static string CreateBatch(SimpleEventHeader header, params SimpleEvent[] events)
        {
            return JsonConvert.SerializeObject(new { Header = header, Events = events }, _jsonSettings) ;
        }

        static SimpleEventHeader CreateHeader()
        {
            return new SimpleEventHeader()
            {
                SessionId = _sessionId,
                ClientId = _clientId,
                Version = "1.0.0"
            };
        }

        static SimpleEvent CreateEvent(string name)
        {
            return new SimpleEvent()
            {
                Name = name,
                ClientTimestamp = DateTime.UtcNow,
                Sequence = _sequence++
            };
        }

        static void PostTelemetry(string data)
        {
            
            _webClient.UploadString(INGEST_URL, data);
        }

        static bool ShouldContinue()
        {
            Console.WriteLine("Press Ctrl-C to exit. Any other key to send more telemetry.");
            var key = Console.ReadKey();
            if (key.KeyChar == 'c' && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                return false;
            }
            return true;
        }
    }
}
