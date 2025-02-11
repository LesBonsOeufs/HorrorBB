using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <typeparam name="T">Data type</typeparam>
public static class LocalDataSaver<T> where T : class, new()
{
	private const string SAVE_EXTENSION = ".save";
	private const bool DEBUG_ENABLED = true;

    private readonly static string saveName = "projecthorror" + typeof(T).Name;

    private static string saveFullPath;

    //256-bit key for AES encryption
    private static readonly byte[] encryptionKey = new byte[32];

    public static T CurrentData
    {
        get
        {
            if (_currentData == null)
                LoadData();

            return _currentData;
        }

        private set
        {
            _currentData = value;
        }
    }
    private static T _currentData;

    /// <summary>
    /// Must be called from the main thread
    /// </summary>
    public static void Init()
    {
        saveFullPath = Application.persistentDataPath + "/" + saveName + SAVE_EXTENSION;

        // Generate a unique key based on the device ID and save name
        using var sha256 = SHA256.Create();
        byte[] lHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier + saveName));
        System.Array.Copy(lHash, encryptionKey, 32);
    }

    /// <typeparam name="T">Data type</typeparam>
    private static void LoadData()
    {
        if (saveFullPath == null)
            Init();

        T lSave = new();

        if (CheckIfSaveExists())
        {
            try
            {
                string lEncryptedJson = File.ReadAllText(saveFullPath);
                string lDecryptedJson = Decrypt(lEncryptedJson);
                lSave = JsonUtility.FromJson<T>(lDecryptedJson);

                if (DEBUG_ENABLED)
                    Debug.Log("Data loaded successfully!");
            }
            catch (System.Exception e)
            {
                File.Delete(saveFullPath);

                if (DEBUG_ENABLED)
                    Debug.LogWarning($"Data was corrupted and has been deleted! Error: {e.Message}");
            }
        }
        else
        {
            if (DEBUG_ENABLED)
                Debug.Log("Requested data not found in local storage! Will use an empty one");
        }

        CurrentData = lSave;
    }

    public static void SaveCurrentData()
    {
        if (CurrentData == null)
            throw new System.Exception("No save loaded!");

        string lJson = JsonUtility.ToJson(CurrentData);
        string lEncryptedJson = Encrypt(lJson);
        File.WriteAllText(saveFullPath, lEncryptedJson);

        if (DEBUG_ENABLED)
            Debug.Log("Local Save Updated!");
    }

    public static bool CheckIfSaveExists()
	{
        if (saveFullPath == null)
            Init();

        return File.Exists(saveFullPath);
	}

	public static void ResetSave()
    {
		if (CheckIfSaveExists())
		{
			File.Delete(saveFullPath);
			LoadData();
		}
	}

    #region Encryption

    private static string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
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

    private static string Decrypt(string cipherText)
    {
        byte[] fullCipher = System.Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
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

    #endregion
}
