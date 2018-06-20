using System.Collections.Generic;
using System.Threading.Tasks;
using WebProcessManager.Models;

namespace WebProcessManager.Core
{
    public interface IContainerComunicate
    {
        void GetTokenFromContainer(Container conn);
        string GetToken(int conntainerId);
        Task<AppResponse> CreateProcess(Process process);
        Task<AppResponse> StartAsync(Process process);
        Task<AppResponse> StopAsync(Process process);
        Task<AppResponse> SetAutoRestart(Process process);
        Task<AppResponse> StopAll(List<Process> procesList);
        Task<AppResponse> RunAll(List<Process> procesList);
    }
}