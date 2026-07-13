using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Crypto;

namespace SharpCompress.Common.Rar;

[SuppressMessage(
    "Security",
    "CA5350:Do Not Use Weak Cryptographic Algorithms",
    Justification = "RAR3 key derivation is SHA-1 based by format definition."
)]
internal class CryptKey3 : ICryptKey
{
    const int AES_128 = 128;

    private readonly string _password;

    public CryptKey3(string? password) => _password = password ?? string.Empty;

    public ICryptoTransform Transformer(byte[] salt)
    {
        var aesIV = new byte[EncryptionConstV5.SIZE_INITV];

        var rawLength = 2 * _password.Length;
        var rawPassword = new byte[rawLength + EncryptionConstV5.SIZE_SALT30];
        var passwordBytes = Encoding.UTF8.GetBytes(_password);
        for (var i = 0; i < _password.Length; i++)
        {
            rawPassword[i * 2] = passwordBytes[i];
            rawPassword[(i * 2) + 1] = 0;
        }

        for (var i = 0; i < salt.Length; i++)
        {
            rawPassword[i + rawLength] = salt[i];
        }

        const int noOfRounds = 1 << 18;
        const int checkpointInterval = noOfRounds / EncryptionConstV5.SIZE_INITV;

        var sha = CopyableSha1.Create();
        Span<byte> counter3 = stackalloc byte[3];
        Span<byte> digest = stackalloc byte[20];

        for (var i = 0; i < noOfRounds; i++)
        {
            sha.Append(rawPassword);
            counter3[0] = (byte)i;
            counter3[1] = (byte)(i >> 8);
            counter3[2] = (byte)(i >> 16);
            sha.Append(counter3);

            // Checkpoint after appending round i (matches prior mega-array prefix hash).
            if (i % checkpointInterval == 0)
            {
                var checkpoint = sha;
                checkpoint.FinalizeTo(digest);
                aesIV[i / checkpointInterval] = digest[19];
            }
        }

        sha.FinalizeTo(digest);

        var aesKey = new byte[EncryptionConstV5.SIZE_INITV];
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                aesKey[(i * 4) + j] = (byte)(
                    (
                        ((digest[i * 4] * 0x1000000) & 0xff000000)
                        | (uint)((digest[(i * 4) + 1] * 0x10000) & 0xff0000)
                        | (uint)((digest[(i * 4) + 2] * 0x100) & 0xff00)
                        | (uint)(digest[(i * 4) + 3] & 0xff)
                    ) >> (j * 8)
                );
            }
        }

        var aes = Aes.Create();
        aes.KeySize = AES_128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        aes.Key = aesKey;
        aes.IV = aesIV;
        return aes.CreateDecryptor();
    }
}
