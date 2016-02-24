using System.Collections.Generic;
using System.Threading.Tasks;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiConsole {
    internal class CompositeOutput : IOutput {
        private readonly IEnumerable<IOutput> _outputs; 
        public CompositeOutput(params IOutput[] outputs){
            _outputs = outputs;
        }
        public async Task WriteLine(string text){
            await _outputs.ForEachAsync(async o=>await o.WriteLine(text));
        }

        public void Clear(){
             _outputs.ForEach(o =>  o.Clear());
        }
    }
}