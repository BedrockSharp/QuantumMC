using BedrockProtocol;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Types;
using Serilog;
using Nbt;
using System.Collections.Generic;
using System;

namespace QuantumMC.Network.Handler
{
    public class SessionStartPacketHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            switch ((PacketIds)packetId)
            {
                case PacketIds.RequestNetworkSettings:
                    HandleRequestNetworkSettings(session, payload);
                    break;
                case PacketIds.ResourcePackClientResponse:
                    HandleResourcePackClientResponse(session, payload);
                    break;
            }
        }

        private void HandleRequestNetworkSettings(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new RequestNetworkSettingsPacket();
            packet.Decode(stream);

            int clientProtocol = System.Net.IPAddress.NetworkToHostOrder(packet.ProtocolVersion);

            Log.Information("Received RequestNetworkSettings from {EndPoint} (Protocol: {Protocol})", session.EndPoint, clientProtocol);

            if (clientProtocol != Protocol.CurrentProtocol)
            {
                Log.Warning("Protocol mismatch from {EndPoint}: client={ClientProtocol}, server={ServerProtocol}", session.EndPoint, clientProtocol, Protocol.CurrentProtocol);
                session.Disconnect();
                return;
            }

            var response = new NetworkSettingsPacket
            {
                CompressionThreshold = 256,
                CompressionAlgorithm = CompressionAlgorithm.Zlib,
                ClientThrottling = false,
                ThrottlingThreshold = 0,
                ThrottlingScalar = 0f
            };

            session.SendPacket(response, false);
            session.CompressionReady = true;
            session.State = SessionState.LoginPhase;
            Log.Information("Sent NetworkSettings to {EndPoint}", session.EndPoint);
        }

        private void HandleResourcePackClientResponse(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new ResourcePackClientResponsePacket();
            packet.Decode(stream);

            Log.Information("Received ResourcePackClientResponse from {Username}: Status={Status}", session.Username, packet.ResponseStatus);

            switch (packet.ResponseStatus)
            {
                case ResourcePackClientResponseStatus.Refused:
                    Log.Warning("Player {Username} refused resource packs, disconnecting", session.Username);
                    session.Disconnect();
                    break;

                case ResourcePackClientResponseStatus.SendPacks:
                    Log.Information("Player {Username} requested pack download (no packs to send)", session.Username);
                    break;

                case ResourcePackClientResponseStatus.HaveAllPacks:
                    var stackPacket = new ResourcePackStackPacket
                    {
                        MustAccept = false,
                        BaseGameVersion = "*",
                        UseVanillaEditorPacks = false
                    };
                    session.SendPacket(stackPacket, false);
                    Log.Information("Sent ResourcePackStack to {Username}", session.Username);
                    break;

                case ResourcePackClientResponseStatus.Completed:
                    session.State = SessionState.PlayPhase;
                    Log.Information("Resource pack phase completed for {Username}", session.Username);

                    var voxelShapes = new VoxelShapesPacket
                    {
                        Shapes = new List<VoxelShape>(),
                        NameMap = new Dictionary<string, ushort>(),
                        CustomShapeCount = 0
                    };
                    session.SendPacket(voxelShapes);
                    Log.Information("Sent VoxelShapes to {Username}", session.Username);

                    var startGame = new StartGamePacket
                    {
                        // --- Header ---
                        EntityUniqueId = 609,
                        EntityRuntimeId = 402,
                        PlayerGamemode = 1, // Creative
                        X = 0, Y = 0, Z = 0,
                        Yaw = 0, Pitch = 0,

                        // --- Level Settings ---
                        Seed = 777777777777,
                        SpawnBiomeType = 0,
                        UserDefinedBiomeName = "plains",
                        Dimension = 0,
                        Generator = 1,
                        WorldGamemode = 1, // Creative
                        IsHardcore = false,
                        Difficulty = 0,
                        SpawnX = 100, SpawnY = 200, SpawnZ = 300,
                        HasAchievementsDisabled = true,
                        EditorWorldType = 0,
                        CreatedInEditor = false,
                        ExportedFromEditor = false,
                        DayCycleStopTime = 0,
                        EduEditionOffer = 0,
                        HasEduFeaturesEnabled = false,
                        EducationProductId = "",
                        RainLevel = 0.0f,
                        LightningLevel = 0.0f,
                        HasConfirmedPlatformLockedContent = false,
                        MultiplayerGame = true,
                        BroadcastToLan = true,
                        XblBroadcastIntent = 2, // FriendsOnly
                        PlatformBroadcastIntent = 2, // FriendsOnly
                        CommandsEnabled = true,
                        IsTexturePacksRequired = false,
                        ExperimentsPreviouslyToggled = false,
                        BonusChest = false,
                        HasStartWithMapEnabled = false,
                        PermissionLevel = 1,
                        ServerChunkTickRange = 4,
                        HasLockedBehaviorPack = false,
                        HasLockedResourcePack = false,
                        IsFromLockedWorldTemplate = false,
                        IsUsingMsaGamertagsOnly = false,
                        IsFromWorldTemplate = false,
                        IsWorldTemplateOptionLocked = false,
                        IsOnlySpawningV1Villagers = false,
                        IsDisablingPersonas = false,
                        IsDisablingCustomSkins = false,
                        MuteEmoteAnnouncements = false,
                        VanillaVersion = "*",
                        LimitedWorldWidth = 16,
                        LimitedWorldDepth = 16,
                        NewNether = false,
                        EduSharedResourceButtonName = "",
                        EduSharedResourceLinkUri = "",
                        ForceExperimentalGameplay = false,
                        ChatRestrictionLevel = 0,
                        DisablePlayerInteractions = false,

                        // --- Post-level-settings ---
                        LevelId = "",
                        WorldName = "World",
                        PremiumWorldTemplateId = "",
                        IsTrial = false,
                        RewindHistorySize = 0,
                        IsServerAuthoritativeBlockBreaking = false,
                        CurrentTick = 9000,
                        EnchantmentSeed = 99000,
                        MultiplayerCorrelationId = "c5d3d2cc-27fd-4221-9de6-d22c4d423d53",
                        IsInventoryServerAuthoritative = false,
                        ServerEngine = "*",
                        PlayerPropertyData = new CompoundTag(),
                        BlockRegistryChecksum = 0,
                        WorldTemplateId = Guid.Empty,

                        // Post-WorldTemplateId
                        ClientSideGenerationEnabled = false,
                        BlockNetworkIdsHashed = false,
                        IsSoundsServerAuthoritative = false,

                        // Telemetry strings
                        ServerId = "",
                        ScenarioId = "",
                        WorldId = "",
                        OwnerId = "",

                        // v924 ServerJoinInformation
                        HasServerJoinInformation = false,
                        ServerJoinInfo = null
                    };

                    session.SendPacket(startGame);
                    Log.Information("Sent StartGame to {Username}", session.Username);

                    var spawnStatus = new PlayStatusPacket
                    {
                        Status = PlayStatus.PlayerSpawn
                    };
                    session.SendPacket(spawnStatus);
                    Log.Information("Sent PlayStatus(PlayerSpawn) to {Username}", session.Username);
                    break;
            }
        }
    }
}
