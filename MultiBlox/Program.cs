using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management;

namespace MultiBlox
{
    partial class Program
    {
        /// <summary>
        /// the roblox event handle names we need to search for and close in order to run multiple roblox instances. 
        /// the order is important. the mutex must go first. 
        /// </summary>
        static readonly string[] _ROBLOX_EVENTS = ["ROBLOX_singletonMutex","ROBLOX_singletonEvent"];

        /// <summary>
        /// used to parse command output by lines
        /// </summary>
        [GeneratedRegex("\r\n|\r|\n")]
        private static partial Regex LineSplitRegex();

        /// <summary>
        /// event for keeping the console app running, will listen for ctrl+c to exit.
        /// </summary>
        static readonly ManualResetEvent _quitEvent = new(false);

        /// <summary>
        /// Calls "handle64" utility included in this project, which is provided by MS / SysInternals: 
        /// https://learn.microsoft.com/en-us/sysinternals/downloads/handle#usage 
        /// it parses the output and ignores the first 5 lines which are always the same header/license text 
        /// </summary>
        /// <param name="args">the command arguments string to pass to handle64 utility</param>
        /// <returns>the parsed output as an array of the lines of text</returns>
        static string[]? RunHandleProc(string args)
        {
            string? output = null;
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
        /// Searches for all the running Roblox instances & returns array of the process IDs.
        /// </summary>
        /// <returns>string array of roblox PIDs</returns>
        static string[] FindRobloxProcesses()
        {
            // get roblox processes ordered by longest running first
            var robloxProcs = Process.GetProcessesByName("RobloxPlayerBeta").OrderByDescending(x => DateTime.Now - x.StartTime).ToArray();
            string[] pids = new string[robloxProcs.Length];
            for(var i = 0; i < robloxProcs.Length; i++)
            {
                pids[i] = robloxProcs[i].Id.ToString();
            }
            return pids;
        }

        /// <summary>
        /// Searches for a Roblox Event Handle for a specified process ID.
        /// If found, closes the handle. 
        /// </summary>
        /// <param name="pid">Roblox Process ID</param>
        /// <param name="handleName">Name of event handle to close</param>
        /// <returns>boolean indicating whether found & closed handle</returns>
        static bool FindAndCloseRobloxEvent(string pid, string handleName)
        {
            bool foundAndClosed = false;
            Console.WriteLine("[PID:{0}] Searching for {1}...",pid,handleName);
            // call sysinternals to search for the roblox event handle 
            // see here for info on the args etc: 
            // https://learn.microsoft.com/en-us/sysinternals/downloads/handle#usage
            string[]? outlines = RunHandleProc(string.Format("-a -p {0} {1} -v -accepteula",pid,handleName)); // adding 'accepteula' arg bc it will fail if its never been accepted 
            if (outlines!=null) // if its null we got an error 
            {
                if (outlines.Length>=1 && outlines[0].Trim()=="No matching handles found.")
                {
                    //Console.WriteLine("[PID:{0}] {1} has already been closed for this instance.",pid,handleName);
                }
                else if (outlines.Length>=2 && outlines[0].Trim()=="Process,PID,Type,Handle,Name") // if we find the event handle, we end up here
                {
                    var handlevalues = outlines[1].Trim().Split(','); // the -v arg we used asks for CSV output, so we're splitting the values line by comma 
                    if (handlevalues.Length==5) // 5 heading names, so 5 values..
                    {
                        var _pid = handlevalues[1]; // process ID
                        var hid = handlevalues[3]; // handle ID

                        Console.WriteLine("[PID:{0}] {1} found! Closing...",_pid,handleName);
                        
                        // call sysinternals using -c arg to specify a handle to close
                        // the -y arg bypasses confirmation before closing handle 
                        // and the -p arg is where we're specifying which process ID 
                        outlines = RunHandleProc(string.Format("-c {0} -y -p {1}",hid,_pid));
                        if (outlines!=null)
                        {
                            if (outlines.Length>=3 && outlines[2].Trim()=="Handle closed.") // confirming output so we know that it worked 
                            {
                                Console.WriteLine("[PID:{0}] {1} closed!",_pid,handleName);
                                foundAndClosed = true;
                            }
                            else // otherwise smth must have gone wrong..
                            {
                                Console.WriteLine("[PID:{0}] ERROR! Unexpected output! Output was: \r\n{1}",_pid,string.Join('\n',outlines));
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[PID:{0}] ERROR! Unexpected number of handle values! Values were: \r\n{1}",pid,outlines[1].Trim());
                    }
                }
                else
                {
                    Console.WriteLine("[PID:{0}] ERROR! Unexpected output! Output was: \r\n{1}",pid,string.Join('\n',outlines));
                }
            }
            else
            {
                Console.WriteLine("[PID:{0}] Failed to locate {1}.",pid,handleName);
            }
            return foundAndClosed;
        }

        /// <summary>
        /// event called when a new roblox instance is started 
        /// will continually attempt to close the singleton handles until it succeeds 
        /// because they might not exist right away on process start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
            // getting the process ID from event args
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            var pid = targetInstance["Handle"].ToString();
            if (pid!=null)
            {
                Console.WriteLine("");
                Console.WriteLine("New Roblox Instance Detected! PID:{0}",pid);
                var evQueue = new Queue<string>(_ROBLOX_EVENTS); // make a new queue from the static event handle names
                string? currEv = null;
                while (evQueue.Count>0) // loop while the queue has anything in it 
                {
                    if (currEv==null) // if no current event handle name, get one from queue without removing any
                    {
                        if (!evQueue.TryPeek(out currEv))
                            break; // if queue empty then we're done, break from loop
                    }
                    if(currEv!=null)
                    {
                        if (FindAndCloseRobloxEvent(pid.ToString(),currEv)) // attempt to close the event handle
                        {
                            evQueue.Dequeue(); // closed this event handle so remove it from the queue
                            currEv = null; // clear the current event var so we get a new one on next loop iteration
                        }
                        else
                        {
                            //Console.WriteLine("[PID:{0}] {1} Not Found Yet",pid,currEv); // was just using this for testing.. dont think need 
                            Thread.Sleep(1000); // failed to find event handle on new process, so lets wait for 1 second before loop tries again
                        }
                    }
                }

                // finished, now just repeat the listening / exit verbiage for ease of use 
                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("Listening for new Roblox instances...");
                Console.WriteLine("Press [Ctrl+C] to stop/exit MultiBlox.");
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
            // this stuff listens for new roblox process instances 
            WqlEventQuery query = new("__InstanceCreationEvent", new TimeSpan(0,0,1),
                                      "TargetInstance isa \"Win32_Process\" and TargetInstance.Name = 'RobloxPlayerBeta.exe'");
            ManagementEventWatcher watcher = new(query);
            watcher.EventArrived += new EventArrivedEventHandler(OnEventArrived);
            watcher.Start();

            Console.WriteLine("Finding Roblox instances running...");
            var pids = FindRobloxProcesses(); // get list of roblox process IDs
            
            if (pids.Length>0)
            {
                Console.WriteLine("Found {0} Roblox instance(s) running. PIDS: {1}",pids.Length,string.Join(", ",pids));

                var results = new List<Tuple<string,string,bool>>();
                foreach(var pid in pids)
                {
                    Console.WriteLine(""); // just so there's a blank line between each roblox instance handle search output
                    // loop over the event handle names we need to search for & close (if found)
                    foreach(var ev in _ROBLOX_EVENTS)
                    {
                        // store results in a list of tuples
                        // PID, Handle Name, True/False if found/closed
                        results.Add(new Tuple<string,string,bool>(pid,ev,FindAndCloseRobloxEvent(pid,ev))); 
                    }
                }
                Console.WriteLine("");
                Console.WriteLine("Results:");
                bool foundAny = false;
                foreach(var tpl in results)
                {
                    Console.WriteLine("[PID:{0}] {1} {2}",tpl.Item1,tpl.Item2,tpl.Item3 ? "CLOSED" : "NOT FOUND");
                    if (tpl.Item3)
                        foundAny = true;
                }
                Console.WriteLine("");
                if (foundAny)
                {
                    Console.WriteLine("You can now open more Roblox instances! :D");
                }
                else
                {
                    Console.WriteLine("Looks like you already closed all Roblox singletons.");
                }
            }
            else
            {
                Console.WriteLine("Roblox not running.");
            }

            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Listening for new Roblox instances...");
            Console.WriteLine("Press [Ctrl+C] to stop/exit MultiBlox.");
            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            // kick off asynchronous stuff 

            _quitEvent.WaitOne();

            Console.WriteLine("Closing...");
            watcher.Stop();
            watcher.Dispose();

            // cleanup/shutdown and quit
        }
    }
}