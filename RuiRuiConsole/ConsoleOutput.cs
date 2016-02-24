using System;
using System.Threading.Tasks;

namespace RuiRuiConsole {
    internal class ConsoleOutput : IOutput {

        public Task WriteLine(string text){
            lock (this) {
                Console.WriteLine(text);
                return Task.CompletedTask;
            }
            
        }

        public void Clear(){
            lock (this) {
                Console.Clear();
            }
        }
    }
}