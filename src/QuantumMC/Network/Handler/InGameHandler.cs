using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class InGameHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            switch ((PacketIds)packetId)
            {
                case PacketIds.Text:
                    HandleText(session, payload);
                    break;
                case PacketIds.MovePlayer:
                    HandleMovePlayer(session, payload);
                    break;
                case PacketIds.PlayerAuthInput:
                    HandlePlayerAuthInput(session, payload);
                    break;
                case PacketIds.Animate:
                    HandleAnimate(session, payload);
                    break;
            }
        }

        private void HandleMovePlayer(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new MovePlayerPacket();
            packet.Decode(stream);

            session.Player.X = packet.X;
            session.Player.Y = packet.Y;
            session.Player.Z = packet.Z;
            session.Player.Pitch = packet.Pitch;
            session.Player.Yaw = packet.Yaw;
            session.Player.HeadYaw = packet.HeadYaw;

            session.Player.UpdateChunks();

            var broadcastPacket = new MovePlayerPacket
            {
                RuntimeEntityId = session.Player.EntityRuntimeId,
                X = packet.X,
                Y = packet.Y,
                Z = packet.Z,
                Pitch = packet.Pitch,
                Yaw = packet.Yaw,
                HeadYaw = packet.HeadYaw,
                Mode = packet.Mode,
                OnGround = packet.OnGround,
                RidingEntityId = packet.RidingEntityId,
                Tick = packet.Tick
            };

            Server.Instance.Network.BroadcastPacket(broadcastPacket, true, session);
        }

        private void HandlePlayerAuthInput(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new PlayerAuthInputPacket();
            packet.Decode(stream);

            session.Player.X = packet.PositionX;
            session.Player.Y = packet.PositionY;
            session.Player.Z = packet.PositionZ;
            session.Player.Pitch = packet.Pitch;
            session.Player.Yaw = packet.Yaw;
            session.Player.HeadYaw = packet.HeadYaw;

            session.Player.UpdateChunks();

            var broadcastPacket = new MovePlayerPacket
            {
                RuntimeEntityId = session.Player.EntityRuntimeId,
                X = packet.PositionX,
                Y = packet.PositionY,
                Z = packet.PositionZ,
                Pitch = packet.Pitch,
                Yaw = packet.Yaw,
                HeadYaw = packet.HeadYaw,
                Mode = MoveMode.Normal,
                OnGround = packet.Tick > 0,
                RidingEntityId = 0,
                Tick = packet.Tick
            };

            Server.Instance.Network.BroadcastPacket(broadcastPacket, true, session);
        }

        private void HandleText(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new TextPacket();
            packet.Decode(stream);

            Log.Information("{Username}: {Text}", session.Username, packet.Message);

            Server.Instance.Network.BroadcastPacket(packet);
        }

        private void HandleAnimate(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new AnimatePacket();
            packet.Decode(stream);

            Server.Instance.Network.BroadcastPacket(packet, true, session);
        }
    }
}
