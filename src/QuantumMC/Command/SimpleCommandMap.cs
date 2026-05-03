using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Packets.Types;
using ProtocolCommand = BedrockProtocol.Packets.Types.Command;

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

            sender.SendMessage($"§cUnknown command: /{sentCommandLabel}. Type /help for the list of available commands.");
            return false;
        }

        public void ClearCommands() => KnownCommands.Clear();

        public Command? GetCommand(string name) => KnownCommands.GetValueOrDefault(name.ToLower());
        
        public IReadOnlyDictionary<string, Command> GetCommands() => KnownCommands;

        public AvailableCommandsPacket GetAvailableCommandsPacket(ICommandSender sender)
        {
            var packet = new AvailableCommandsPacket();
            var seen = new HashSet<string>();

            foreach (var kvp in KnownCommands)
            {
                var cmd = kvp.Value;
                if (seen.Contains(cmd.Name.ToLower()))
                    continue;

                if (!cmd.TestPermission(sender))
                    continue;

                seen.Add(cmd.Name.ToLower());

                uint aliasesOffset = 0xFFFFFFFF;

                if (cmd.Aliases.Count > 0)
                {
                    var aliasEnum = new CommandEnum
                    {
                        Type = cmd.Name.ToLower() + "Aliases",
                        ValueIndices = new List<uint>()
                    };

                    uint baseIndex = (uint)packet.EnumValues.Count;
                    packet.EnumValues.Add(cmd.Name.ToLower());
                    aliasEnum.ValueIndices.Add(baseIndex);

                    foreach (var alias in cmd.Aliases)
                    {
                        packet.EnumValues.Add(alias.ToLower());
                        aliasEnum.ValueIndices.Add((uint)(packet.EnumValues.Count - 1));
                    }

                    aliasesOffset = (uint)packet.Enums.Count;
                    packet.Enums.Add(aliasEnum);
                }

                var protocolCmd = new ProtocolCommand
                {
                    Name = cmd.Name.ToLower(),
                    Description = cmd.Description ?? string.Empty,
                    Flags = 0,
                    PermissionLevel = string.IsNullOrEmpty(cmd.Permission) 
                        ? CommandPermissionLevel.Any 
                        : CommandPermissionLevel.GameDirectors,
                    AliasesOffset = aliasesOffset,
                    Overloads = new List<CommandOverload>
                    {
                        new CommandOverload
                        {
                            Chaining = false,
                            Parameters = new List<CommandParameter>()
                        }
                    }
                };

                packet.Commands.Add(protocolCmd);
            }

            return packet;
        }
    }
}
