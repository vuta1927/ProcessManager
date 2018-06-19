using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public async Task GetTokenFromContainer(Container conn)
        {
            using (var client = new HttpClient())
            {
                var httpContent = new StringContent("{'UserName':'admin@localhost', 'Password':'Echo@1927'}",Encoding.UTF8, "application/json");
                var result = await client.PostAsync(conn.Address + "/Account/RequestToken", httpContent);
                if (result.IsSuccessStatusCode)
                {
                    var cont = result.Content.ReadAsStringAsync();
                    TokenRepository.AccessTokens.Add(conn.Id, cont.Result);
                }
            }
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
                var httpContent = new StringContent(JsonConvert.SerializeObject("{'Authorization': 'Bearer " + token + "'}"), Encoding.UTF8, "application/json");

                var result = await client.PostAsync(process.Container.Address + "/api/process/run/" + process.Id, httpContent);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
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
                var result = await client.PostAsync(process.Container.Address + "/api/process/stop/" + process.Id, httpContent);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
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
