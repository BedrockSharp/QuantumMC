using System.IO;
using MiNET.LevelDB;
using Nbt;
using Serilog;

namespace QuantumMC.Player
{
    public class LevelDBPlayerProvider : IPlayerProvider, IDisposable
    {
        private readonly string _basePath;
        private static readonly object _lock = new object();

        public LevelDBPlayerProvider(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
            
            Log.Debug("LevelDBPlayerProvider base path set to {Path}", _basePath);
        }

        private string GetPlayerPath(string username)
        {
            return Path.Combine(_basePath, username.ToLowerInvariant());
        }

        public void SavePlayer(Player player)
        {
            if (string.IsNullOrEmpty(player.Username)) return;

            string path = GetPlayerPath(player.Username);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            lock (_lock)
            {
                Database? db = null;
                try
                {
                    var options = new Options();
                    db = new Database(new DirectoryInfo(path), true, options);
                    db.Open();

                    var compound = new CompoundTag("");
                    compound.Add(new StringTag("Username", player.Username));
                    compound.Add(new StringTag("Xuid", player.Xuid));
                    compound.Add(new StringTag("Uuid", player.Uuid));
                    compound.Add(new IntTag("GameMode", player.Gamemode));
                    
                    var posList = new ListTag("Pos");
                    posList.Add(new FloatTag(player.X));
                    posList.Add(new FloatTag(player.Y));
                    posList.Add(new FloatTag(player.Z));
                    compound.Add(posList);

                    var rotList = new ListTag("Rotation");
                    rotList.Add(new FloatTag(player.Yaw));
                    rotList.Add(new FloatTag(player.Pitch));
                    compound.Add(rotList);

                    if (player.World != null)
                    {
                        compound.Add(new StringTag("World", player.World.Name));
                    }

                    compound.Add(new ListTag("Inventory", TagType.Compound));
                    
                    using var ms = new MemoryStream();
                    var nbtFile = new NbtFile(compound);
                    nbtFile.BigEndian = false;
                    nbtFile.SaveToStream(ms, NbtCompression.None);
                    
                    byte[] data = ms.ToArray();
                    db.Put(System.Text.Encoding.UTF8.GetBytes("data"), data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to save player database for {Username}", player.Username);
                }
                finally
                {
                    db?.Close();
                    db?.Dispose();
                }
            }
        }

        public bool LoadPlayer(Player player)
        {
            if (string.IsNullOrEmpty(player.Username)) return false;

            string path = GetPlayerPath(player.Username);
            if (!Directory.Exists(path)) return false;

            lock (_lock)
            {
                Database? db = null;
                try
                {
                    var options = new Options();
                    db = new Database(new DirectoryInfo(path), true, options);
                    db.Open();

                    byte[]? data = db.Get(System.Text.Encoding.UTF8.GetBytes("data"));
                    if (data == null || data.Length == 0) return false;

                    using var ms = new MemoryStream(data);
                    var nbtFile = new NbtFile();
                    nbtFile.BigEndian = false;
                    nbtFile.LoadFromStream(ms, NbtCompression.None);

                    var compound = nbtFile.RootTag;
                    if (compound == null) return false;

                    player.Username = compound["Username"]?.StringValue ?? player.Username;
                    player.Xuid = compound["Xuid"]?.StringValue ?? player.Xuid;
                    player.Gamemode = compound["GameMode"]?.IntValue ?? player.Gamemode;

                    if (compound["Pos"] is ListTag posList && posList.Count >= 3)
                    {
                        player.X = posList[0].FloatValue;
                        player.Y = posList[1].FloatValue;
                        player.Z = posList[2].FloatValue;
                    }

                    if (compound["Rotation"] is ListTag rotList && rotList.Count >= 2)
                    {
                        player.Yaw = rotList[0].FloatValue;
                        player.Pitch = rotList[1].FloatValue;
                    }

                    string? worldName = compound["World"]?.StringValue;
                    if (!string.IsNullOrEmpty(worldName))
                    {
                        player.World = Server.Instance.WorldManager.GetWorld(worldName);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Player database for {Username} not found or corrupted", player.Username);
                    return false;
                }
                finally
                {
                    db?.Close();
                    db?.Dispose();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
