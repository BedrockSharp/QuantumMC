using System.Net;
using BedrockPacket = BedrockProtocol.Packets.Packet;
using RaknetCS.Network;
using RaknetCS.Protocol.Raknet;
using Serilog;
using Org.BouncyCastle.Crypto;
using QuantumMC.Network.Handler;
using QuantumMC.World;

namespace QuantumMC.Network
{
    public class PlayerSession
    {
        public RaknetSession RakSession { get; }
        public IPEndPoint EndPoint => RakSession.PeerEndPoint;
        public SessionState State { get; set; } = SessionState.HandshakePhase;
        public bool CompressionReady { get; set; } = false;
        public string Username { get; set; } = string.Empty;

        public bool EncryptionEnabled { get; set; } = false;
        public byte[]? AesKey { get; set; }
        public byte[]? IvBase { get; set; }
        public ulong SendCounter { get; set; } = 0;
        public ulong ReceiveCounter { get; set; } = 0;
        
        public BedrockStreamCipher? Encryptor { get; private set; }
        public BedrockStreamCipher? Decryptor { get; private set; }

        public World.World? World { get; set; }
        public int ChunkRadius { get; set; } = 8;

        private readonly SessionManager _sessionManager;

        public PlayerSession(RaknetSession rakSession, SessionManager sessionManager)
        {
            RakSession = rakSession;
            _sessionManager = sessionManager;

            rakSession.SessionReceiveRaw += OnRawPacketReceived;
            rakSession.SessionDisconnected += OnDisconnected;
        }

        private void OnRawPacketReceived(IPEndPoint address, byte[] data)
        {
            if (data.Length < 1)
                return;

            if (data[0] != 0xFE)
                return;

            try
            {
                var decoded = PacketBatchCodec.Decode(data, this);

                foreach (var (packetId, payload) in decoded)
                {
                    PacketDispatcher.Dispatch(this, packetId, payload);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing packet from {EndPoint}", EndPoint);
            }
        }

        public void SendPacket(BedrockPacket packet, bool immediate = true)
        {
            try
            {
                byte[] encoded = PacketBatchCodec.Encode(packet, this);
                RakSession.Send(Reliability.ReliableOrdered, encoded, immediate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error sending packet {PacketType} to {EndPoint}", packet.GetType().Name, EndPoint);
            }
        }

        public void InitializeEncryption(byte[] key, byte[] iv)
        {
            AesKey = key;
            IvBase = iv;
            Encryptor = EncryptionUtils.CreateCipher(true, key, iv);
            Decryptor = EncryptionUtils.CreateCipher(false, key, iv);
            EncryptionEnabled = true;
            
            Log.Information("Encryption initialized for {Username} ({EndPoint})", Username, EndPoint);
        }

        public void Disconnect()
        {
            Log.Information("Disconnecting player {Username} ({EndPoint})", Username, EndPoint);
            _sessionManager.RemoveSession(EndPoint);
        }

        private void OnDisconnected(RaknetSession session)
        {
            Log.Information("Player {Username} ({EndPoint}) disconnected", Username, EndPoint);
            _sessionManager.RemoveSession(EndPoint);
        }
    }
}
