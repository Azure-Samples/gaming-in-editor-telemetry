using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

using TelemetryAPI.Query;

namespace TelemetryRetriever
{
    class Program
    {
        static WebClient _webClient = new WebClient();
        static string _host = "http://localhost:7071/";
        static string _authCode = ""; // put your authentication code here if querying against a function in azure

        static string AuthCode => String.IsNullOrWhiteSpace(_authCode) ? "" : $"&code={_authCode}";

        static void Main(string[] args)
        {
            Console.WriteLine("Query with no filtering");
            Console.WriteLine(GetQuery() + "\n\n");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            var query1 = QBuilder.Eq("seq", 1);
            Console.WriteLine("Query: " + JsonConvert.SerializeObject(query1));
            Console.WriteLine(PostQuery(query1) + "\n\n");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            var query2 = QBuilder.In("name", "Test2", "Test3");
            Console.WriteLine("Query: " + JsonConvert.SerializeObject(query2));
            Console.WriteLine(PostQuery(query2) + "\n\n");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            var query3 = QBuilder.Or(query1, query2);
            Console.WriteLine("Query: " + JsonConvert.SerializeObject(query3));
            Console.WriteLine(PostQuery(query3) + "\n\n");
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Gets the latest events with no filtering
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        static string GetQuery(int take = 10)
        {
            return JObject.Parse(_webClient.DownloadString($"{_host}api/query?take={take}{AuthCode}")).ToString(Formatting.Indented);
        }

        /// <summary>
        /// Gets events matching the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        static string PostQuery(QuerySpec query, int take = 10)
        {
            string data = JsonConvert.SerializeObject(query);
            return JObject.Parse(_webClient.UploadString($"{_host}api/query?take={take}{AuthCode}", data)).ToString(Formatting.Indented);
        }
    }
}
