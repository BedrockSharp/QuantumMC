using System.Net;
using BedrockProtocol;
using QuantumMC.Network;
using RaknetCS.Network;
using Serilog;

namespace QuantumMC
{
    public class Server
    {
        private readonly RaknetListener _listener;
        private readonly SessionManager _sessionManager;
        private readonly int _port;
        private readonly int _maxPlayers;
        private bool _running;

        public Server(int port = 19132, int maxPlayers = 20)
        {
            _port = port;
            _maxPlayers = maxPlayers;
            _sessionManager = new SessionManager();

            var endpoint = new IPEndPoint(IPAddress.Any, _port);
            _listener = new RaknetListener(endpoint);

            UpdateMotd();

            _listener.SessionConnected += OnSessionConnected;
        }

        public void Start()
        {
            _running = true;

            Log.Information("  ____                    _                   __  __  ____ ");
            Log.Information(" / __ \\                  | |                 |  \\/  |/ ___|");
            Log.Information("| |  | |_   _  __ _ _ __ | |_ _   _ _ __ ___ | |\\/| | |    ");
            Log.Information("| |  | | | | |/ _` | '_ \\| __| | | | '_ ` _ \\| |  | | |    ");
            Log.Information("| |__| | |_| | (_| | | | | |_| |_| | | | | | | |  | | |___ ");
            Log.Information(" \\___\\_\\\\__,_|\\__,_|_| |_|\\__|\\__,_|_| |_| |_|_|  |_|\\____|");
            Log.Information("");
            Log.Information("QuantumMC — Minecraft: Bedrock Edition Server");
            Log.Information("Protocol: {Protocol} | Version: {Version}", Protocol.CurrentProtocol, Protocol.MinecraftVersion);
            Log.Information("Listening on port {Port} (Max players: {MaxPlayers})", _port, _maxPlayers);
            Log.Information("");

            _listener.BeginListener();

            Log.Information("Server started! Waiting for connections...");

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Stop();
            };

            while (_running)
            {
                Thread.Sleep(50);
            }
        }

        public void Stop()
        {
            if (!_running) return;
            _running = false;

            Log.Information("Stopping server...");

            _listener.StopListener();

            Log.Information("Server stopped.");
        }

        private void OnSessionConnected(RaknetSession rakSession)
        {
            Log.Information("New RakNet session from {EndPoint}", rakSession.PeerEndPoint);

            var playerSession = new PlayerSession(rakSession, _sessionManager);
            _sessionManager.AddSession(rakSession.PeerEndPoint, playerSession);

            UpdateMotd();
        }

        private void UpdateMotd()
        {
            _listener.Motd = $"MCPE;QuantumMC Server;{Protocol.CurrentProtocol};{Protocol.MinecraftVersion};{_sessionManager.OnlineCount};{_maxPlayers};{DateTimeOffset.Now.ToUnixTimeMilliseconds()};QuantumMC;Survival;1;{_port};{_port + 1};";
        }
    }
}
