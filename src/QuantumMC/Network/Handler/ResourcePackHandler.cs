using System;
using System.Collections.Generic;
using BedrockProtocol;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Types;
using BedrockProtocol.Utils;
using Nbt;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class ResourcePackHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new ResourcePackClientResponsePacket();
            packet.Decode(stream);

            switch (packet.ResponseStatus)
            {
                case ResourcePackClientResponseStatus.Refused:
                    session.Disconnect();
                    break;

                case ResourcePackClientResponseStatus.SendPacks:
                    Log.Debug("Player {Username} requested pack download (no packs to send)", session.Username);
                    break;

                case ResourcePackClientResponseStatus.HaveAllPacks:
                    var stackPacket = new ResourcePackStackPacket
                    {
                        MustAccept = false,
                        BaseGameVersion = "*",
                        UseVanillaEditorPacks = false
                    };
                    session.SendPacket(stackPacket);
                    break;

                case ResourcePackClientResponseStatus.Completed:
                    HandleCompleted(session);
                    break;
            }
        }

        private void HandleCompleted(PlayerSession session)
        {
            session.State = SessionState.PlayPhase;

            var voxelShapes = new VoxelShapesPacket
            {
                Shapes = new List<VoxelShape>(),
                NameMap = new Dictionary<string, ushort>(),
                CustomShapeCount = 0
            };
            session.SendPacket(voxelShapes);

            var startGame = new StartGamePacket
            {
                EntityUniqueId = session.Player.EntityUniqueId,
                EntityRuntimeId = session.Player.EntityRuntimeId,
                PlayerGamemode = session.Player.Gamemode,
                X = session.Player.X, Y = session.Player.Y, Z = session.Player.Z,
                Yaw = session.Player.Yaw, Pitch = session.Player.Pitch,
                Seed = -1,
                SpawnBiomeType = 0,
                UserDefinedBiomeName = "plains",
                Dimension = 0,
                Generator = 1,
                WorldGamemode = session.Player.Gamemode,
                IsHardcore = false,
                Difficulty = 0,
                SpawnX = session.Player.World!.SpawnX, SpawnY = session.Player.World.SpawnY, SpawnZ = session.Player.World.SpawnZ,
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
                XblBroadcastIntent = 2,
                PlatformBroadcastIntent = 2,
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
                LevelId = "",
                WorldName = session.Player.World!.Name,
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
                ClientSideGenerationEnabled = false,
                BlockNetworkIdsHashed = false,
                IsSoundsServerAuthoritative = false,
                ServerId = "",
                ScenarioId = "",
                WorldId = "",
                OwnerId = "",
                HasServerJoinInformation = false,
                ServerJoinInfo = null
            };

            session.SendPacket(startGame);
        }
    }
}
