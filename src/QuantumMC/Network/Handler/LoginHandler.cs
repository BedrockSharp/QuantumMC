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
            
            _ = stream.ReadBytes(4); 
            
            uint reqLen = stream.ReadUnsignedVarInt();
            byte[] requestBytes = stream.ReadBytes((int)reqLen);
            
            string authInfoStr = "";
            if (requestBytes.Length > 0)
            {
                if (requestBytes[0] == '{')
                {
                    authInfoStr = System.Text.Encoding.UTF8.GetString(requestBytes);
                }
                else if (requestBytes.Length > 4 && requestBytes[4] == '{')
                {
                    int strLen = BitConverter.ToInt32(requestBytes, 0);
                    if (strLen > 0 && strLen <= requestBytes.Length - 4)
                        authInfoStr = System.Text.Encoding.UTF8.GetString(requestBytes, 4, strLen);
                }
                else
                {
                    var innerStream = new BinaryStream(requestBytes);
                    uint innerLen = innerStream.ReadUnsignedVarInt();
                    if (innerLen > 0 && innerLen <= requestBytes.Length)
                    {
                        byte[] strBytes = innerStream.ReadBytes((int)innerLen);
                        authInfoStr = System.Text.Encoding.UTF8.GetString(strBytes);
                    }
                }
            }
            
            stream.Position = 0;
            try { packet.Decode(stream); } catch {}

            Log.Information("Received Login from {EndPoint} (Protocol: {Protocol})", session.EndPoint, packet.ProtocolVersion);

            string clientPubKeyBase64 = string.Empty;
            string username = "Unknown";
            
            try
            {
                authInfoStr = SanitizeJsonString(authInfoStr);
                var authDoc = JsonDocument.Parse(authInfoStr);
                
                if (authDoc.RootElement.TryGetProperty("Token", out var tokenProp))
                {
                    string token = tokenProp.GetString() ?? "";
                    var tokenParts = token.Split('.');
                    if (tokenParts.Length == 3)
                    {
                        string payloadBase64 = tokenParts[1];
                        int padding = 4 - (payloadBase64.Length % 4);
                        if (padding < 4) payloadBase64 += new string('=', padding);
                        payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');
                        
                        var payloadDoc = JsonDocument.Parse(Convert.FromBase64String(payloadBase64));
                        
                        if (payloadDoc.RootElement.TryGetProperty("xname", out var xnameProp))
                            username = xnameProp.GetString() ?? "Unknown";
                        
                        if (payloadDoc.RootElement.TryGetProperty("cpk", out var cpkProp))
                            clientPubKeyBase64 = cpkProp.GetString() ?? "";
                        
                        Log.Information("Parsed OpenID login for {Username}", username);
                    }
                }
                else if (authDoc.RootElement.TryGetProperty("Certificate", out var certProp))
                {
                    string certStr = certProp.GetString() ?? "";
                    username = ExtractUsernameFromChain(certStr);
                    clientPubKeyBase64 = ExtractClientPublicKey(certStr);
                }
                else
                {
                    username = ExtractUsernameFromChain(authInfoStr);
                    clientPubKeyBase64 = ExtractClientPublicKey(authInfoStr);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to parse AuthInfo/ChainData: {Msg}", ex.Message);
            }
            
            session.Username = username;
            Log.Information("Player {Username} is logging in from {EndPoint}", session.Username, session.EndPoint);

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
                
                Log.Information("Sent ServerToClientHandshakePacket and enabled encryption for {Username}", session.Username);
                
                session.State = SessionState.LoginPhase; 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize encryption for {Username}", session.Username);
            }
        }

        private string ExtractUsernameFromChain(string chainDataJwt)
        {
            try
            {
                chainDataJwt = SanitizeJsonString(chainDataJwt);
                using var doc = JsonDocument.Parse(chainDataJwt);
                if (doc.RootElement.TryGetProperty("chain", out var chainArray) && chainArray.GetArrayLength() > 0)
                {
                    foreach (var jwtElem in chainArray.EnumerateArray())
                    {
                        var jwt = jwtElem.GetString();
                        if (string.IsNullOrEmpty(jwt)) continue;

                        var parts = jwt.Split('.');
                        if (parts.Length < 2) continue;

                        string payloadBase64 = parts[1];
                        int padding = 4 - (payloadBase64.Length % 4);
                        if (padding < 4) payloadBase64 += new string('=', padding);
                        payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');

                        byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
                        var payloadDoc = JsonDocument.Parse(payloadBytes);

                        if (payloadDoc.RootElement.TryGetProperty("extraData", out var extraData) &&
                            extraData.TryGetProperty("displayName", out var displayName))
                        {
                            return displayName.GetString() ?? "Unknown";
                        }
                    }
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        private string ExtractClientPublicKey(string chainDataJwt)
        {
            try
            {
                chainDataJwt = SanitizeJsonString(chainDataJwt);
                using var doc = JsonDocument.Parse(chainDataJwt);
                if (doc.RootElement.TryGetProperty("chain", out var chainArray) && chainArray.GetArrayLength() > 0)
                {
                    for (int i = chainArray.GetArrayLength() - 1; i >= 0; i--)
                    {
                        var jwt = chainArray[i].GetString();
                        if (string.IsNullOrEmpty(jwt)) continue;

                        var parts = jwt.Split('.');
                        if (parts.Length < 2) continue;

                        string headerBase64 = parts[0];
                        int padding = 4 - (headerBase64.Length % 4);
                        if (padding < 4) headerBase64 += new string('=', padding);
                        headerBase64 = headerBase64.Replace('-', '+').Replace('_', '/');
                        
                        try
                        {
                            byte[] headerBytes = Convert.FromBase64String(headerBase64);
                            var headerDoc = JsonDocument.Parse(headerBytes);
                            if (headerDoc.RootElement.TryGetProperty("x5u", out var x5u))
                            {
                                string? val = x5u.GetString();
                                if (!string.IsNullOrEmpty(val)) return val;
                            }
                        }
                        catch {}

                        string payloadBase64 = parts[1];
                        padding = 4 - (payloadBase64.Length % 4);
                        if (padding < 4) payloadBase64 += new string('=', padding);
                        payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');

                        try
                        {
                            byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
                            var payloadDoc = JsonDocument.Parse(payloadBytes);
                            if (payloadDoc.RootElement.TryGetProperty("identityPublicKey", out var pubKey))
                                return pubKey.GetString() ?? "";
                            
                            if (payloadDoc.RootElement.TryGetProperty("extraData", out var extraData) &&
                                extraData.TryGetProperty("identityPublicKey", out pubKey))
                                return pubKey.GetString() ?? "";
                        }
                        catch {}
                    }
                }
            }
            catch (Exception ex) { Log.Warning("Failed to extract client public key: {Message}", ex.Message); }
            return string.Empty;
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
