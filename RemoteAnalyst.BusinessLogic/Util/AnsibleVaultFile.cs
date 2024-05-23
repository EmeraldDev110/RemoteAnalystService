using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.Util
{
    class AnsibleVaultFile
    {
        const string AnsibleVaultSignature = "$ANSIBLE_VAULT";
        public string Version { get; set; }
        public string Algorithm { get; set; }
        public string Label { get; set; }
        public byte[] Salt { get; set; }
        public byte[] ExpectedHMac { get; set; }
        public byte[] EncryptedBytes { get; set; }
        public static AnsibleVaultFile Load(TextReader sr)
        {
            /* Sample ansible vault file
             * !vault |
                          $ANSIBLE_VAULT;1.1;AES256
                          65373538646137646131653930323838613532346433653366613266373333353934346230353866
                          3765386563356637636138383661636634343466393630350a313537633866373861626536656631
                          65323534336363336236656130356161656436363431646236383536333231616439656330313165
                          3162343865306538320a626239653532396639333134343065646336313437613134656234613736
                          63623738383037346561663938306564643061323133353132626432323432353233393063653363
             */
            var ret = new AnsibleVaultFile();
            // Skip "!vault |" line
            var headerString = sr.ReadLine();
            if (headerString.Contains("!vault"))
                headerString = sr.ReadLine();
            if (headerString == null) throw new ArgumentException("invalid ansible vault file");
            var header = headerString.Trim().Split(';');
            if (header[0] != AnsibleVaultSignature)
            {
                throw new ArgumentException($"invalid ansible vault header:{headerString}");
            }
            ret.Version = header[1];
            ret.Algorithm = header[2];
            if (header.Length > 3)
            {
                ret.Label = header[3];
            }
            var bodyBytes = new List<byte>();
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                ByteUtil.ConvertToBytes(line.Trim(), bodyBytes);
            }
            var (salt, expectedhmac, encrypted_bytes) = DecodeBody(bodyBytes.ToArray());
            ret.Salt = salt;
            ret.ExpectedHMac = expectedhmac;
            ret.EncryptedBytes = encrypted_bytes;
            return ret;
        }

        public static void Save(AnsibleVaultFile f, TextWriter output, string label, int width = 80)
        {
            if (string.IsNullOrEmpty(f.Label))
            {
                output.WriteLine($"{AnsibleVaultSignature};{f.Version};{f.Algorithm}");
            }
            else
            {
                output.WriteLine($"{AnsibleVaultSignature};{f.Version};{f.Algorithm};{f.Label}");
            }
            var data = new byte[f.Salt.Length * 2 + f.ExpectedHMac.Length * 2 + f.EncryptedBytes.Length * 2 + 2];
            var dataSpan = data.AsSpan();
            Encoding.ASCII.GetBytes(ByteUtil.ConvertToHexString(f.Salt)).AsSpan().CopyTo(dataSpan);
            dataSpan[f.Salt.Length * 2] = 0x0a;

            Encoding.ASCII.GetBytes(ByteUtil.ConvertToHexString(f.ExpectedHMac)).AsSpan().CopyTo(dataSpan.Slice(f.Salt.Length * 2 + 1));
            dataSpan[f.Salt.Length * 2 + 1 + f.ExpectedHMac.Length * 2] = 0x0a;

            Encoding.ASCII.GetBytes(ByteUtil.ConvertToHexString(f.EncryptedBytes)).AsSpan().CopyTo(dataSpan.Slice(f.Salt.Length * 2 + f.ExpectedHMac.Length * 2 + 2));
            Span<char> buffer = stackalloc char[width];
            while (!dataSpan.IsEmpty)
            {
                var len = Math.Min(40, dataSpan.Length);
                ByteUtil.ConvertToHexChars(dataSpan.Slice(0, len), buffer);
                output.WriteLine(buffer.Slice(0, len * 2).ToArray());
                dataSpan = dataSpan.Slice(len);
            }
        }

        static (byte[] salt, byte[] expectedhmac, byte[] encryptedbytes) DecodeBody(byte[] body)
        {
            byte[] salt = null, expectedhmac = null, encrypted_bytes = null;
            var sp = body.AsSpan();
            for (int i = 0; i < 3; i++)
            {
                var idx = sp.IndexOf((byte)0x0a);
                if (idx < 0)
                {
                    encrypted_bytes = ByteUtil.ConvertToBytes(Encoding.ASCII.GetString(sp));
                    break;
                }
                switch (i)
                {
                    case 0:
                        salt = ByteUtil.ConvertToBytes(Encoding.ASCII.GetString(sp.Slice(0, idx)));
                        break;
                    case 1:
                        expectedhmac = ByteUtil.ConvertToBytes(Encoding.ASCII.GetString(sp.Slice(0, idx)));
                        break;
                    default:
                        throw new ArgumentException("invalid body format");
                }
                sp = sp.Slice(idx + 1);
            }
            return (salt, expectedhmac, encrypted_bytes);
        }
    }
    public static class EncodingExtensions
    {
        public static string GetString(this Encoding encoding, Span<byte> source)
        {
            //naive way using ToArray, but possible to improve when needed
            return encoding.GetString(source.ToArray());
        }
    }
}
