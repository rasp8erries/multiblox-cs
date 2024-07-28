using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MultiBlox
{
    partial class Program
    {
        /// <summary>
        /// used to parse command output by lines
        /// </summary>
        [GeneratedRegex("\r\n|\r|\n")]
        private static partial Regex LineSplitRegex();

        /// <summary>
        /// Calls "handle64" utility included in this project, which is provided by MS / SysInternals: 
        /// https://learn.microsoft.com/en-us/sysinternals/downloads/handle#usage 
        /// it parses the output and ignores the first 5 lines which are always the same header/license text 
        /// </summary>
        /// <param name="args">the command arguments string to pass to handle64 utility</param>
        /// <returns>the parsed output as an array of the lines of text</returns>
        static string[] RunHandleProc(string args)
        {
            string output = null;
            using(Process pProcess = new Process())
            {
                pProcess.StartInfo.FileName = @"handle64.exe"; // sysinternals util included in this proj's build 
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
                    Console.WriteLine("ERROR: {0}",ex.Message); // smth wtf happen lol
                }
                
            }
            if (output!=null) // if its null we got an error 
            {
                var outlines = LineSplitRegex().Split(output); // split the output by lines of text 
                if (outlines.Length>5) // should always be more than 5 lines of output bc always has 5 initial lines of just header/license text 
                {
                    return outlines.Skip(5).ToArray();
                }
                else
                {
                    Console.WriteLine("ERROR: Unexpected number of output lines! Output was: \r\n{0}",output); // otherwise smth is very wrong..
                    return null;
                }
            }
            else
            {
                Console.WriteLine("ERROR Unknown Issue. Null output smh!");
                return null;
            }
        }

        /// <summary>
        /// main execution begins here.
        /// but u should know this. 
        /// if ur pokin around. 
        /// in here. 
        /// :D
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Searching for Roblox singleton event...");
            // call sysinternals to search for the roblox singleton handle 
            // see here for info on the args etc: 
            // https://learn.microsoft.com/en-us/sysinternals/downloads/handle#usage
            string[] outlines = RunHandleProc("-a -p RobloxPlayerBeta.exe ROBLOX_singletonEvent -v -accepteula"); // adding 'accepteula' arg bc it will fail if its never been accepted 

            if (outlines!=null) // again, if its null we got an error 
            {
                if (outlines.Length>=1 && outlines[0].Trim()=="No matching handles found.") 
                {
                    Console.WriteLine("Roblox is not running OR the singleton event has already been closed.\r\nRun the first instance of Roblox and try again.");
                }
                else if (outlines.Length>=2 && outlines[0].Trim()=="Process,PID,Type,Handle,Name") // if we find the singleton handle, we end up here
                {
                    var handlevalues = outlines[1].Trim().Split(','); // the -v arg we used asks for CSV output, so we're splitting the values line by comma 
                    if (handlevalues.Length==5) // 5 heading names, so 5 values..
                    {
                        Console.WriteLine("Roblox singleton event found! Closing...");
                        var pid = handlevalues[1]; // process ID
                        var hid = handlevalues[3]; // handle ID
                        // call sysinternals using -c arg to specify a handle to close
                        // the -y arg bypasses confirmation before closing handle 
                        // and the -p arg is where we're specifying which process ID 
                        outlines = RunHandleProc(string.Format("-c {0} -y -p {1}",hid,pid));
                        if (outlines!=null)
                        {
                            if (outlines.Length>=3 && outlines[2].Trim()=="Handle closed.") // confirming output so we know that it worked 
                            {
                                Console.WriteLine("Roblox singleton event closed!");
                                Console.WriteLine("You may now open additional Roblox instances! :D");
                            }
                            else // otherwise smth must have gone wrong..
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