using System.Net;
using BedrockProtocol;
using RaknetCS.Network;
using Serilog;
using QuantumMC.World;

namespace QuantumMC.Network
{
    public class Network
    {
        private readonly RaknetListener _listener;
        private readonly SessionManager _sessionManager;
        private readonly int _port;
        private readonly int _maxPlayers;
        public MotdAdvertisement Advertisement { get; }

        public SessionManager SessionManager => _sessionManager;
        public World.World World { get; }

        public Network(int port, int maxPlayers)
        {
            _port = port;
            _maxPlayers = maxPlayers;
            _sessionManager = new SessionManager();
            World = new World.World(new FlatWorldGenerator());
            Advertisement = new MotdAdvertisement
            {
                Motd = "QuantumMC Server",
                SubMotd = "QuantumMC",
                Protocol = BedrockProtocol.Protocol.CurrentProtocol.ToString(),
                Version = BedrockProtocol.Protocol.MinecraftVersion,
                MaxPlayers = _maxPlayers,
                Port = _port
            };

            var endpoint = new IPEndPoint(IPAddress.Any, _port);
            _listener = new RaknetListener(endpoint);

            UpdateMotd();

            _listener.SessionConnected += OnSessionConnected;
        }

        public void Start()
        {
            _listener.BeginListener();
        }

        public void Stop()
        {
            _listener.StopListener();
        }

        private void OnSessionConnected(RaknetSession rakSession)
        {
            Log.Information("New RakNet session from {EndPoint}", rakSession.PeerEndPoint);

            var playerSession = new PlayerSession(rakSession, _sessionManager)
            {
                World = this.World
            };
            _sessionManager.AddSession(rakSession.PeerEndPoint, playerSession);

            UpdateMotd();
        }

        public void UpdateMotd()
        {
            Advertisement.OnlineCount = _sessionManager.OnlineCount;
            _listener.Motd = Advertisement.ToString();
        }
    }

    public class MotdAdvertisement
    {
        public string Motd { get; set; } = "QuantumMC Server";
        public string SubMotd { get; set; } = "QuantumMC";
        public string Protocol { get; set; } = "944";
        public string Version { get; set; } = "1.26.10";
        public int OnlineCount { get; set; } = 0;
        public int MaxPlayers { get; set; } = 20;
        public long ServerId { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public string GameMode { get; set; } = "Survival";
        public int Port { get; set; } = 19132;

        public override string ToString()
        {
            return $"MCPE;{Motd};{Protocol};{Version};{OnlineCount};{MaxPlayers};{ServerId};{SubMotd};{GameMode};1;{Port};{Port + 1};";
        }
    }
}