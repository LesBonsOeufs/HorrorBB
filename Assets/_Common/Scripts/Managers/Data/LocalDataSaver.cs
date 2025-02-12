using System.IO;
using UnityEngine;

/// <typeparam name="T">Data type</typeparam>
public static class LocalDataSaver<T> where T : class, new()
{
	private const string SAVE_EXTENSION = ".save";
	private const bool DEBUG_ENABLED = true;

    private readonly static string saveName = "projecthorror" + typeof(T).Name;

    private static string saveFullPath;

    //256-bit key for AES encryption
    private static byte[] encryptionKey;

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
        encryptionKey = AESEncryptor.BuildKey(saveName);
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
                string lDecryptedJson = AESEncryptor.Decrypt(lEncryptedJson, encryptionKey);
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

    public static void UnloadData() => CurrentData = null;

    public static void SaveCurrentData()
    {
        if (CurrentData == null)
            throw new System.Exception("No save loaded!");

        string lJson = JsonUtility.ToJson(CurrentData);
        string lEncryptedJson = AESEncryptor.Encrypt(lJson, encryptionKey);
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
}