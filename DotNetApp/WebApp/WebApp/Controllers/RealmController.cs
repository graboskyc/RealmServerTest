using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Realms;
using Realms.Sync;
using Nito.AsyncEx;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RealmController : ControllerBase
    {
        private readonly ILogger<RealmController> _logger;

        public RealmController(ILogger<RealmController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task Get(string name = "noargname", string status = "noargstatus")
        {
            AsyncContext.Run(async () => await InsertAsync(name, status));
        }

        private static async Task InsertAsync(string name, string status) { 
            var app = App.Create("realmsynctest-snsxl");
            var user = await app.LogInAsync(Credentials.Anonymous());

            //var config = new SyncConfiguration("_pk", user);
            var config = new SyncConfiguration($"user={ user.Id }", user);


            var realm = await Realm.GetInstanceAsync(config);

            var t = new Todo
            {
                Name = name,
                Status = status,
                //Partition = user.Id
                Partition = $"user={ user.Id }"
            };

            realm.Write(() =>
            {
                realm.Add(t);
                realm.Add(t);
                realm.Add(t);
            });

            await user.LogOutAsync();

            await Task.Delay(2000);
        }
           
    }



    public class Todo : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [MapTo("_pk")]
        [Required]
        public string Partition { get; set; }
        [MapTo("name")]
        [Required]
        public string Name { get; set; }
        [MapTo("status")]
        public string Status { get; set; }
    }

    public enum TaskStatus
    {
        Open,
        InProgress,
        Complete
    }
}
