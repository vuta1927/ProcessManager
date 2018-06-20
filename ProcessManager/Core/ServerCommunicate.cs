using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ProcessManagerCore.Core
{
    public class ServerCommunicate : IServerCommunicate
    {
        private readonly IConfiguration _configuration;
        public ServerCommunicate(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void GetTokenFromServer()
        {
            using (var client = new HttpClient())
            {
                var httpContent = new StringContent("{'UserName':'admin@localhost', 'Password':'Echo@1927'}",Encoding.UTF8, "application/json");
                var url = _configuration["JwtIssuer"] + "/connect/token";
                try
                {
                    var result = client.PostAsync(url, httpContent).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var token = result.Content.ReadAsStringAsync().Result;

                        if (!string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                var handler = new JwtSecurityTokenHandler();
                                var tokenString = handler.ReadToken(token) as JwtSecurityToken;
                                var experiedTime = int.Parse(tokenString.Claims.First(claim => claim.Type == "exp").Value);
                                var currentUnixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                var delayTime = (experiedTime - currentUnixTimestamp) * 1000;

                                TokenRepository.token = token;

                                //Get token after experied
                                Task.Run(() => RefreshToken(delayTime));
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Task.Run(() => RefreshToken(10000));
                }
                
            }
        }

        private void RefreshToken(int delay)
        {
            Thread.Sleep(delay);
            GetTokenFromServer();
        }

        public void ProcessStopHandler()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TokenRepository.token);
                try
                {
                    var result = await client.PostAsync("/api/process/run/" + process.Id, null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = result.Content.ReadAsStringAsync().Result;
                        return new AppResponse(false, content);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }

            }
        }
    }
}
