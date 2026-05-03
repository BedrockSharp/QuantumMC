namespace QuantumMC.Commands
{
    public class Command
    {
        public string Label { get; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Permission { get; set; }
        public string[] Aliases { get; set; }

        public Command(string label, string description = "", string usage = "", string permission = "", string[]? aliases = null)
        {
            Label = label;
            Description = description;
            Usage = usage;
            Permission = permission;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }
}