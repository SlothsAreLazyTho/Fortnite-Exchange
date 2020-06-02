using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using Fortnite_Exchange.Responses;

namespace Fortnite_Exchange
{
    class Program
    {
        private readonly string iosClientToken = "MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=";
        private readonly string header = "ZWM2ODRiOGM2ODdmNDc5ZmFkZWEzY2IyYWQ4M2Y1YzY6ZTFmMzFjMjExZjI4NDEzMTg2MjYyZDM3YTEzZmM4NGQ=";

        private readonly string serviceUrl = "https://account-public-service-prod03.ol.epicgames.com";
        private readonly string authcodeUrl = "https://www.epicgames.com/id/logout?redirectUrl=https%3A//www.epicgames.com/id/login%3FredirectUrl%3Dhttps%253A%252F%252Fwww.epicgames.com%252Fid%252Fapi%252Fredirect%253FclientId%253Dec684b8c687f479fadea3cb2ad83f5c6%2526responseType%253Dcode";

        private readonly string fileName = @"device.json";

        private readonly HttpClient client = new HttpClient();

        private AuthResponse aRsp;
        private ExchangeResponse excRsp;
        private DeviceResponse devRsp;

        static void Main(string[] args)
        {
            new Program().start();
            Console.ReadLine();
        }

        async void start()
        {
            if (!File.Exists(fileName))
            {
                System.Diagnostics.Process.Start(authcodeUrl);
                Console.WriteLine("You need to create a Device.Json in order to keep getting the Exchange");
                Console.Write("Authorization Code: ");
                var input = Console.ReadLine();
                this.aRsp = await this.GetAuthResponse(input, false);
            } else
            {
                var file = File.ReadAllText(this.fileName);
                this.devRsp = JsonConvert.DeserializeObject<DeviceResponse>(file);
                this.aRsp = await this.GetDeviceResponse();
            }

            this.excRsp = await this.GetExchangeCode();
            Console.WriteLine($"Account Name: {this.aRsp.displayName}");
            Console.WriteLine($"Exchange Code: {this.excRsp.code}");
            
            if(!File.Exists(fileName))
            {
                var rsp = await CreateDevice(await this.GetAuthResponse(this.excRsp.code, true));
                var text = JsonConvert.SerializeObject(rsp);
                File.WriteAllText(this.fileName, text);
                this.excep("Succesfully written all text to Device.Json\nYou can now generate the exchange token over and over again");
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="code">Exchange Code or Authentication code.</param>
        /// <param name="exchange">If exchange is true, it will authenticate via exchange_code. Otherwise via code.</param>
        /// <see cref="AuthResponse"/>
        /// <returns>Authentication Response</returns>
        private async Task<AuthResponse> GetAuthResponse(String code, bool exchange)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if(exchange)
            {
                list.Add(this.createKey("grant_type", "exchange_code"));
                list.Add(this.createKey("exchange_code", $"{code}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", this.iosClientToken);
            } 
            else
            { 
                list.Add(this.createKey("grant_type", "authorization_code"));
                list.Add(this.createKey("code", $"{code}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", this.header);
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(list);
            var rsp = client.PostAsync($"{this.serviceUrl}/account/api/oauth/token", content).Result;

            if (rsp.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AuthResponse>(await rsp.Content.ReadAsStringAsync());
            }
            else
            {
                var e = JsonConvert.DeserializeObject<EpicError>(await rsp.Content.ReadAsStringAsync());
                this.excep($"Error: {e.errorCode}\nMessage: {e.errorMessage}");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response">Response from authentication.</param>
        /// <see cref="DeviceResponse"/>
        /// <returns>Device Response</returns>
        private async Task<DeviceResponse> CreateDevice(AuthResponse response)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", response.access_token);
            var rsp = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"{this.serviceUrl}/account/api/public/account/{response.account_id}/deviceAuth")).Result;

            if (rsp.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<DeviceResponse>(await rsp.Content.ReadAsStringAsync());
            }
            else
            {
                var e = JsonConvert.DeserializeObject<EpicError>(await rsp.Content.ReadAsStringAsync());
                this.excep($"Error: {e.errorCode}\nMessage: {e.errorMessage}");
                return null;
            }
        }

        /// <summary>
        ///     Authentication from response
        /// </summary>
        /// <see cref="AuthResponse"/>
        /// <returns>Authentication Response</returns>
        /// <see cref="AuthResponse"/>
        private async Task<AuthResponse> GetDeviceResponse()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            list.Add(this.createKey("grant_type", "device_auth"));
            list.Add(this.createKey("account_id", $"{devRsp.accountId}"));
            list.Add(this.createKey("device_id", $"{devRsp.deviceId}"));
            list.Add(this.createKey("secret", $"{devRsp.secret}"));

            FormUrlEncodedContent content = new FormUrlEncodedContent(list);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", this.iosClientToken);
            var rsp = client.PostAsync($"{this.serviceUrl}/account/api/oauth/token", content).Result;

            if(rsp.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AuthResponse>(await rsp.Content.ReadAsStringAsync());
            } 
            else
            {
                var e = JsonConvert.DeserializeObject<EpicError>(await rsp.Content.ReadAsStringAsync());
                this.excep($"Error: {e.errorCode}\nMessage: {e.errorMessage}");
                return null;
            }
        }


        /// <summary>
        ///     
        /// </summary>
        /// <see cref="ExchangeResponse"/>
        /// <returns>Exchange token response.</returns>
        private async Task<ExchangeResponse> GetExchangeCode()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", this.aRsp.access_token);
            var rsp = client.GetAsync($"{this.serviceUrl}/account/api/oauth/exchange").Result;

            if (rsp.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ExchangeResponse>(await rsp.Content.ReadAsStringAsync());
            }
            else
            {
                var e = JsonConvert.DeserializeObject<EpicError>(await rsp.Content.ReadAsStringAsync());
                this.excep($"Error: {e.errorCode}\nMessage: {e.errorMessage}");
                return null;
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="message">Argument to print in console.</param>
        void excep(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to restart the program.");
            Console.ReadKey();
            Console.Clear();
            new Program().start();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key">Key in map</param>
        /// <param name="value">Value in map</param>
        /// <returns>KeyValuePair</returns>
        KeyValuePair<string,string> createKey(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
