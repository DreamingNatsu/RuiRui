using System.Threading.Tasks;

namespace RuiRuiConsole {
    internal interface IOutput {
        Task WriteLine(string text);
        void Clear();
    }
}