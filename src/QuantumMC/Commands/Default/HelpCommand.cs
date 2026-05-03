using QuantumMC.Utils;

namespace QuantumMC.Commands.Default
{
    public class HelpCommand : CommandExecutor
    {
        public override bool OnCommand(ICommandSender sender, Command command, string label, string[] args)
        {
            sender.SendMessage(TextFormat.Yellow + "Server Help Guide");
            sender.SendMessage("");
            sender.SendMessage("/help - Shows this help message");
            sender.SendMessage("/version - Shows Server Version");
            sender.SendMessage("/plugins - Shows Server Plugins");
            sender.SendMessage("/tps - Shows Server Performance");

            return true;
        }

    }
}