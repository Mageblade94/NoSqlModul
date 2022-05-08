using MongoDB.Bson;
using MongoDB.Driver;
using DbConnector;
using System.IO;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading;
using MongoDB.Driver.Linq;
using System.Linq;

namespace SQLfill
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("No Sql Datenbanken werden befüllt");
            var mongoDb = DatabaseHandler.GetCollectionFromMongoDb();
            var mongoDbTask = GenerateElementsForMongoDbAsync(mongoDb);
            var couchDbTask = GenerateElementsForCouchDbAsync();
            Task.WaitAll(mongoDbTask, couchDbTask);
            Console.WriteLine("Befüllung fertig");

            ////////////////////////////////////

            var stopwatch = new Stopwatch();


            //Abfragen für CouchDb
            var optionsForRestSharpClient = new RestClientOptions()
            {
                ThrowOnAnyError = true,
            };
            var contentFromFile = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "BsonExample.json"));
            var client = new RestClient(optionsForRestSharpClient);
            client.Authenticator = new HttpBasicAuthenticator("admin", "admin");

            //Abfrage für Alle Datensätze
            var callAllDocuments = new RestRequest("http://46.38.237.173:5984/questionnaire/_all_docs", Method.Get);
            stopwatch.Start();
            var allCouchDbDocuments = await client.ExecuteAsync(callAllDocuments);
            stopwatch.Stop();
            Console.WriteLine($"Abfrage aller Daten der Couchdb dauerte insgesamt {stopwatch.ElapsedMilliseconds} sekunden");

            //Abfrage für 10 Datensätze
            var callTenDocuments = new RestRequest("http://46.38.237.173:5984/questionnaire/_find?limit=10", Method.Get);
            stopwatch.Start();
            var tenDocuments = await client.ExecuteAsync(callTenDocuments);
            stopwatch.Stop();
            Console.WriteLine($"Abfrage aller Daten der Couchdb dauerte insgesamt {stopwatch.ElapsedMilliseconds} sekunden");

            //Abfrage für 100 Datensätze
            var callOneHundretDocuments = new RestRequest("http://46.38.237.173:5984/questionnaire/_find?limit=100", Method.Get);
            stopwatch.Start();
            var oneHundretDocuments = await client.ExecuteAsync(callOneHundretDocuments);
            stopwatch.Stop();
            Console.WriteLine($"Abfrage aller Daten der Couchdb dauerte insgesamt {stopwatch.ElapsedMilliseconds} sekunden");

            ////Vergleich zweier Datensätze 10000 mal
            //var callOneHundretDocuments = new RestRequest("http://46.38.237.173:5984/questionnaire/_find?limit=10", Method.Get);
            //stopwatch.Start();
            //var oneHundretDocuments = client.ExecuteAsync(callOneHundretDocuments).Result;
            //stopwatch.Stop();
            //Console.WriteLine($"Abfrage aller Daten der Couchdb dauerte insgesamt {stopwatch.ElapsedMilliseconds} sekunden");


            //////////////////////////////////////////////////

            //Abfragen für Mongo

            //Abfrage All Data
            stopwatch.Restart();
            var allDocuments = mongoDb.Find(_ => true).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Abfrage aller Daten der MongoDb dauerte insgesamt {stopwatch.ElapsedMilliseconds} milisekunden");

            //Abfrage 10 Datensätze
            stopwatch.Restart();
            var a = mongoDb.Find(_ => true).Limit(10).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Abfrage von 10 Datensätzen der MongoDb dauerte insgesamt {stopwatch.ElapsedMilliseconds} milisekunden");

            //Abfrage 100 Datensätze
            stopwatch.Restart();
            var b = mongoDb.Find(_ => true).Limit(100).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Abfrage von 100 Datensätzen der MongoDb dauerte insgesamt {stopwatch.ElapsedMilliseconds} milisekunden");

            //Vergleich von zwei Datensätzen, 10000 mal
            stopwatch.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var itemOne = mongoDb.AsQueryable().Sample(1);
                var itemTwo = mongoDb.AsQueryable().Sample(1);
                bool isEqual = itemOne == itemTwo;
            }
            stopwatch.Stop();
            Console.WriteLine($"Vergleich von 10000 Random Datensätzen dauerte insgesamt {stopwatch.ElapsedMilliseconds} milisekunden");
        }

        private static async Task GenerateElementsForCouchDbAsync(CancellationToken token = default)
        {
            Console.WriteLine("Task läuft");
            var optionsForRestSharpClient = new RestClientOptions()
            {
                ThrowOnAnyError = true,
                Timeout = 1000,
            };
            var contentFromFile = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "BsonExample.json"));
            var client = new RestClient(optionsForRestSharpClient);
            client.Authenticator = new HttpBasicAuthenticator("admin", "admin");
            var postRequest = new RestRequest("http://46.38.237.173:5984/questionnaire", Method.Post);
            postRequest.AddStringBody(contentFromFile, DataFormat.Json);

            for (int i = 0; i < 100000; i++)
            {
                await client.ExecuteAsync(postRequest);
            }
            Console.WriteLine("Couch Db gefüllt!");
        }

        private static async Task GenerateElementsForMongoDbAsync(IMongoCollection<BsonDocument> collection, CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Task läuft");
                string pathToJsonFile = Path.Combine(Environment.CurrentDirectory, "BsonExample.json");
                using (StreamReader r = new StreamReader(pathToJsonFile))
                {
                    string json = r.ReadToEnd();
                    var parsedBsonDocument = BsonDocument.Parse(json);

                    for (int i = 0; i < 100000; i++)
                    {
                        await collection.InsertOneAsync(parsedBsonDocument.Set("_id", Guid.NewGuid()));
                    }

                    Console.WriteLine("MongoDb gefüllt!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
