using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
                var httpContent = new StringContent("{'UserName':'admin@localhost', 'Password':'Echo@1927'}", Encoding.UTF8, "application/json");
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

        public async Task<AppResponse> StartAsync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var result = await client.PostAsync(process.Container.Address + "/api/process/run/" + process.Id, null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }

            }
        }

        public async Task<AppResponse> CreateProcess(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var p = new ProcessModels.ProcessForAdd()
                    {
                        Id = process.Id,
                        Application = process.Application,
                        IsRunning = false,
                        Arguments = process.Arguments,
                        AutoRestart = process.AutoRestart
                    };

                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(p),
                        Encoding.UTF8,
                        "application/json");

                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return new AppResponse(true, "Container not found!");
                    }
                    var result = await client.PostAsync(cont.Address + "/api/process", httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }

            }
        }

        public async Task<AppResponse> StopAsync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var result = await client.PostAsync(process.Container.Address + "/api/process/stop/" + process.Id, null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public Task<AppResponse> SetAutoRestart(Process process)
        {
            throw new NotImplementedException();
        }

        public Task<AppResponse> StopAll(List<Process> procesList)
        {
            throw new NotImplementedException();
        }

        public Task<AppResponse> RunAll(List<Process> procesList)
        {
            throw new NotImplementedException();
        }

        public async Task<AppResponse> EditAsync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var p = new ProcessModels.ProcessForAdd()
                    {
                        Id = process.Id,
                        Application = process.Application,
                        IsRunning = process.IsRunning,
                        Arguments = process.Arguments,
                        AutoRestart = process.AutoRestart
                    };

                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(p),
                        Encoding.UTF8,
                        "application/json");

                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return new AppResponse(true, "Container not found!");
                    }
                    var result = await client.PutAsync(cont.Address + "/api/process", httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public async Task<AppResponse> RemoveASync(int id)
        {
            using (var client = new HttpClient())
            {
                var process = _context.Processes.Include(x=>x.Container).SingleOrDefault(x => x.Id == id);
                if(process == null) return new AppResponse(true, "process not found!");

                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return new AppResponse(true, "Container not found!");
                    }

                    var result = await client.DeleteAsync(cont.Address + "/api/process/" + process.Id);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public async Task<AppResponse> StopAllAsync(Container cont)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(cont.Id))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[cont.Id];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var result = await client.PostAsync(cont.Address + "/api/process/stopall", null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public async Task<AppResponse> StartAllAsync(Container cont)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(cont.Id))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[cont.Id];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var result = await client.PostAsync(cont.Address + "/api/process/startall", null);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public async Task<AppResponse> Sync(Process process)
        {
            using (var client = new HttpClient())
            {
                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return new AppResponse(true, "Code: 401");
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var p = new ProcessModels.ProcessForAdd()
                    {
                        Id = process.Id,
                        Application = process.Application,
                        IsRunning = process.IsRunning,
                        Arguments = process.Arguments,
                        AutoRestart = process.AutoRestart
                    };

                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(p),
                        Encoding.UTF8,
                        "application/json");

                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return new AppResponse(true, "Container not found!");
                    }
                    var result = await client.PostAsync(cont.Address + "/api/process/sync", httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = JsonConvert.DeserializeObject<AppResponse>(result.Content.ReadAsStringAsync().Result);
                        return new AppResponse(content.IsError, content.Message);
                    }
                    return new AppResponse(true, "Code: " + result.StatusCode);
                }
                catch (Exception e)
                {
                    return new AppResponse(true, e.ToString());
                }
            }
        }

        public async Task<List<Report>> GetLogsAsync(int id, DateTime from, DateTime to)
        {
            using (var client = new HttpClient())
            {
                var process = _context.Processes.SingleOrDefault(x => x.Id == id);
                if (process == null) return null;

                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return null;
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return null;
                    }

                    var p = new FormGetLog() {From = from, To = to};
                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(p),
                        Encoding.UTF8,
                        "application/json");

                    var result = await client.PostAsync(cont.Address + "/api/process/log/" + id, httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<List<Report>>(json);
                        return data;
                    }
                }
                catch (Exception)
                {
                    return null;
                }

                return null;
            }
        }

        public async Task<List<Report>> GetErrorLogsAsync(int id, DateTime from, DateTime to)
        {
            using (var client = new HttpClient())
            {
                var process = _context.Processes.SingleOrDefault(x => x.Id == id);
                if (process == null) return null;

                if (!TokenRepository.AccessTokens.ContainsKey(process.ContainerId))
                {
                    return null;
                }

                var token = TokenRepository.AccessTokens[process.ContainerId];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                try
                {
                    var cont = _context.Containers.SingleOrDefault(x => x.Id == process.ContainerId);
                    if (cont == null)
                    {
                        return null;
                    }

                    var p = new FormGetLog() { From = from, To = to };
                    var httpContent = new StringContent(
                        JsonConvert.SerializeObject(p),
                        Encoding.UTF8,
                        "application/json");

                    var result = await client.PostAsync(cont.Address + "/api/process/error/" + id, httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<List<Report>>(json);
                        return data;
                    }
                }
                catch (Exception)
                {
                    return null;
                }

                return null;
            }
        }
    }

    public class FormGetLog
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
