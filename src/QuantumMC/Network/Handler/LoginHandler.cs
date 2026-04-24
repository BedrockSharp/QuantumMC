using System.Security.Cryptography;
using System.Text.Json;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class LoginHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new LoginPacket();
            
            string authInfoStr = System.Text.Encoding.UTF8.GetString(payload);
            
            stream.Position = 0;
            try { packet.Decode(stream); } catch {}

            LoginChainData chainData = ParseChainData(authInfoStr);
            session.Player.ChainData = chainData;
            
            session.Username = chainData.Username;
            session.Player.Username = chainData.Username;
            session.Player.Xuid = chainData.Xuid;
            session.Player.Uuid = chainData.ClientUuid.ToString();

            string clientPubKeyBase64 = chainData.IdentityPublicKey;

            if (Server.Instance.PlayerProvider.LoadPlayer(session.Player))
            {
                Log.Debug("Loaded persistent data for {Username} (XUID: {Xuid})", chainData.Username, chainData.Xuid);
            }

            Log.Information("Creating player: {Username} ({EndPoint})", session.Username, session.EndPoint);

            try
            {
                using var serverEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP384);
                string serverPublicKeyBase64 = Convert.ToBase64String(serverEcdh.PublicKey.ExportSubjectPublicKeyInfo());

                if (string.IsNullOrEmpty(clientPubKeyBase64))
                {
                    Log.Error("Could not extract client public key! Server cannot establish encryption.");
                    return;
                }
                
                byte[] clientPubKeyBytes = Convert.FromBase64String(clientPubKeyBase64);
                using var clientKey = ECDiffieHellman.Create();
                clientKey.ImportSubjectPublicKeyInfo(clientPubKeyBytes, out _);

                byte[] sharedSecret = serverEcdh.DeriveRawSecretAgreement(clientKey.PublicKey);
                byte[] serverSalt = new byte[16];
                RandomNumberGenerator.Fill(serverSalt);

                var (aesKey, ivBase) = EncryptionUtils.DeriveKeys(sharedSecret, serverSalt);
                
                string headerJson = $"{{\"alg\":\"ES384\",\"x5u\":\"{serverPublicKeyBase64}\"}}";
                string payloadJson = $"{{\"salt\":\"{Convert.ToBase64String(serverSalt)}\"}}";

                string headerB64 = EncryptionUtils.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(headerJson));
                string payloadB64 = EncryptionUtils.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(payloadJson));
                
                string unsignedToken = $"{headerB64}.{payloadB64}";
                using var ecdsa = ECDsa.Create(serverEcdh.ExportParameters(true));
                byte[] signature = ecdsa.SignData(System.Text.Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA384);
                string signatureB64 = EncryptionUtils.Base64UrlEncode(signature);

                var handshakePacket = new ServerToClientHandshakePacket
                {
                    JwtToken = $"{unsignedToken}.{signatureB64}"
                };
                
                session.SendPacket(handshakePacket);
                session.InitializeEncryption(aesKey, ivBase);
                
                session.State = SessionState.LoginPhase; 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize encryption for {Username}", session.Username);
            }
        }

        private LoginChainData ParseChainData(string authInfoStr)
        {
            var data = new LoginChainData();
            if (string.IsNullOrEmpty(authInfoStr)) return data;

            try
            {
                if (authInfoStr.StartsWith("ey") && authInfoStr.Contains('.'))
                {
                    var parts = authInfoStr.Split('.');
                    if (parts.Length >= 2)
                    {
                        try
                        {
                            var payloadDoc = JsonDocument.Parse(EncryptionUtils.Base64UrlDecode(parts[1]));
                            var payload = payloadDoc.RootElement;
                            
                            if (payload.TryGetProperty("cpk", out var cpkProp))
                                data.IdentityPublicKey = cpkProp.GetString() ?? "";
                            if (payload.TryGetProperty("xname", out var nameProp))
                                data.Username = nameProp.GetString() ?? "Unknown";
                            if (payload.TryGetProperty("xid", out var xidProp))
                                data.Xuid = xidProp.GetString() ?? "";
                            
                            return data;
                        }
                        catch {}
                    }
                }

                authInfoStr = SanitizeJsonString(authInfoStr);
                using var doc = JsonDocument.Parse(authInfoStr);
                var root = doc.RootElement;
                data.RawChainData = JsonDocument.Parse(authInfoStr);

                if (root.TryGetProperty("Token", out var wrapperToken))
                {
                    return ParseChainData(wrapperToken.GetString() ?? "");
                }
                if (root.TryGetProperty("Certificate", out var wrapperCert))
                {
                    return ParseChainData(wrapperCert.GetString() ?? "");
                }

                if (root.TryGetProperty("chain", out var chainArray))
                {
                    foreach (var jwtElem in chainArray.EnumerateArray())
                    {
                        var jwt = jwtElem.GetString();
                        if (string.IsNullOrEmpty(jwt)) continue;
                        var parts = jwt.Split('.');
                        if (parts.Length < 2) continue;

                        try
                        {
                            var payloadDoc = JsonDocument.Parse(EncryptionUtils.Base64UrlDecode(parts[1]));
                            var payload = payloadDoc.RootElement;

                            if (payload.TryGetProperty("identityPublicKey", out var pubKeyProp))
                                data.IdentityPublicKey = pubKeyProp.GetString() ?? data.IdentityPublicKey;

                            if (payload.TryGetProperty("extraData", out var extraData))
                            {
                                if (extraData.TryGetProperty("displayName", out var nameProp))
                                    data.Username = nameProp.GetString() ?? data.Username;
                                if (extraData.TryGetProperty("XUID", out var xuidProp))
                                    data.Xuid = xuidProp.GetString() ?? data.Xuid;
                                if (extraData.TryGetProperty("identity", out var uuidProp) && Guid.TryParse(uuidProp.GetString(), out var uuid))
                                    data.ClientUuid = uuid;
                            }
                        }
                        catch (Exception ex) { Log.Verbose("Failed to parse chain JWT part: {Msg}", ex.Message); }

                        try
                        {
                            var headerDoc = JsonDocument.Parse(EncryptionUtils.Base64UrlDecode(parts[0]));
                            if (headerDoc.RootElement.TryGetProperty("x5u", out var x5u))
                                data.IdentityPublicKey = x5u.GetString() ?? data.IdentityPublicKey;
                        }
                        catch {}
                    }
                }
                else
                {
                    Log.Warning("No 'chain' property found in login data!");
                }

                string clientDataJwt = string.Empty;
                if (root.TryGetProperty("clientData", out var clientDataProp)) clientDataJwt = clientDataProp.GetString() ?? "";
                else if (root.TryGetProperty("ClientData", out clientDataProp)) clientDataJwt = clientDataProp.GetString() ?? "";

                if (!string.IsNullOrEmpty(clientDataJwt))
                {
                    var parts = clientDataJwt.Split('.');
                    if (parts.Length >= 2)
                    {
                        try
                        {
                            var payloadDoc = JsonDocument.Parse(EncryptionUtils.Base64UrlDecode(parts[1]));
                            data.RawClientData = payloadDoc;
                            var payload = payloadDoc.RootElement;

                            if (payload.TryGetProperty("ClientUUID", out var uuidProp) && Guid.TryParse(uuidProp.GetString(), out var uuid))
                                data.ClientUuid = uuid;
                            
                            if (payload.TryGetProperty("DeviceModel", out var modelProp))
                                data.DeviceModel = modelProp.GetString() ?? "";
                            if (payload.TryGetProperty("DeviceOS", out var osProp))
                                data.DeviceOs = osProp.GetInt32();
                            if (payload.TryGetProperty("DeviceId", out var idProp))
                                data.DeviceId = idProp.GetString() ?? "";
                            if (payload.TryGetProperty("GameVersion", out var verProp))
                                data.GameVersion = verProp.GetString() ?? "";
                            if (payload.TryGetProperty("LanguageCode", out var langProp))
                                data.LanguageCode = langProp.GetString() ?? "";
                            if (payload.TryGetProperty("GuiScale", out var guiProp))
                                data.GuiScale = guiProp.GetInt32();
                            if (payload.TryGetProperty("UIProfile", out var uiProp))
                                data.UiProfile = uiProp.GetInt32();
                            
                            if (payload.TryGetProperty("ClientId", out var idLongProp))
                                data.ClientId = idLongProp.GetInt32() == 0 ? 0 : idLongProp.GetInt64();
                            if (payload.TryGetProperty("ServerAddress", out var addrProp))
                                data.ServerAddress = addrProp.GetString() ?? "";
                            if (payload.TryGetProperty("CurrentInputMode", out var inputProp))
                                data.CurrentInputMode = inputProp.GetInt32();
                            if (payload.TryGetProperty("DefaultInputMode", out var defInputProp))
                                data.DefaultInputMode = defInputProp.GetInt32();
                            if (payload.TryGetProperty("MemoryTier", out var memProp))
                                data.MemoryTier = memProp.GetInt32();
                            if (payload.TryGetProperty("PartyId", out var partyProp))
                                data.PartyId = partyProp.GetString() ?? "";
                            if (payload.TryGetProperty("CapeData", out var capeProp))
                                data.CapeData = capeProp.GetString() ?? "";
                        }
                        catch (Exception ex) { Log.Warning("Failed to parse client data JWT: {Msg}", ex.Message); }
                    }
                }
                else
                {
                    Log.Warning("No 'clientData' property found in login data!");
                }
                
                data.IsXboxAuthed = !string.IsNullOrEmpty(data.Xuid);
            }
            catch (Exception ex) { Log.Warning("Failed to parse login chain data: {Message}. Data: {Data}", ex.Message, authInfoStr); }
            
            return data;
        }

        private static string SanitizeJsonString(string json)
        {
            int start = json.IndexOf('{');
            int end = json.LastIndexOf('}');
            if (start != -1 && end != -1 && end >= start)
                return json.Substring(start, end - start + 1);
            return json;
        }
    }
}
