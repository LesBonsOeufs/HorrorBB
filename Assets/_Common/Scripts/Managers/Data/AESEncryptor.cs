using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AESEncryptor
{
    /// <summary>
    /// Depends on used device & added id
    /// </summary>
    public static byte[] BuildKey(string addedId)
    {
        byte[] lResult = new byte[32];
        // Generate a unique key based on the device ID and addedId
        using var sha256 = SHA256.Create();
        byte[] lHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier + addedId));
        System.Array.Copy(lHash, lResult, 32);
        return lResult;
    }

    public static string Encrypt(string plainText, byte[] key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                byte[] iv = aesAlg.IV;
                byte[] encryptedContent = msEncrypt.ToArray();
                byte[] result = new byte[iv.Length + encryptedContent.Length];
                System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                return System.Convert.ToBase64String(result);
            }
        }
    }

    public static string Decrypt(string cipherText, byte[] key)
    {
        byte[] fullCipher = System.Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            System.Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            System.Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipher))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}