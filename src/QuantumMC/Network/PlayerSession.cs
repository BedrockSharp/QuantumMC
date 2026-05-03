using System.Net;
using BedrockPacket = BedrockProtocol.Packets.Packet;
using RaknetCS.Network;
using RaknetCS.Protocol.Raknet;
using Serilog;
using Org.BouncyCastle.Crypto;
using QuantumMC.Network.Handler;
using QuantumMC.World;
using QuantumMC.Event.Impl;
using QuantumMC.Utils;

namespace QuantumMC.Network
{
    public class PlayerSession
    {
        public RaknetSession RakSession { get; }
        public IPEndPoint EndPoint => RakSession.PeerEndPoint;
        public SessionState State { get; set; } = SessionState.HandshakePhase;
        public Player.Player Player { get; }
        public string Username { get; set; } = string.Empty;
        public bool CompressionReady { get; set; } = false;
        
        public bool EncryptionEnabled { get; set; } = false;
        public byte[]? AesKey { get; set; }
        public byte[]? IvBase { get; set; }
        public ulong SendCounter { get; set; } = 0;
        public ulong ReceiveCounter { get; set; } = 0;
        
        public BedrockStreamCipher? Encryptor { get; private set; }
        public BedrockStreamCipher? Decryptor { get; private set; }

        private readonly SessionManager _sessionManager;

        public PlayerSession(RaknetSession rakSession, SessionManager sessionManager)
        {
            RakSession = rakSession;
            _sessionManager = sessionManager;
            Player = new Player.Player(this);

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
        }

        public void Disconnect()
        {
            Server.Instance.PlayerProvider.SavePlayer(Player);
            _sessionManager.RemoveSession(EndPoint);
        }

        private void OnDisconnected(RaknetSession session)
        {
            var playerQuitEvent = new PlayerQuitEvent(Player, "%multiplayer.player.left");
            Server.Instance.PluginManager.EventManager.CallEventAsync(playerQuitEvent).GetAwaiter().GetResult();

            var playerKickEvent = new PlayerKickEvent(Player, "%multiplayer.player.left");
            Server.Instance.PluginManager.EventManager.CallEventAsync(playerKickEvent).GetAwaiter().GetResult();

            if (playerQuitEvent.QuitMessage != null)
            {
                Server.Instance.SendTranslation(TextFormat.Yellow + playerQuitEvent.QuitMessage, [Username]);
            }
            
            if (playerKickEvent.KickMessage != null)
            {
                Server.Instance.SendTranslation(TextFormat.Red + playerKickEvent.KickMessage, [Username]);
            }

            Log.Information("Player {Username} ({EndPoint}) disconnected", Username, EndPoint);
            Server.Instance.PlayerProvider.SavePlayer(Player);
            _sessionManager.RemoveSession(EndPoint);
        }
    }
}
