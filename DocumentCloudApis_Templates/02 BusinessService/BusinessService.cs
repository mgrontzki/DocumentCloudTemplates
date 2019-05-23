using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DocumentCloudApis_Templates.Klassen;
using DocumentCloudApis_Templates.Models;


namespace DocumentCloudApis_Templates.Services
{
    public class BusinessService
    {
        private string _strAccountName;
        private string _strAccountKey;
        private CloudStorageAccount _storageAccount;
        private string _strContainer;

        private CloudBlobContainer _TemplateContainer;


        public BusinessService(string container)
        {
            _strContainer = container;
            _strAccountName = ConfigurationManager.AppSettings["storage:account:name"];
            _strAccountKey = ConfigurationManager.AppSettings["storage:account:key"];
            _storageAccount = new CloudStorageAccount(new StorageCredentials(_strAccountName, _strAccountKey), true);
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            _TemplateContainer = blobClient.GetContainerReference(_strContainer);
        }

        public int CountTemplates()
        {
            var ListOfBlobs = _TemplateContainer.ListBlobs();
            return ListOfBlobs.Count();
        }


        public bool ExistsTemplate(string templateName)
        {

            foreach (IListBlobItem item in _TemplateContainer.ListBlobs())
            {
                if (item is CloudBlockBlob)
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;

                    if (blob.Name.ToLower() == templateName.ToLower())
                    {
                        return true;
                    }

                    //URL = blob.Uri.ToString()
                    //Size = blob.Properties.Length    
                }
            }

            return false;
        }




        public List<Template> List()
        {
            List<Template> Liste = new List<Template>();

            foreach (IListBlobItem item in _TemplateContainer.ListBlobs())
            {
                if (item is CloudBlockBlob)
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    Template Vorlage = new Template();

                    Vorlage.templateName = blob.Name;
                    Vorlage.size = blob.Properties.Length;
                    Vorlage.modified = blob.Properties.LastModified.ToString();

                    Liste.Add(Vorlage);
                }
            }

            return Liste;
        }

        public bool Delete(string templateName)
        {

            foreach (IListBlobItem item in _TemplateContainer.ListBlobs())
            {
                if (item is CloudBlockBlob)
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;

                    if (blob.Name.ToLower() == templateName.ToLower())
                    {
                        blob.Delete();
                        return true;
                    }
                }
            }

            return false;
        }

        public CloudBlockBlob Download(string templateName)
        {
            var blob = _TemplateContainer.GetBlockBlobReference(templateName);
            return blob;
        }

        public async Task<string> Upload(HttpContent ctnt, string templateName)
        {
            string filename = "";

            var provider = new AzureStorageMultipartFormDataStreamProvider(_TemplateContainer);
            await ctnt.ReadAsMultipartAsync(provider);

            // Retrieve the filename of the file you have uploaded
            filename = provider.FileData.FirstOrDefault()?.LocalFileName;
            if (string.IsNullOrEmpty(filename))
            {
                return "error";
            }

            return filename;
        }
    }
}