namespace QuantumMC.Command
{
    public class SimpleCommandMap : ICommandMap
    {
        protected readonly Dictionary<string, Command> KnownCommands = new();
        private readonly Server _server;

        public SimpleCommandMap(Server server)
        {
            _server = server;
            RegisterDefaultCommands();
        }

        private void RegisterDefaultCommands()
        {
            Register("quantum", new Default.VersionCommand());
        }

        public void Register(string fallbackPrefix, Command command)
        {
            string name = command.Name.ToLower();
            KnownCommands[name] = command;

            foreach (var alias in command.Aliases)
            {
                KnownCommands[alias.ToLower()] = command;
            }
        }

        public void RegisterAll(string fallbackPrefix, List<Command> commands)
        {
            foreach (var command in commands)
            {
                Register(fallbackPrefix, command);
            }
        }

        public bool Dispatch(ICommandSender sender, string commandLine)
        {
            var args = commandLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 0)
            {
                return false;
            }

            var sentCommandLabel = args[0].ToLower();
            if (sentCommandLabel.StartsWith("/"))
            {
                sentCommandLabel = sentCommandLabel.Substring(1);
            }

            var commandArgs = args.Skip(1).ToArray();

            if (KnownCommands.TryGetValue(sentCommandLabel, out var command))
            {
                if (!command.TestPermission(sender))
                {
                    sender.SendMessage("§cYou don't have permission to use this command.");
                    return false;
                }

                try
                {
                    return command.Execute(sender, sentCommandLabel, commandArgs);
                }
                catch (Exception ex)
                {
                    sender.SendMessage($"§cAn error occurred while executing the command: {ex.Message}");
                    return false;
                }
            }

            sender.SendMessage($"§cUnknown command: /{sentCommandLabel}. Type 'help' for the list of available commands.");
            return false;
        }

        public void ClearCommands() => KnownCommands.Clear();

        public Command? GetCommand(string name) => KnownCommands.GetValueOrDefault(name.ToLower());
        
        public IReadOnlyDictionary<string, Command> GetCommands() => KnownCommands;
    }
}
