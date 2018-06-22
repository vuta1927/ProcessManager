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
using ProcessManagerCore.Models;

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
                var httpContent = new StringContent("{'UserName':'admin@localhost', 'Password':'Echo@1927'}", Encoding.UTF8, "application/json");
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

                                Task.Run(() => RunAllProcess());

                                //Get token after experied
                                Task.Run(() => RefreshToken(delayTime));

                                //Auto sync process to server every 1 mins
                                Task.Run(() => SyncProcess());
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        Task.Run(() => RefreshToken(5000));
                    }
                }
                catch (Exception)
                {
                    Task.Run(() => RefreshToken(5000));
                }

            }
        }

        private void RefreshToken(int delay)
        {
            Thread.Sleep(delay);
            GetTokenFromServer();
        }

        public void RunAllProcess()
        {
            var listP = Program.PManager.GetAll();
            foreach (var p in listP)
            {
                p.OnStart += ProcessOnStartHandler;
                p.OnStop += ProcessOnStopHandler;
                p.Start();
            }
        }

        private void SyncProcess()
        {
            var a = Program.PManager.GetAll();
            var all = new List<ProcessModels.ProcessForSync>();
            {
                foreach (var p in a)
                {
                    all.Add(new ProcessModels.ProcessForSync()
                    {
                        Application = p.Application,
                        Arguments = p.Arguments,
                        Id = p.Id,
                        IsRunning = p.IsRunning,
                        AutoRestart = p.AutoRestart,
                        ContainerUrl = _configuration["ContainerUrl"]
                    });
                }
            }

            using (var client = new HttpClient())
            {
                Thread.Sleep(60000);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TokenRepository.token);
                try
                {
                    var url = _configuration["JwtIssuer"] + "/api/process/syncall";
                    var content = new StringContent(JsonConvert.SerializeObject(all), Encoding.UTF8, "application/json");
                    var result = client.PostAsync(url, content).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var da = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<AppResponse>(da);
                        if (response.IsError)
                        {
                            Task.Run(() => SyncProcess());
                        }
                        var ids = JsonConvert.DeserializeObject<Dictionary<int,int>>(response.Message);
                        UpdateProcessId(ids);

                        LogHelper.Add(new LogMessage() { FileName = _configuration["mainLogFile"] + ".out", Message = "Success sync all process to server!"});
                    }
                    else
                    {
                        LogHelper.Add(new LogMessage(){ FileName = _configuration["mainLogFile"] + ".error", Message = "Falsed to send SyncProcess request to Server: " + result.StatusCode });
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Add(new LogMessage() { FileName = _configuration["mainLogFile"] + ".error", Message = "Falsed to sync all process to Server. Error: " + e });
                    Task.Run(() => SyncProcess());
                }

                Task.Run(() => SyncProcess());
            }
        }

        private void UpdateProcessId(Dictionary<int, int> ids)
        {
            var all = Program.PManager.GetAll();
            foreach (var newid in ids)
            {
                var old = all.SingleOrDefault(x => x.Id == newid.Key);
                if(old != null)
                {
                    old.Id = newid.Value;
                } 
            }
            Program.PManager.ReloadProcessFile();
        }

        public void ProcessOnStopHandler(Process p, EventArgs ev)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TokenRepository.token);
                try
                {
                    var url = _configuration["JwtIssuer"] + "/api/process/stopped/" + p.Id;
                    var result = client.PostAsync(url, null).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<AppResponse>(json);

                        if (response.IsError)
                        {
                            LogHelper.Add(new LogMessage() { FileName = p.Id + ".error", Message = response.Message });
                        }
                        else
                        {
                            LogHelper.Add(new LogMessage() { FileName = p.Id + ".out", Message = "process stopped!" });
                        }
                        return;
                    }
                    LogHelper.Add(new LogMessage() { FileName = p.Id + ".error", Message = "process stop handler falsed to send notification: " + result.StatusCode.ToString() });
                    Task.Run(() =>
                    {
                        Thread.Sleep(3000);
                        ProcessOnStopHandler(p, ev);
                    });
                }
                catch (Exception e)
                {
                    LogHelper.Add(new LogMessage() { FileName = p.Id + "error", Message = e.ToString() });
                }

            }
        }
        public void ProcessOnStartHandler(Process p, EventArgs ev)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TokenRepository.token);
                try
                {
                    var url = _configuration["JwtIssuer"] + "/api/process/started/" + p.Id;
                    var result = client.PostAsync(url, null).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<AppResponse>(json);

                        if (response.IsError)
                        {
                            LogHelper.Add(new LogMessage() { FileName = p.Id + ".error", Message = response.Message });
                        }
                        else
                        {
                            LogHelper.Add(new LogMessage() { FileName = p.Id + ".out", Message = "process started!" });
                        }
                        return;
                    }
                    LogHelper.Add(new LogMessage() { FileName = p.Id + ".error", Message = "process started handler falsed to send notification: " + result.StatusCode.ToString() });
                    Task.Run(() =>
                        {
                            Thread.Sleep(3000);
                            ProcessOnStartHandler(p, ev);
                        });
                }
                catch (Exception e)
                {
                    LogHelper.Add(new LogMessage() { FileName = p.Id + ".error", Message = e.ToString() });
                }

            }
        }

    }
}
