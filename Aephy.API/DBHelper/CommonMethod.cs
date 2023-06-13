using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Aephy.API.DBHelper
{
    public class CommonMethod
    {
        static IConfiguration _configuration = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        private readonly AephyAppDbContext _db;
        public CommonMethod(AephyAppDbContext dbContext)
        {
            _db = dbContext;
        }
        public DataSet jsonToDataSet(string jsonString)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
                xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);
                DataSet ds = new DataSet();
                ds.ReadXml(new XmlNodeReader(xd));
                return ds;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
        
        

        
        
        public string GenerateRandomPassword()
        {
            PasswordOptions opts = null;
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }
            //string newPassword = string.Join("", chars.ToArray());
            return string.Join("", chars.ToArray());
        }
        
        

        public async Task<string> SendMail(string ReceiverEmailId, string Newpassword, string FirstName)
        {
            var FilePath = _configuration["BlobStorageSettings:DocumentPath"] + " Logo.png" + _configuration["BlobStorageSettings:DocumentPathToken"];
            var SenderPassword = _configuration["Smtp:Password"].ToString();
            var SendEmailId = _configuration["Smtp:FromAddress"].ToString();
            var Host = _configuration["Smtp:Server"].ToString();
            var Port = Convert.ToInt32(_configuration["Smtp:Port"]);

            var MailSubject = "Password Reset";
            var MailBody = "<p style='padding-left:2%;'>Hello " + FirstName + "," +
                            "</p><p style='padding-left: 5%;'>Your password has been updated successfully." +
                            "<br>Your updated password is: <b>" + Newpassword + "</b>" +
                            "<br><b>Note:- </b> We recommend you to change the password when you login first time with this new password.</p>";
            var sumUp = "<p style='padding-top: 3%;padding-left: 3%;border-left: 1px solid #d5d5ec;'> </p>";


            AlternateView alternateView = AlternateView.CreateAlternateViewFromString
            (
             MailBody + "<br> <div style=\"display: flex;\"><p style=\"padding-left: 2%;\">" +
             "<img src=" + FilePath + " style=\"height: 100px;width: 120px;\"></p>" +
             sumUp + "</div>", null, "text/html"
            );

            //Byte[] bytes = System.IO.File.ReadAllBytes(FilePath);
            //System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bytes);
            //var imageToInline = new LinkedResource(streamBitmap, MediaTypeNames.Image.Jpeg);
            //imageToInline.ContentId = "MyImage";
            //imageToInline.ContentType.Name = " ";
            //alternateView.LinkedResources.Add(imageToInline);

            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(ReceiverEmailId);
                mail.From = new MailAddress(SendEmailId);
                mail.Subject = MailSubject;
                mail.AlternateViews.Add(alternateView);
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(Host, Port);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(SendEmailId, SenderPassword);
                try
                {
                    smtp.Send(mail);
                    return "Mail Sent Successfully";
                }
                catch (Exception ex)
                {
                    string SendMailError = ex.Message + ex.StackTrace;
                    return SendMailError;
                }

            }
            catch (Exception ex)
            {
                string Error = ex.Message;
                return Error;
            }

        }

        public async Task<string> UploadBlobFile(IFormFile files, string BlobContainerName)
        {
            string FileName = "";
            try
            {
                //var BlobContainerName = "userimages";
                FileName = Path.GetRandomFileName().Replace(".", "") + Path.GetExtension(files.FileName).ToLowerInvariant();
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_configuration["BlobStorageSettings:BlobStorageConnStr"].ToString());
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(BlobContainerName);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(FileName);
                await using (var data = files.OpenReadStream())
                {
                    await blockBlob.UploadFromStreamAsync(data);
                }
                return FileName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task DeleteBlobFile(string fileName, string BlobContainerName)
        {
            try
            {
                //var BlobContainerName = "userimages";
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_configuration["BlobStorageSettings:BlobStorageConnStr"].ToString());
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(BlobContainerName);
                var blob = cloudBlobContainer.GetBlobReference(fileName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
    }
}
