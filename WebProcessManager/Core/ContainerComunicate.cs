using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebProcessManager.Data;
using WebProcessManager.Models;

namespace WebProcessManager.Core
{
    public class ContainerComunicate : IContainerComunicate
    {
        private ApplicationDbContext _context;
        public ContainerComunicate(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }
        public void GetTokenFromContainer(Container cont)
        {
            using (var client = new HttpClient())
            {
                var httpContent = new StringContent("{'UserName':'admin@localhost', 'Password':'Echo@1927'}",Encoding.UTF8, "application/json");
                try
                {
                    var response = client.PostAsync(cont.Address + "/RequestToken", httpContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var token = response.Content.ReadAsStringAsync().Result;

                        if (!string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                var handler = new JwtSecurityTokenHandler();
                                var tokenString = handler.ReadToken(token) as JwtSecurityToken;
                                var experiedTime = int.Parse(tokenString.Claims.First(claim => claim.Type == "exp").Value);
                                var currentUnixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                var delayTime = (experiedTime - currentUnixTimestamp) * 1000;

                                if (TokenRepository.AccessTokens.ContainsKey(cont.Id))
                                {
                                    TokenRepository.AccessTokens[cont.Id] = token;
                                }
                                else
                                {
                                    TokenRepository.AccessTokens.Add(cont.Id, token);
                                }

                                //Get token after experied
                                Task.Run(() => RefreshToken(delayTime, cont));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Task.Run(() => RefreshToken(10000, cont));
                }
            }
        }

        private void RefreshToken(int delay, Container cont)
        {
            Thread.Sleep(delay);
            GetTokenFromContainer(cont);
        }

        public string GetToken(int conntainerId)
        {
            if (!TokenRepository.AccessTokens.ContainsKey(conntainerId)) return null;

            return TokenRepository.AccessTokens[conntainerId];
        }

        public async Task<bool> StartAsync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return false;
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var result = await client.PostAsync(process.Container.Address + "/api/process/run/" + process.Id, null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = result.Content.ReadAsStringAsync().Result;
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
                
            }
        }

        public async Task<bool> StopAsync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return false;
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                var httpContent = new StringContent(JsonConvert.SerializeObject("{'Authorization': 'Bearer "+ token + "'}"),Encoding.UTF8, "application/json");
                try
                {
                    var result = await client.PostAsync(process.Container.Address + "/api/process/stop/" + process.Id, httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
        }

        public Task SetAutoRestart(Process process)
        {
            throw new NotImplementedException();
        }

        public Task StopAll(List<Process> procesList)
        {
            throw new NotImplementedException();
        }

        public Task RunAll(List<Process> procesList)
        {
            throw new NotImplementedException();
        }
    }
}
