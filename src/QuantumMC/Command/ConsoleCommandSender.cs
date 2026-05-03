using Serilog;
using QuantumMC.Utils;

namespace QuantumMC.Command
{
    public class ConsoleCommandSender : ICommandSender
    {

        public void SendMessage(string message)
        {
            Log.Information(TextFormat.ToAnsi(message));
        }

        public Server GetServer() => Server.Instance;

        public string GetName() => "CONSOLE";

        public bool HasPermission(string permission) => true;
    }
}
