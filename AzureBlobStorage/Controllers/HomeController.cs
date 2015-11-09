using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace AzureBlobStorage.Controllers
{
    public class HomeController : Controller
    {
        public void InitializeBlobStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("demo");
            container.CreateIfNotExists();
            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess =
                        BlobContainerPublicAccessType.Blob
                });

            var blockBlob = container.GetBlockBlobReference("myblob.jpg");

            WebRequest req = HttpWebRequest.Create("http://www.rabbit.org/adoption/bunny.jpg");
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                blockBlob.UploadFromStream(stream);
            }

            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                Debug.WriteLine("Listing blobs:");
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    Debug.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);
                    ViewBag.BlobUri = blob.Uri;
                }
            }

            using (var fileStream = System.IO.File.OpenWrite(@"C:\Users\Razvan\Desktop\myblob.jpg"))
            {
                blockBlob.DownloadToStream(fileStream);
            }

            //blockBlob.Delete();
        }

        public ActionResult Index()
        {
            Debug.WriteLine("Index.html");
            this.InitializeBlobStorage();

            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
