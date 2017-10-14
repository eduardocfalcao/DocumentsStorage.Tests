using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace DocumentsStorate.Api.Controllers
{
    public class FileStorageController : ApiController
    {
        public IActorSystem ActorSystem { get; }

        public FileStorageController(IActorSystem actorSystem)
        {
            ActorSystem = actorSystem;
        }

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        
        public string Get(int id)
        {
            return "value";
        }
        
        public async Task<IHttpActionResult> Post(FileMetadata fileMetadata)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

            foreach (var stream in filesReadToProvider.Contents)
            {
                var fileBytes = await stream.ReadAsByteArrayAsync();

                //ActorSystem.PublishMessage(new )
            }

            return Ok();
        }
        
        public void Put(int id, [FromBody]string value)
        {
        }
        
        public void Delete(int id)
        {
        }
    }
}
