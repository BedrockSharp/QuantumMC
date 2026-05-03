namespace QuantumMC.Command
{
    public abstract class Command
    {
        public string Name { get; protected set; }
        public string Description { get; set; }
        public string UsageMessage { get; set; }
        public string Permission { get; set; }
        public List<string> Aliases { get; set; }

        protected Command(string name, string description = "", string usageMessage = "", List<string>? aliases = null)
        {
            Name = name;
            Description = description;
            UsageMessage = usageMessage;
            Aliases = aliases ?? new List<string>();
        }

        public abstract bool Execute(ICommandSender sender, string commandLabel, string[] args);

        public bool TestPermission(ICommandSender sender)
        {
            if (string.IsNullOrEmpty(Permission) || sender.HasPermission(Permission))
            {
                return true;
            }

            return false;
        }
    }
}
