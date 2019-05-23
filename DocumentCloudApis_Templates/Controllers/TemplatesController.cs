using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DocumentCloudApis_Templates.Klassen;
using DocumentCloudApis_Templates.Models;
using DocumentCloudApis_Templates.Services;

namespace DocumentCloudApis_Templates.Controllers
{
    public class TemplatesController : ApiController
    {
        private const string Client = "templates";

        [HttpGet]
        public IHttpActionResult Count()
        {
            int Anzahl;

            BusinessService bizService = new BusinessService(Client);

            Anzahl = bizService.CountTemplates();

            if (Anzahl == 0)
            {
                return NotFound();
            }
            return Ok(Anzahl);
        }


        [HttpGet]
        public IHttpActionResult Exists(string templateName)
        {
            BusinessService bizService = new BusinessService(Client);

            return Ok(bizService.ExistsTemplate(templateName));
        }



        [HttpPost]
        public async Task<IHttpActionResult> Upload(string templateName)
        {

            string filename = "";

            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            BusinessService bizService = new BusinessService(Client);

            HttpContent ctnt = Request.Content;

            try
            {
                filename = await bizService.Upload(ctnt, templateName);
                //await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }

            // Retrieve the filename of the file you have uploaded
            if (filename == "error")
            {
                return BadRequest("An error has occured while uploading your file. Please try again.");
            }

            return Ok($"{filename} has successfully uploaded");

        }


        [HttpGet]
        public IHttpActionResult List()
        {
            List<Template> Liste;
            BusinessService bizService = new BusinessService(Client);

            Liste = bizService.List();
            if (Liste.Count == 0)
            {
                return NotFound();
            }

            return Ok(bizService.List());
        }

        [HttpDelete]
        [Authorize]
        public IHttpActionResult Delete(string templateName)
        {
            BusinessService bizService = new BusinessService(Client);

            if (bizService.Delete(templateName))
            {
                return Ok();
            }

            return BadRequest("Template could not be deleted");
        }


        [HttpGet]
        public async Task<HttpResponseMessage> Download(string templateName)
        {
            BusinessService bizService = new BusinessService(Client);

            CloudBlockBlob blob = bizService.Download(templateName);

            var blobExists = await blob.ExistsAsync();
            if (!blobExists)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File not found");
            }

            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            Stream blobStream = await blob.OpenReadAsync();

            message.Content = new StreamContent(blobStream);
            message.Content.Headers.ContentLength = blob.Properties.Length;
            message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(blob.Properties.ContentType);
            message.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = blob.Name,
                Size = blob.Properties.Length
            };

            return message;
        }

    }
}
