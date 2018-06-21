namespace WebProcessManager.Models
{
    public static class ProcessModels
    {
        public class ProcessForAddToFile
        {
            public int Id { get; set; }
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool AutoRestart { get; set; }
        }
        public class ProcessForAdd
        {
            public int Id { get; set; }
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool IsRunning { get; set; }
            public bool AutoRestart { get; set; }
        }
        public class ProcessForSync
        {
            public int Id { get; set; }
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool IsRunning { get; set; }
            public bool AutoRestart { get; set; }
            public string ContainerUrl { get; set; }
        }
        public class ProcessForView : Process
        { 
            public string Output { get; set; }
            public string Errors { get; set; }
        }
    }
}
