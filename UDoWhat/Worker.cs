using System.Runtime.InteropServices;
using System.Text;

namespace UDoWhat
{
    public class Worker : BackgroundService
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var handle1 = GetConsoleWindow();

            ShowWindow(handle1, SW_HIDE);

            while (!stoppingToken.IsCancellationRequested)
            {
                IntPtr handle = GetForegroundWindow();
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(handle, sb, 256);
                string windowTitle = sb.ToString();

                if (File.Exists(DateTime.Today.ToString("yyyy-mm-dd") + ".csv"))
                {
                    // read last line
                    string lastLine = File.ReadLines(DateTime.Today.ToString("yyyy-mm-dd") + ".csv").Last();
                    // split proccess name
                    string windowNameFromFile = lastLine.Split(',')[0];
                    // check if the window title is the same as the last line
                    if (windowTitle == windowNameFromFile)
                    {
                        // if the window title is the same, modify the end time to the current time
                        string line = File.ReadAllLines(DateTime.Today.ToString("yyyy-mm-dd") + ".csv").Last();
                        line = windowTitle + "," + line.Split(',')[1] + "," + DateTime.Now.ToString("HH:mm:ss");
                        File.WriteAllLines(DateTime.Today.ToString("yyyy-mm-dd") + ".csv", File.ReadAllLines(DateTime.Today.ToString("yyyy-mm-dd") + ".csv").Take(File.ReadAllLines(DateTime.Today.ToString("yyyy-mm-dd") + ".csv").Count() - 1).Concat(new[] { line }));
                    }
                    else
                    {
                        // if the window title is different, add the new window title to the file
                        File.AppendAllText(DateTime.Today.ToString("yyyy-mm-dd") + ".csv", windowTitle + "," + DateTime.Now.ToString("HH:mm:ss") + "," + DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine);
                    }
                }
                else
                {
                    // if the file does not exist, create it and add the window title to the file
                    File.AppendAllText(DateTime.Today.ToString("yyyy-mm-dd") + ".csv", windowTitle + "," + DateTime.Now.ToString("HH:mm:ss") + "," + DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine);
                }

                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}