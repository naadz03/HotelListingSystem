using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public class SMSHelper
    {
        private readonly ApplicationDbContext _context;
        public SMSHelper(ApplicationDbContext _context)
        {
            this._context = _context;
        }

        public async Task SMSSend(String body, String MobileNumber)
        {
            SMSViewModel sms = new SMSViewModel();
            sms.messages = new Message[]
            {
                new Message
            {
                to =String.Format("+27{0}", MobileNumber.Substring(Math.Max(0, MobileNumber.Length - 9))),
                source = "php",
                body = body,
                custom_string = body
            }
            };

            string username = ConfigurationManager.AppSettings["sms_username"];
            string api_key = ConfigurationManager.AppSettings["sms_api_key"];

            // Concatenate username and password with a colon
            string credentials = String.Format("{0}:{1}", username, api_key);

            // Convert the concatenated string to a byte array
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);

            // Encode the byte array as a Base64 string
            string base64Credentials = Convert.ToBase64String(credentialsBytes);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://rest.clicksend.com/v3/sms/send");
            request.Headers.Add("Authorization", $"Basic {base64Credentials}");
            var content = new StringContent(JsonSerializer.Serialize(sms), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public class SMSViewModel
        {
            public Message[] messages { get; set; }
        }

        public class Message
        {
            public string source { get; set; }
            public string body { get; set; }
            public string to { get; set; }
            public string custom_string { get; set; }
        }


    }
}