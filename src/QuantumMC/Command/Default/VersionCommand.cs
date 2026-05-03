using QuantumMC.Utils;

namespace QuantumMC.Command.Default
{
    public class VersionCommand : Command
    {
        public VersionCommand() : base("version", "Shows the server version", "/version", new List<string> { "ver" })
        {
        }

        public override bool Execute(ICommandSender sender, string commandLabel, string[] args)
        {
            sender.SendMessage(TextFormat.MinecoinGold + "This server is running QuantumMC version " + Utils.Version.Current);
            return true;
        }
    }
}
