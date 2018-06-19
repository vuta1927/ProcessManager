using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebProcessManager.Core
{
    public static class TokenRepository
    {
        public static Dictionary<int, string> AccessTokens = new Dictionary<int, string>();
    }
}
