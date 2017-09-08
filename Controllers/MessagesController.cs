using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace TestBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        MySqlConnection ConDB =
               new MySqlConnection("Server=yazokulu-yemekbot-mysql.mysql.database.azure.com; Port=3306; Database=yemek; Uid=yemekbot@yazokulu-yemekbot-mysql; Pwd=WY0G0eZjfOYT; SslMode=Preferred;");
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return

                string GonderilcekMesaj;
                string gelenMesaj = activity.Text;


                GonderilcekMesaj = "Lütfen Tekrar deneyiniz";
                string Canahtar = "7v3aahe567p9pza7g3cotyultuc10e";
                //CyanLog a istek gönderir ve cevabı alır
                string url = $"http://api.cyanlog.com/?q={gelenMesaj}&Anahtar={Canahtar}";
                string requesturl = url;
                string content = fileGetContents(requesturl);
                JObject o = JObject.Parse(content);



                string YemekTur;
                string Sehir;
              //  somedeneme = "ddd";
                YemekTur = (string)o.SelectToken("yemektur");
                Sehir = (string)o.SelectToken("sehir");

                if (YemekTur != null)
                {
                GonderilcekMesaj = YemekTarifiGetir(YemekTur);
                  //  GonderilcekMesaj = YemekTur;
                }
                if(Sehir != null)
                {
                    GonderilcekMesaj = $"Bence {Sehir} şehrinde yenecek en güzel yemek {SehirYemekGetir(Sehir)} ";

                }


                

                Activity reply = activity.CreateReply(GonderilcekMesaj);
                                 
                

                // return our reply to the user
                
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        public string YemekTarifiGetir(string YemekTur)
        {
            if(YemekTur == "mad??mak")
            {
                YemekTur = "madımak";
            }
            ConDB.Open();
            MySqlCommand komut2 = new MySqlCommand("SELECT * FROM yemektarifleri where yemek like '" 
                + YemekTur + "'", ConDB);
           
            MySqlDataAdapter adap = new MySqlDataAdapter(komut2);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            ConDB.Close();
                       

            return dt.Rows[0]["Tarif"].ToString();
        }
        public string SehirYemekGetir(string Sehir)
        {
            ConDB.Open();
            MySqlCommand komut2 = new MySqlCommand("SELECT * FROM yemektarifleri where Sehir like '"
                + Sehir + "'", ConDB);

            MySqlDataAdapter adap = new MySqlDataAdapter(komut2);
            DataTable dt = new DataTable();
            adap.Fill(dt);
            ConDB.Close();
            return dt.Rows[0]["Yemek"].ToString(); 
        }
        protected string fileGetContents(string fileName)
        {
            string sContents = string.Empty;
            string me = string.Empty;
            try
            {
                if (fileName.ToLower().IndexOf("http:") > -1)
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    byte[] response = wc.DownloadData(fileName);
                    sContents = System.Text.Encoding.ASCII.GetString(response);

                }
                else
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileName, Encoding.UTF8);
                    sContents = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch { sContents = "unable to connect to server "; }
            return sContents;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}