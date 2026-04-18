namespace QuantumMC.Config
{
    public class ServerConfig
    {
        // Network
        public int Port { get; set; } = 19132;
        public int MaxPlayers { get; set; } = 20;

        // MOTD
        public string Motd { get; set; } = "A QuantumMC Server";
        public string SubMotd { get; set; } = "QuantumMC";
        public string GameMode { get; set; } = "Survival";

        // World
        public string WorldName { get; set; } = "world";
        public string WorldGenerator { get; set; } = "flat";
    }
}
