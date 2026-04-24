using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace QuantumMC.Network
{
    public static class EncryptionUtils
    {
        public static (byte[] AesKey, byte[] IvBase) DeriveKeys(byte[] sharedSecret, byte[] salt)
        {
            byte[] secret = (byte[])sharedSecret.Clone();

            using var sha256 = SHA256.Create();
            byte[] toHash = new byte[salt.Length + secret.Length];
            Buffer.BlockCopy(salt, 0, toHash, 0, salt.Length);
            Buffer.BlockCopy(secret, 0, toHash, salt.Length, secret.Length);
            
            byte[] key = sha256.ComputeHash(toHash);
            byte[] iv = new byte[12];
            Buffer.BlockCopy(key, 0, iv, 0, 12);
            
            return (key, iv);
        }

        public static BedrockStreamCipher CreateCipher(bool forEncryption, byte[] key, byte[] iv)
        {
            byte[] iv16 = new byte[16];
            Buffer.BlockCopy(iv, 0, iv16, 0, 12);
            iv16[15] = 2;
            
            var cipher = new SicBlockCipher(new AesEngine());
            cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(key), iv16));
            
            return new BedrockStreamCipher(cipher);
        }

        public static byte[] CalculateChecksum(byte[] plaintext, ulong counter, byte[] key)
        {
            using var sha256 = SHA256.Create();
            
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (!BitConverter.IsLittleEndian) Array.Reverse(counterBytes);
            
            byte[] toHash = new byte[8 + plaintext.Length + key.Length];
            Buffer.BlockCopy(counterBytes, 0, toHash, 0, 8);
            Buffer.BlockCopy(plaintext, 0, toHash, 8, plaintext.Length);
            Buffer.BlockCopy(key, 0, toHash, 8 + plaintext.Length, key.Length);
            
            byte[] hash = sha256.ComputeHash(toHash);
            byte[] checksum = new byte[8];
            Buffer.BlockCopy(hash, 0, checksum, 0, 8);
            
            return checksum;
        }

        public static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static byte[] Base64UrlDecode(string input)
        {
            return Convert.FromBase64String(PadBase64(input.Replace('-', '+').Replace('_', '/')));
        }

        public static string PadBase64(string input)
        {
            int padding = 4 - (input.Length % 4);
            if (padding < 4) input += new string('=', padding);
            return input;
        }
    }

    public class BedrockStreamCipher
    {
        private readonly SicBlockCipher _cipher;
        private readonly byte[] _counterBlock = new byte[16];
        private readonly byte[] _keyStream = new byte[16];
        private int _keyStreamIdx = 0;

        public BedrockStreamCipher(SicBlockCipher cipher)
        {
            _cipher = cipher;
        }

        public void ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
        {
            for (int i = 0; i < len; i++)
            {
                if (_keyStreamIdx == 0)
                {
                    _cipher.ProcessBlock(_counterBlock, 0, _keyStream, 0);
                }

                output[outOff + i] = (byte)(input[inOff + i] ^ _keyStream[_keyStreamIdx]);

                _keyStreamIdx = (_keyStreamIdx + 1) % 16;
            }
        }
    }
}
