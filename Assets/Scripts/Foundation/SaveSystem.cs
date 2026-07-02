using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    [Serializable]
    public class TeacherSaveData
    {
        public string TeacherId;
        public string Name;
        public string Specialty;     // TraitType enum value, stored as string
        public string TrainingFocus; // StatType enum value, stored as string
        public int TrainingBuff;
    }

    [Serializable]
    public class SaveData
    {
        public List<TeacherSaveData> Teachers = new List<TeacherSaveData>();
        public int TotalRunsStarted = 0;
        public int TotalRunsCompleted = 0;
        public string SaveVersion = "1.0";
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string Subdirectory = "MagicTutor";
        private const string SaveFileName = "save.json";
        private const string BackupFileName = "save.bak";

        private string _saveFolderPath;
        private string _saveFilePath;
        private string _backupFilePath;

        private SaveData _cachedData;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[SaveSystem] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _saveFolderPath = Path.Combine(Application.persistentDataPath, Subdirectory);
            _saveFilePath = Path.Combine(_saveFolderPath, SaveFileName);
            _backupFilePath = Path.Combine(_saveFolderPath, BackupFileName);

            // Ensure subdirectory exists
            if (!Directory.Exists(_saveFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(_saveFolderPath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to create directory: {_saveFolderPath}. Exception: {e}");
                }
            }

            // Early load to cache data on Awake
            Load();
        }

        public void Save(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveSystem] Attempted to save null SaveData.");
                return;
            }

            _cachedData = data;

            try
            {
                // Synchronous backup copy before write
                if (File.Exists(_saveFilePath))
                {
                    File.Copy(_saveFilePath, _backupFilePath, overwrite: true);
                }

                // Synchronous JSON serialization
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Synchronous Save failed. Exception: {e}");
            }
        }

        public SaveData Load()
        {
            if (_cachedData != null)
            {
                return _cachedData;
            }

            if (!File.Exists(_saveFilePath))
            {
                Debug.Log("[SaveSystem] Save file not found. Initializing new SaveData.");
                _cachedData = new SaveData();
                return _cachedData;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                _cachedData = JsonUtility.FromJson<SaveData>(json);

                if (_cachedData == null)
                {
                    throw new Exception("Deserialized SaveData was null.");
                }

                // Check version
                if (_cachedData.SaveVersion != "1.0")
                {
                    Debug.LogWarning($"[SaveSystem] Save file version mismatch (File: {_cachedData.SaveVersion}, Expected: 1.0). Lenient loading will proceed.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load save file. Exception: {e}. Attempting backup load.");
                _cachedData = LoadFromBackup();
            }

            return _cachedData;
        }

        private SaveData LoadFromBackup()
        {
            if (!File.Exists(_backupFilePath))
            {
                Debug.LogWarning("[SaveSystem] Backup save file not found. Returning default SaveData.");
                return new SaveData();
            }

            try
            {
                string json = File.ReadAllText(_backupFilePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data != null)
                {
                    Debug.Log("[SaveSystem] Successfully recovered from backup file.");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load backup save file. Exception: {e}");
            }

            return new SaveData();
        }
    }
}
