using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ProcessManagerCore
{
    public class Program
    {
        public static Core.ProcessManager PManager;
        public static void Main(string[] args)
        {
            PManager = new Core.ProcessManager("process.json");
            PManager.RunAll();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
