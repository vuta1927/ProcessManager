using System.Collections.Generic;
using System.Threading.Tasks;
using WebProcessManager.Models;

namespace WebProcessManager.Core
{
    public interface IContainerComunicate
    {
        void GetTokenFromContainer(Container conn);
        string GetToken(int conntainerId);
        Task<bool> StartAsync(Process process);
        Task<bool> StopAsync(Process process);
        Task SetAutoRestart(Process process);
        Task StopAll(List<Process> procesList);
        Task RunAll(List<Process> procesList);
    }
}