using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProcessManagerCore.Models;

namespace ProcessManagerCore.Core
{
    public interface IServerCommunicate
    {
        void GetTokenFromServer();
        void ProcessOnStopHandler(Process p, EventArgs e);
        void ProcessOnStartHandler(Process p, EventArgs e);
        void RunAllProcess();
    }
}