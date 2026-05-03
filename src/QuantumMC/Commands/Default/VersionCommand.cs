using QuantumMC.Utils;

namespace QuantumMC.Commands.Default
{
    public class VersionCommand : CommandExecutor
    {
        public override bool OnCommand(ICommandSender sender, Command command, string label, string[] args)
        {
            sender.SendMessage(TextFormat.MinecoinGold + "Server Version");
            sender.SendMessage("");
            sender.SendMessage($"Version: {Utils.Version.Current}");
            sender.SendMessage("");
            
            return true;
        }

    }
}