using System;
using System.Text.Json;

namespace QuantumMC.Network
{
    public class LoginChainData
    {
        public string Username { get; set; } = string.Empty;
        public Guid ClientUuid { get; set; }
        public string IdentityPublicKey { get; set; } = string.Empty;
        public long ClientId { get; set; }
        public string ServerAddress { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public int DeviceOs { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string GameVersion { get; set; } = string.Empty;
        public int GuiScale { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Xuid { get; set; } = string.Empty;
        public bool IsXboxAuthed { get; set; }
        public int CurrentInputMode { get; set; }
        public int DefaultInputMode { get; set; }
        public string CapeData { get; set; } = string.Empty;
        public int UiProfile { get; set; }
        public int MaxViewDistance { get; set; }
        public int MemoryTier { get; set; }
        public string PartyId { get; set; } = string.Empty;
        
        public JsonDocument? RawChainData { get; set; }
        public JsonDocument? RawClientData { get; set; }
    }
}
