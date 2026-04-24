using System.Net;
using BedrockProtocol;
using QuantumMC.Network;
using QuantumMC.Config;
using RaknetCS.Network;
using Serilog;

namespace QuantumMC
{
    public class Server
    {
        public static Server Instance { get; private set; } = default!;
        
        private readonly int _port;
        private readonly int _maxPlayers;
        private bool _running;
        public ServerConfig Config;
        public World.WorldManager WorldManager;
        public Player.IPlayerProvider PlayerProvider;
        public Network.Network Network;

        public Server(ServerConfig config)
        {
            Instance = this;

            Config = config;
            _port = config.Port;
            _maxPlayers = config.MaxPlayers;

            WorldManager = new World.WorldManager();
            PlayerProvider = new Player.LevelDBPlayerProvider(Path.Combine(QuantumMC.DataFolder, "players"));
            Network = new Network.Network(config);
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
            Log.Information("QuantumMC - Minecraft: Bedrock Edition Server");
            Log.Information("Protocol: {Protocol} | Version: {Version}", Protocol.CurrentProtocol, Protocol.MinecraftVersion);
            Log.Information("Listening on port {Port} (Max players: {MaxPlayers})", _port, _maxPlayers);

            Registry.BlockRegistry.Init();
            WorldManager.LoadWorlds();
            
            Network.Start();
            Log.Information("Server started! Type /help for the list of available commands.");

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
            Network.Stop();
            if (PlayerProvider is IDisposable disposable) disposable.Dispose();
            Log.Information("Server has stopped.");
        }
    }
}
