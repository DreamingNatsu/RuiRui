using Discord;
using Discord.Commands;
using Discord.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuiRuiBot;
using RuiRuiBot.Botplugins.Tools;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiTests
{
    [TestClass]
    public class PluginSelectorTests : DiscordTest {

        [ClassInitialize]
        public static void Init(TestContext t){
            Initialize(new PluginSelector(),new TestModule());
        } 
        [ClassCleanup]
        public static void Clean() => Cleanup();

        [TestMethod]
        public void Test()
        {
            string text = $"test_{Random.Next()}";
            AssertEvent<MessageEventArgs>(
            "MessageCreated event never received",
            () => HostClient.SendMessage(TestServerChannel, text),
            x => ObserverBot.MessageReceived += x,
            x => ObserverBot.MessageReceived -= x,
            (s, e) => e.Message.Text == text);
        }

    }

    public class TestModule : IModule {
        public void Install(ModuleManager manager){
            manager.CreateCommands("", man =>{
                man.CreateCommand("test").Do(m=> "test");
            });
        }
    }
}
