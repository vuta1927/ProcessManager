using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManagerCore.Models
{
    public class AppResponse
    {
        public AppResponse(bool isError, string mess)
        {
            IsError = isError;
            Message = mess;
        }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
