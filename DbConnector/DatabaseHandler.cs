using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace DbConnector
{
    public class DatabaseHandler
    {
        private string ConnectionString { get; set; }
        private DatabaseHandler(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private static string GetConnectionStringForProvidedDb(ConnectorType connectorType)
        {
            var connector = Create(connectorType);
            return connector.ConnectionString;
        }
        private static DatabaseHandler Create(ConnectorType connectorType)
        {
            string conString;
            switch (connectorType)
            {
                case ConnectorType.NoSqlMongo:
                    conString = "mongodb://46.38.237.173:27020/?readPreference=primary&directConnection=true&ssl=false";
                    break;
                case ConnectorType.NoSqlCouch:
                    conString = "couchbase://46.38.237.173:5984/";
                    break;
                case ConnectorType.MySql:
                    conString = "";
                    break;
                default:
                    throw new Exception("No String execption");
            }

            return new DatabaseHandler(conString);
        }

        public static IMongoCollection<BsonDocument> GetCollectionFromMongoDb()
        {
            var mongoConnectionString = GetConnectionStringForProvidedDb(ConnectorType.NoSqlMongo);
            var mongoClient = new MongoClient(mongoConnectionString);
            var db = mongoClient.GetDatabase("QuestionareDb");
            var collection = db.GetCollection<BsonDocument>("QuestionareCollection");
            return collection;
        }

    }

    public enum ConnectorType
    {
        NoSqlMongo,
        NoSqlCouch,
        MySql
    }
}
