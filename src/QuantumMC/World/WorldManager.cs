using Serilog;
using System.Collections.Concurrent;
using System.IO;

namespace QuantumMC.World
{
    public class WorldManager
    {
        public static WorldManager Instance { get; private set; } = default!;

        private readonly ConcurrentDictionary<string, World> _worlds = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _worldsFolder;

        public WorldManager()
        {
            Instance = this;
            _worldsFolder = Path.Combine(QuantumMC.DataFolder, "worlds");

            if (!Directory.Exists(_worldsFolder))
            {
                Directory.CreateDirectory(_worldsFolder);
            }
        }

        public void LoadWorlds()
        {
            Log.Debug("Loading worlds from {Path}...", _worldsFolder);

            string defaultWorldName = Server.Instance.Config.WorldName;
            string defaultWorldPath = Path.Combine(_worldsFolder, defaultWorldName);

            if (!Directory.Exists(defaultWorldPath))
            {
                Log.Warning("Default world '{WorldName}' not found. Creating it...", defaultWorldName);
                CreateWorld(defaultWorldName);
            }

            foreach (var worldDir in Directory.GetDirectories(_worldsFolder))
            {
                string worldName = Path.GetFileName(worldDir);
                if (_worlds.ContainsKey(worldName)) continue;

                try
                {
                    LoadWorld(worldName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load world '{WorldName}'", worldName);
                }
            }

            Log.Information("Loaded {Count} worlds.", _worlds.Count);
        }

        /// <summary>
        /// Loads a specific world by name from the worlds folder.
        /// </summary>
        public World LoadWorld(string name)
        {
            if (_worlds.TryGetValue(name, out var existingWorld))
            {
                return existingWorld;
            }

            string worldPath = Path.Combine(_worldsFolder, name);
            if (!Directory.Exists(worldPath))
            {
                throw new DirectoryNotFoundException($"World directory not found: {worldPath}");
            }

            Log.Information("Loading world: {WorldName}", name);

            IWorldGenerator generator = new FlatWorldGenerator();
            
            var provider = new LevelDBWorldProvider(worldPath);
            var world = new World(generator, provider);
            
            _worlds.TryAdd(name, world);
            return world;
        }

        /// <summary>
        /// Creates a new world directory and then loads it.
        /// </summary>
        public World CreateWorld(string name)
        {
            string worldPath = Path.Combine(_worldsFolder, name);
            if (!Directory.Exists(worldPath))
            {
                Directory.CreateDirectory(worldPath);
            }
            
            return LoadWorld(name);
        }

        /// <summary>
        /// Gets a loaded world by name.
        /// </summary>
        public World? GetWorld(string name)
        {
            _worlds.TryGetValue(name, out var world);
            return world;
        }

        /// <summary>
        /// Returns all currently loaded worlds.
        /// </summary>
        public IEnumerable<World> GetAllWorlds() => _worlds.Values;
    }
}
