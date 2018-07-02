using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebProcessManager.Models;

namespace WebProcessManager.Core
{
    public interface IContainerComunicate
    {
        void GetTokenFromContainer(Container conn);
        string GetToken(int conntainerId);
        Task<AppResponse> Sync(Process process);
        Task<AppResponse> CreateProcess(Process process);
        Task<AppResponse> StartAsync(Process process);
        Task<AppResponse> StopAsync(Process process);
        Task<AppResponse> EditAsync(Process process);
        Task<AppResponse> RemoveASync(int id);
        Task<List<Report>> GetLogsAsync(int id, DateTime from, DateTime to);
        Task<List<Report>> GetErrorLogsAsync(int id, DateTime from, DateTime to);
        Task<AppResponse> StopAllAsync(Container cont);
        Task<AppResponse> StartAllAsync(Container cont);
        Task<AppResponse> SetAutoRestart(Process process);
        Task<AppResponse> StopAll(List<Process> procesList);
        Task<AppResponse> RunAll(List<Process> procesList);
    }
}