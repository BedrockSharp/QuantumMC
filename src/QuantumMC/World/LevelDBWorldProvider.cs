using MiNET.LevelDB;
using Serilog;
using System;
using System.IO;

namespace QuantumMC.World
{
    public class LevelDBWorldProvider : IWorldProvider, IDisposable
    {
        public string LevelName { get; private set; } = "world";
        public int SpawnX { get; private set; } = 0;
        public int SpawnY { get; private set; } = 66;
        public int SpawnZ { get; private set; } = 0;

        private readonly IDatabase _db;
        private readonly string _path;

        public LevelDBWorldProvider(string worldPath)
        {
            LevelName = Path.GetFileName(worldPath);
            _path = Path.Combine(worldPath, "db");
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            var options = new Options();
            
            _db = new Database(new DirectoryInfo(_path), true, options);
            _db.Open();

            Log.Debug("LevelDBWorldProvider initialized at {Path}", _path);

            string levelDatPath = Path.Combine(worldPath, "level.dat");
            if (File.Exists(levelDatPath))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(levelDatPath);
                    if (data.Length > 8)
                    {
                        using var ms = new MemoryStream(data, 8, data.Length - 8);
                        var nbtFile = new Nbt.NbtFile();
                        nbtFile.LoadFromStream(ms, Nbt.NbtCompression.None);

                        if (nbtFile.RootTag != null)
                        {
                            var root = nbtFile.RootTag;
                            var levelNameTag = root["LevelName"];
                            if (levelNameTag != null) LevelName = levelNameTag.StringValue ?? LevelName;

                            var spawnXTag = root["SpawnX"];
                            if (spawnXTag != null) SpawnX = spawnXTag.IntValue;

                            var spawnYTag = root["SpawnY"];
                            if (spawnYTag != null) SpawnY = spawnYTag.IntValue;

                            var spawnZTag = root["SpawnZ"];
                            if (spawnZTag != null) SpawnZ = spawnZTag.IntValue;
                            
                            Log.Information("Loaded world '{LevelName}' with spawn ({SpawnX}, {SpawnY}, {SpawnZ})", LevelName, SpawnX, SpawnY, SpawnZ);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to parse local level.dat");
                }
            }
        }

        public Chunk? LoadChunk(int x, int z)
        {
            try
            {
                byte[] versionKey = GetKey(x, z, 118); // Chunk version v118
                var versionData = _db.Get(versionKey);

                if (versionData == null || versionData.Length == 0)
                {
                    return null;
                }

                var chunk = new Chunk(x, z);
                bool foundData = false;

                for (sbyte y = Chunk.SubChunkIndexOffset; y < Chunk.SubChunkIndexOffset + Chunk.SubChunkCount; y++)
                {
                    byte[] subChunkKey = GetKey(x, z, 47, unchecked((byte)y));
                    var subChunkData = _db.Get(subChunkKey);
                    
                    if (subChunkData == null)
                    {
                        subChunkKey = GetKey(x, z, 44, unchecked((byte)y));
                        subChunkData = _db.Get(subChunkKey);
                    }

                    if (subChunkData != null && subChunkData.Length > 0)
                    {
                        foundData = true;
                        // TODO: Implement full Mojang SubChunk Disk Paletted Block deserialization
                        // For now we return null so the generator can at least provide some terrain
                        Log.Warning("Chunk at {X},{Z} SubChunk {Y} has data ({Len} bytes) but deserialization is TODO. Falling back to generator.", x, z, y, subChunkData.Length);
                        return null; 
                    }
                }

                if (!foundData) return null;

                return chunk;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load chunk at {X}, {Z} from LevelDB", x, z);
                return null;
            }
        }

        public void SaveChunk(Chunk chunk)
        {
            try
            {
                _db.Put(GetKey(chunk.ChunkX, chunk.ChunkZ, 118), new byte[] { 8 }); // Version 8

                // TODO: SubChunk serialization
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save chunk at {X}, {Z} to LevelDB", chunk.ChunkX, chunk.ChunkZ);
            }
        }

        /// <summary>
        /// Generates a Bedrock LevelDB chunk key
        /// </summary>
        private static byte[] GetKey(int x, int z, byte tag, byte? subChunkY = null)
        {
            // Format: X (4) + Z (4) + Tag (1) [+ SubChunkY (1)] 
            int len = subChunkY.HasValue ? 10 : 9;
            var key = new byte[len];

            BitConverter.GetBytes(x).CopyTo(key, 0);
            BitConverter.GetBytes(z).CopyTo(key, 4);

            key[8] = tag;
            
            if (subChunkY.HasValue) 
            {
                key[9] = subChunkY.Value;
            }

            return key;
        }

        public void Dispose()
        {
            _db?.Close();
            _db?.Dispose();
        }
    }
}
