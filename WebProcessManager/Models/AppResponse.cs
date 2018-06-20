using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebProcessManager.Models
{
    public class AppResponse
    {
        public AppResponse(bool isError, string message = null)
        {
            IsError = isError;
            Message = message;
        }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
