using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessManagerCore.Core
{
    public interface IServerCommunicate
    {
        void GetTokenFromServer();
        void ProcessStopHandler();
    }
}