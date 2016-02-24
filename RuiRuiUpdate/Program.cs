using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuiRuiUpdate
{
    public class Program
    {
        private const string Url = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases/download/v{0}/Hearthstone.Deck.Tracker-v{0}.zip";

        //TODO: Pull from git and build instead of this dirty local network hack, or use something like an automated builder


        public static void Main(string[] args)
        {
            Console.Title = "RuiRui Updater";
            Console.CursorVisible = false;
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid arguments");
                
            }
            else
            try
            {
                //wait for tracker to shut down
                Thread.Sleep(1000);

                var procId = int.Parse(args[0]);
                if (Process.GetProcesses().Any(p => p.Id == procId))
                {
                    Process.GetProcessById(procId).Kill();
                    Thread.Sleep(5000);
                    Console.WriteLine("Killed RuiRuiBot process");
                }
            }
            catch
            {
                return;
            }


            var update = Update();
            update.Wait();

        }
#pragma warning disable 1998
        private static async Task Update()
#pragma warning restore 1998
        {
            try
            {
                Console.WriteLine("Extracting files...");
                Copy(@"C:\Rui\temp", @"C:\Rui\app");
                Process.Start("RuiRuiConsole.exe");
            }
            catch
            {
                Console.WriteLine("There was a problem updating to the latest version.");
                Console.ReadKey();
            }
            finally
            {
                try
                {
                    Console.WriteLine("Cleaning up...");

                    Console.WriteLine("Done!");
                }
                catch
                {
                    Console.WriteLine("Failed to delete temp file directory");
                }
            }
        }


        private static void Copy(string source, string destination) {
            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(source, destination), true);
                File.Delete(newPath);
                Console.WriteLine("Writing {0}", newPath);
            }
        }
    }
}
