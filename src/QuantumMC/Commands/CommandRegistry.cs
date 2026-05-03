using System.Reflection;

namespace QuantumMC.Commands
{
    public static class CommandRegistry
    {
        private static Dictionary<string, CommandExecutor> _commands = new();

        static CommandRegistry()
        {
            _commands["help"] = new Default.HelpCommand();
            _commands["tps"] = new Default.TpsCommand();
            _commands["version"] = new Default.VersionCommand();
        }

        public static void AutoRegister()
        {
            var executorType = typeof(CommandExecutor);
            var registryType = typeof(ICommandRegistry);

            var types = Assembly.GetExecutingAssembly().GetTypes();

            // Auto register default commands
            foreach (var type in types.Where(t => t.IsClass && !t.IsAbstract && executorType.IsAssignableFrom(t)))
            {
                string commandName = type.Name.Replace("Command", "").ToLower();
                CommandExecutor? executor = Activator.CreateInstance(type) as CommandExecutor;

                if (executor != null)
                {
                    _commands[commandName] = executor;
                    Console.WriteLine($"[CommandRegistry] Registered: /{commandName}");
                }
            }

            // Find and run all custom registries
            foreach (var type in types.Where(t => t.IsClass && !t.IsAbstract && registryType.IsAssignableFrom(t)))
            {
                ICommandRegistry? customRegistry = Activator.CreateInstance(type) as ICommandRegistry;
                customRegistry?.RegisterCommands(CommandRegistry.Register);
                Console.WriteLine($"[CommandRegistry] Loaded custom registry: {type.Name}");
            }
        }

        public static void Register(string commandName, CommandExecutor executor)
        {
            _commands[commandName.ToLower()] = executor;
        }

        public static bool Dispatch(ICommandSender sender, string label, string[] args)
        {
            if (_commands.TryGetValue(label.ToLower(), out CommandExecutor? executor))
            {
                Command command = new Command(label);
                return executor.OnCommand(sender, command, label, args);
            }

            sender.SendMessage($"§cUnknown command: /{label}");
            return false;
        }

        public static IReadOnlyDictionary<string, CommandExecutor> GetCommands() => _commands;
    }
}