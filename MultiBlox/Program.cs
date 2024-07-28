using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MultiBlox
{
    partial class Program
    {
        [GeneratedRegex("\r\n|\r|\n")]
        private static partial Regex LineSplitRegex();

        static string[] RunHandleProc(string args)
        {
            string output = null;
            using(Process pProcess = new Process())
            {
                pProcess.StartInfo.FileName = @"handle64.exe";
                pProcess.StartInfo.Arguments = args;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true; 
                try
                {
                    pProcess.Start();
                    output = pProcess.StandardOutput.ReadToEnd(); 
                    pProcess.WaitForExit(30000); // just in case.. dont hang for more than 30 sec...
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR: {0}",ex.Message);
                }
                
            }
            if (output!=null)
            {
                var outlines = LineSplitRegex().Split(output);
                if (outlines.Length>5)
                {
                    return outlines.Skip(5).ToArray();
                }
                else
                {
                    Console.WriteLine("ERROR: Unexpected number of output lines! Output was: \r\n{0}",output);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("ERROR Unknown Issue. Null output smh!");
                return null;
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Searching for Roblox singleton event...");
            string[] outlines = RunHandleProc("-a -p RobloxPlayerBeta.exe ROBLOX_singletonEvent -v -accepteula"); // adding 'accepteula' arg bc it will fail if its never been accepted 

            if (outlines!=null)
            {
                if (outlines.Length>=1 && outlines[0].Trim()=="No matching handles found.")
                {
                    Console.WriteLine("Roblox is not running OR the singleton event has already been closed.\r\nRun the first instance of Roblox and try again.");
                }
                else if (outlines.Length>=2 && outlines[0].Trim()=="Process,PID,Type,Handle,Name")
                {
                    var handlevalues = outlines[1].Trim().Split(',');
                    if (handlevalues.Length==5)
                    {
                        Console.WriteLine("Roblox singleton event found! Closing...");
                        var pid = handlevalues[1];
                        var hid = handlevalues[3];
                        outlines = RunHandleProc(string.Format("-c {0} -y -p {1}",hid,pid));
                        if (outlines!=null)
                        {
                            if (outlines.Length>=3 && outlines[2].Trim()=="Handle closed.")
                            {
                                Console.WriteLine("Roblox singleton event closed!");
                                Console.WriteLine("You may now open additional Roblox instances! :D");
                            }
                            else
                            {
                                Console.WriteLine("ERROR! Unexpected output! Output was: \r\n{0}",string.Join('\n',outlines));
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR! Unexpected number of handle values! Values were: \r\n{0}",outlines[1].Trim());
                    }
                }
                else
                {
                    Console.WriteLine("ERROR! Unexpected output! Output was: \r\n{0}",string.Join('\n',outlines));
                }
            }
            else
            {
                Console.WriteLine("Failed to locate Roblox singleton.");
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Press [ENTER] key to exit.");
            Console.ReadLine();
        }
    }
}