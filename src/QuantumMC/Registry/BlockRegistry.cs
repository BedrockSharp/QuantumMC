using System;
using System.Collections.Generic;
using System.IO;
using QuantumMC.Blocks;

namespace QuantumMC.Registry
{
    public static class BlockRegistry
    {
        private static readonly Dictionary<int, Block> _byRuntimeId = new();
        private static readonly Dictionary<string, Block> _byIdentifier = new();
        
        public static void Init()
        {   
            using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("canonical_block_states.nbt");
            if (stream == null) throw new Exception("canonical_block_states.nbt not found");
            
            var blocks = new List<string>();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] buf = ms.ToArray();
            
            byte[] pattern = new byte[] { 0x08, 0x04, (byte)'n', (byte)'a', (byte)'m', (byte)'e' };
            
            int idx = 0;
            while ((idx = IndexOf(buf, pattern, idx)) != -1)
            {
                idx += pattern.Length;
                
                int nameLen = 0;
                int shift = 0;
                while (idx < buf.Length)
                {
                    byte b = buf[idx++];
                    nameLen |= (b & 0x7F) << shift;
                    if ((b & 0x80) == 0) break;
                    shift += 7;
                }
                
                if (idx + nameLen <= buf.Length)
                {
                    string name = System.Text.Encoding.UTF8.GetString(buf, idx, nameLen);
                    blocks.Add(name);
                }
            }

            int GetId(string name)
            {
                int index = blocks.IndexOf(name);
                if (index == -1) throw new Exception("Block not found in canonical NBT: " + name);
                return index;
            }

            _byRuntimeId.Clear();
            _byIdentifier.Clear();

            var blockTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in blockTypes)
            {
                if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
                {
                    var block = (Block)Activator.CreateInstance(type)!;
                    int runtimeId = GetId(block.Identifier);

                    var idProp = type.GetProperty("ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (idProp != null)
                    {
                        idProp.SetValue(null, runtimeId);
                    }

                    Register(block);
                }
            }
        }

        private static int IndexOf(byte[] source, byte[] pattern, int start)
        {
            for (int i = start; i <= source.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        private static void Register(Block block)
        {
            _byRuntimeId[block.RuntimeId] = block;
            _byIdentifier[block.Identifier] = block;
        }

        public static Block? GetByRuntimeId(int runtimeId)
        {
            _byRuntimeId.TryGetValue(runtimeId, out var block);
            return block;
        }

        public static Block? GetByIdentifier(string identifier)
        {
            _byIdentifier.TryGetValue(identifier, out var block);
            return block;
        }
    }
}
