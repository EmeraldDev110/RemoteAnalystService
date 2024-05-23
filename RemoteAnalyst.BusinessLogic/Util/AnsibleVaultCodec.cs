using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

namespace RemoteAnalyst.BusinessLogic.Util
{
    public class AnsibleVaultCodec
    {
        public string Decode(TextReader vaultFileReader, byte[] password)
        {
            var vaultFile = AnsibleVaultFile.Load(vaultFileReader);
            var cipher = CipherUtilities.GetCipher("AES/CTR/PKCS7Padding");
            var pbkdf2 = new Rfc2898DeriveBytes(password, vaultFile.Salt, 10000, HashAlgorithmName.SHA256);
            var derived = pbkdf2.GetBytes(32 + 32 + 16);
            var cipherKeyBytes = derived.AsSpan(0, 32).ToArray();
            var hmacKey = derived.AsSpan(32, 32).ToArray();
            var iv = derived.AsSpan(64, 16).ToArray();
            var hmac256 = new HMACSHA256(hmacKey);
            var actualhmac = hmac256.ComputeHash(vaultFile.EncryptedBytes);
            if (!actualhmac.AsSpan().SequenceEqual(vaultFile.ExpectedHMac))
            {
                throw new InvalidKeyException("HMAC check error: invalid password");
            }
            var cparam = new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", cipherKeyBytes), iv);
            cipher.Init(false, cparam);
            var decrypted = cipher.DoFinal(vaultFile.EncryptedBytes);
            string xml = Encoding.ASCII.GetString(decrypted.ToArray());
            return xml;
        }

        public void Encode(byte[] data, byte[] password, byte[] salt, TextWriter output, string label, int width)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var derived = pbkdf2.GetBytes(32 + 32 + 16);
            var vaultFile = new AnsibleVaultFile();
            vaultFile.Version = "1.1";
            vaultFile.Algorithm = "AES256";
            vaultFile.Salt = salt;
            var cipher = CipherUtilities.GetCipher("AES/CTR/PKCS7Padding");
            var cipherKeyBytes = derived.AsSpan(0, 32).ToArray();
            var hmacKey = derived.AsSpan(32, 32).ToArray();
            var iv = derived.AsSpan(64, 16).ToArray();
            var cparam = new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", cipherKeyBytes), iv);
            cipher.Init(true, cparam);
            var encrypted = cipher.DoFinal(data);
            var hmac256 = new HMACSHA256(hmacKey);
            vaultFile.EncryptedBytes = encrypted;
            vaultFile.ExpectedHMac = hmac256.ComputeHash(encrypted);
            AnsibleVaultFile.Save(vaultFile, output, label, width);
        }
    }
}
