using System.Linq;
using Discord.Modules;
using RuiRuiBot.Services;

namespace RuiRuiBot.ExtensionMethods {
    public static class ModuleExtensions {
        public static string GetName(this IModule module)
            => module.GetType().Name;

        public static bool IsLocked(this IModule module)
            => module.GetType().GetCustomAttributes(typeof (LockedPluginAttribute), true).Any();

    }
}