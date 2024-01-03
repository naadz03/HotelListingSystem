using HotelListingSystem.Models.FacialRecognition;
using iTextSharp.text.pdf.codec.wmf;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace HotelListingSystem.Helpers.API
{
    public class MXFaceAi
    {
        public  Boolean CompareImageWithStored(MXFaceFacialRequest _request, MXFaceAiResponse mXFaceAiResponse = null)
        {
            try
            {
                RestClient client = new RestClient("https://faceapi.mxface.ai/api/v3/face/verify");
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                RestRequest request = new RestRequest() { Method = Method.Post };
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("subscriptionkey", "QRVl7d42FMw5XXYhVp-sDqHOrdtJT1823");
                request.AddParameter("application/json", JsonConvert.SerializeObject(_request), ParameterType.RequestBody);
                var response = client.Execute(request);
                mXFaceAiResponse = JsonConvert.DeserializeObject<MXFaceAiResponse>(response.Content);
                return response.StatusCode == HttpStatusCode.BadRequest ? false : (mXFaceAiResponse.MatchedFaces[0].matchResult == 1);
            }
            catch (Exception)
            {

                throw;
            }

            
        }
    }
}