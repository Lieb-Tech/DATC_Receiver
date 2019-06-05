using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATC_Receiver.Helpers
{
    public class CosmosSetting
    {
        public CosmosDB Cosmos { get; set; }
        public class CosmosDB
        {
            public string EndpointUrl { get; set; }
            public string PrimaryKey { get; set; }
        }
    }

    public class CosmosDB
    {
        private DocumentClient client;

        public CosmosDB()
        {
            var data = File.ReadAllText("cosmosDbKey.json");
            if (data == null)
                throw new ArgumentNullException("missing cosmosDbKey.json");

            var config = JsonConvert.DeserializeObject<CosmosSetting>(data);

            this.client = new DocumentClient(new Uri(config.Cosmos.EndpointUrl), config.Cosmos.PrimaryKey, new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            });
        }

        public void OpenConnection()
        {
            client.OpenAsync().Wait();
        }

        public void DeleteDocument(string collection, string id, string partionKey)
        {
            try
            {
                var lnk = UriFactory.CreateDocumentUri("datafeeds", collection, id);
                client.DeleteDocumentAsync(lnk,
                    new RequestOptions() { PartitionKey = new PartitionKey(partionKey) }
                ).Wait();
            }
            catch (Exception ex)
            {
                var z = ex.Message;
            }
        }
       

        public IQueryable<dynamic> GetDocumentQuery(string collection, string query)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            return client.CreateDocumentQuery<dynamic>(
                GetCollectionLink(collection),
                query,
                queryOptions);
        }

        public IQueryable<T> GetDocumentQuery<T>(string collection)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            return client.CreateDocumentQuery<T>(this.GetCollectionLink(collection), queryOptions);
        }

        public Uri GetCollectionLink(string collectionName)
        {
            return UriFactory.CreateDocumentCollectionUri("liebfeeds", collectionName);
        }

        public Uri GetDocumentLink(string collectionName, string documentId)
        {
            return UriFactory.CreateDocumentUri("liebfeeds", collectionName, documentId);
        }

        public async Task UpsertDocument(dynamic doc, string collection)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");

            try
            {
                var res = await client.UpsertDocumentAsync(GetCollectionLink(collection), doc);
                var a = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("419");
                System.Threading.Thread.Sleep(500);
                try
                {
                    Console.WriteLine("419 2");
                    await client.UpsertDocumentAsync(GetCollectionLink(collection), doc);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("419 2");
                    System.Threading.Thread.Sleep(500);
                    Console.WriteLine("419 3");
                    await client.UpsertDocumentAsync(GetCollectionLink(collection), doc);
                }
            }
        }
        public async Task ReplaceDocument(dynamic doc, string collection)
        {
            try
            {
                await client.ReplaceDocumentAsync(GetCollectionLink(collection), doc);
                var a = "";
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(2000);
                try
                {
                    await client.UpsertDocumentAsync(GetCollectionLink(collection), doc);
                }
                catch (Exception ex2)
                {
                    System.Threading.Thread.Sleep(2000);
                    try
                    {
                        await client.UpsertDocumentAsync(GetCollectionLink(collection), doc);
                    }
                    catch (Exception ex3)
                    {
                    }
                }
            }
        }
    }
}