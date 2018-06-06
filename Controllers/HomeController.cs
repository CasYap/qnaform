using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using customerfeedback_web.Models;

using MongoDB.Bson;
using MongoDB.Driver;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace customerfeedback_web.Controllers
{
    public class HomeController : Controller
    {
        string storageAccountName;
        string storageAccountKey;

        public HomeController()
        {
            // Read env variables
      //      storageAccountName = Environment.GetEnvironmentVariable("STORAGEACCOUNT_NAME");
        //    storageAccountKey = Environment.GetEnvironmentVariable("STORAGEACCOUNT_KEY");

            storageAccountName = "storageqna";
            storageAccountKey = "JMSkY+o7WttRGkiO2MOyBmqjbWBan7zTOObkQp+wbQVVjwgnmTBJw2U7e9jzfhuibibwgsiQh6vhMOrq4uMhtA==";
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string question, string answer, string email)
        {
            var randomId = Guid.NewGuid();

            // Store in Cosmos DB (Mongo DB)
            /*
            var document = new BsonDocument
            {
                { "email", email },
                { "feedback", feedback },
                { "feedback_id",  randomId.ToString()}
            };

            var collection = _database.GetCollection<BsonDocument>("feedback");
            await collection.InsertOneAsync(document);
            */

            // Send message to Queue
            var storageAccount = CloudStorageAccount.Parse(
                "DefaultEndpointsProtocol=http;AccountName=" + storageAccountName + ";AccountKey=" + storageAccountKey
            );

             var queueClient = storageAccount.CreateCloudQueueClient();
             var messageQueue = queueClient.GetQueueReference("qnaqueue");
             await messageQueue.CreateIfNotExistsAsync();

             var qnapair = new {
                email = email,
                question = question,
                ansrew = answer
             };

             var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(qnapair));
             await messageQueue.AddMessageAsync(queueMessage);
             
            // Go back to index
            return RedirectToAction("Index"); 
        }  

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
