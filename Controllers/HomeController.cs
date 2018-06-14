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
        string urlWebCaht;

        public HomeController()
        {
            // Read env variables
            storageAccountName = Environment.GetEnvironmentVariable("STORAGEACCOUNT_NAME");
            storageAccountKey = Environment.GetEnvironmentVariable("STORAGEACCOUNT_KEY");
            urlWebCaht = Environment.GetEnvironmentVariable("URL");

        }
        public IActionResult Index()
        {
            //ViewBag.WebChatUrl = "https://webchat.botframework.com/embed/myqnavals?s=e8joe2NMj-Q.cwA.cDA.VOnrHSWDElARr2hDn3sZq9x64QzSArIYP5fZfho-Nj0";
            ViewBag.WebChatUrl = urlWebCaht;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string question, string answer, string department)
        {
            var randomId = Guid.NewGuid();

            // Send message to Queue
            var storageAccount = CloudStorageAccount.Parse(
                "DefaultEndpointsProtocol=http;AccountName=" + storageAccountName + ";AccountKey=" + storageAccountKey
            );

             var queueClient = storageAccount.CreateCloudQueueClient();
             var messageQueue = queueClient.GetQueueReference("qnaqueue");
             await messageQueue.CreateIfNotExistsAsync();

             var qnapair = new {
                 department = department,
                question = question,
                 answer = answer
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
