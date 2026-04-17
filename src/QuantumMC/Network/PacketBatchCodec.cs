using BedrockProtocol.Packets;
using BedrockProtocol.Utils;
using System.IO.Compression;

namespace QuantumMC.Network
{
    public static class PacketBatchCodec
    {
        private const byte GAME_PACKET_HEADER = 0xFE;

        public static List<(uint packetId, byte[] payload)> Decode(byte[] data, bool compressionReady)
        {
            var packets = new List<(uint, byte[])>();

            if (data.Length < 1 || data[0] != GAME_PACKET_HEADER)
                return packets;

            Serilog.Log.Debug("Decode Raw Data (len={Len}): {Hex}", data.Length, BitConverter.ToString(data));

            var stream = new BinaryStream(data);
            stream.Position = 1;

            byte[] batchPayload;

            if (compressionReady)
            {
                byte algorithm = stream.ReadByte();
                
                if (algorithm == 0x00) // Zlib
                {
                    long remaining = data.Length - stream.Position;
                    byte[] compressed = stream.ReadBytes((int)remaining);
                    batchPayload = ZlibDecompress(compressed);
                }
                else if (algorithm == 0xFF) // None
                {
                    long remaining = data.Length - stream.Position;
                    batchPayload = stream.ReadBytes((int)remaining);
                }
                else
                {
                    throw new Exception($"Unknown compression algorithm: {algorithm}");
                }
            }
            else
            {
                long remaining = data.Length - stream.Position;
                batchPayload = stream.ReadBytes((int)remaining);
            }

            var batchStream = new BinaryStream(batchPayload);

            while (!batchStream.Eof)
            {
                uint length = batchStream.ReadUnsignedVarInt();
                byte[] packetData = batchStream.ReadBytes((int)length);

                var packetStream = new BinaryStream(packetData);
                uint header = packetStream.ReadUnsignedVarInt();
                uint packetId = header & 0x3FF;

                long remaining = packetData.Length - packetStream.Position;
                byte[] payload = packetStream.ReadBytes((int)remaining);

                packets.Add((packetId, payload));
            }

            return packets;
        }

        public static byte[] Encode(Packet packet, bool compressionReady, int threshold = 256)
        {
            return EncodeBatch(new List<Packet> { packet }, compressionReady, threshold);
        }

        public static byte[] EncodeBatch(List<Packet> packets, bool compressionReady, int threshold = 256)
        {
            var bodyStream = new BinaryStream();

            foreach (var packet in packets)
            {
                var packetStream = new BinaryStream();
                uint header = packet.PacketId & 0x3FF; // 10 bits packet id
                packetStream.WriteUnsignedVarInt(header);
                packet.Encode(packetStream);
                byte[] packetBody = packetStream.GetBuffer();

                bodyStream.WriteUnsignedVarInt((uint)packetBody.Length);
                bodyStream.WriteBytes(packetBody);
            }

            return CreateBatch(bodyStream.GetBuffer(), compressionReady, threshold);
        }

        private static byte[] CreateBatch(byte[] payloads, bool compressionReady, int threshold)
        {
            var batchStream = new BinaryStream();
            batchStream.WriteByte(GAME_PACKET_HEADER);

            if (compressionReady)
            {
                if (payloads.Length >= threshold)
                {
                    batchStream.WriteByte(0x00); // Zlib
                    byte[] compressed = ZlibCompress(payloads);
                    batchStream.WriteBytes(compressed);
                }
                else
                {
                    batchStream.WriteByte(0xFF); // None
                    batchStream.WriteBytes(payloads);
                }
            }
            else
            {
                batchStream.WriteBytes(payloads);
            }

            return batchStream.GetBuffer();
        }

        private static byte[] ZlibDecompress(byte[] data)
        {
            using var memStream = new MemoryStream(data);
            using var deflateStream = new DeflateStream(memStream, CompressionMode.Decompress);
            using var outStream = new MemoryStream();
            deflateStream.CopyTo(outStream);
            return outStream.ToArray();
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using var outStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(outStream, CompressionLevel.Fastest))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return outStream.ToArray();
        }
    }
}
