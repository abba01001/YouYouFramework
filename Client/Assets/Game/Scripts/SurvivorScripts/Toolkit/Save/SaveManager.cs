using System.Collections;
using UnityEngine;
using System.Threading;
using UnityEngine.Events;
using OctoberStudio.Easing;
using System.IO;
using Main;
using OctoberStudio.Abilities;
using OctoberStudio.Audio;
using OctoberStudio.Upgrades;
using OctoberStudio.Vibration;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace OctoberStudio.Save
{
    public static class SaveKey
    {
        public const string StageData = "stage";
        public const string GoldData = "gold";
        public const string CharactersData = "characters";
        public const string SaveFileName = "game_save";
        public const string AbilitiesData = "abilities_save";
        public const string UpgradesData = "upgrades_save";
        public const string InputData = "input";
    }
    
    public interface ISaveManager
    {
        T GetSave<T>(int hash) where T : ISave, new();
        T GetSave<T>(string uniqueName) where T : ISave, new();
        void Save(bool multithreading = false);

        
        StageSave StageData { get; }
        CharactersSave CharactersData { get; }
        AbilitiesSave AbilitiesData { get; }
        UpgradesSave UpgradesData { get; }
        InputSave InputData { get; }
    }

    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        #region 获取保存的数据
        private InputSave _inputSave;
        public InputSave InputData
        {
            get
            {
                _inputSave ??= GetSave<InputSave>(SaveKey.InputData);
                return _inputSave;
            }
        }
        
        private UpgradesSave _upgradesSave;
        public UpgradesSave UpgradesData
        {
            get
            {
                _upgradesSave ??= GetSave<UpgradesSave>(SaveKey.UpgradesData);
                return _upgradesSave;
            }
        }
        
        private AbilitiesSave _abilitiesSave;
        public AbilitiesSave AbilitiesData
        {
            get
            {
                _abilitiesSave ??= GetSave<AbilitiesSave>(SaveKey.AbilitiesData);
                return _abilitiesSave;
            }
        }
        
        private CharactersSave _charactersSave;
        public CharactersSave CharactersData
        {
            get
            {
                _charactersSave ??= GetSave<CharactersSave>(SaveKey.CharactersData);
                return _charactersSave;
            }
        }
  
        private StageSave _stageData;
        public StageSave StageData
        {
            get
            {
                _stageData ??= GetSave<StageSave>(SaveKey.StageData);
                return _stageData;
            }
        }
        
        #endregion
        
        private static SaveManager instance;

        [SerializeField] SaveType saveType = SaveType.SaveFile;

        [Space]
        [SerializeField] bool clearSave;

        [Space]
        [SerializeField] bool autoSaveEnabled;
        [SerializeField] float autoSaveDelay;

        private SaveDatabase SaveDatabase { get; set; }

        public bool IsSaveLoaded { get; private set; }

        public event UnityAction OnSaveLoaded;

        private Coroutine saveCoroutine;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            if (clearSave)
            {
                InitClear();
            }
            else
            {
                Load();
            }

            if (autoSaveEnabled)
            {
                StartCoroutine(AutoSaveCoroutine());
            }

            GameController.RegisterSaveManager(this);
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(int hash) where T : ISave, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return SaveDatabase.GetSave<T>(hash);
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(string uniqueName) where T : ISave, new()
        {
            return GetSave<T>(uniqueName.GetHashCode());
        }

        private void InitClear()
        {
            SaveDatabase = new SaveDatabase();
            SaveDatabase.Init();

            Debug.Log("New save is created");

            IsSaveLoaded = true;
        }

        private void Load()
        {
            if (IsSaveLoaded)
                return;

            if(saveType == SaveType.SaveFile)
            {
                // Try to read and deserialize file or create new one
                SaveDatabase = LoadSave();

                SaveDatabase.Init();

                Debug.Log("Save file is loaded");
            } else
            {
                var json = PlayerPrefs.GetString(SaveKey.SaveFileName);
                SaveDatabase = JsonUtility.FromJson<SaveDatabase>(json);
                if (SaveDatabase == null) SaveDatabase = new SaveDatabase();

                SaveDatabase.Init();

                Debug.Log("Loaded SaveDatabase from PlayerPrefs");
            }

            IsSaveLoaded = true;

            OnSaveLoaded?.Invoke();
        }

        private SaveDatabase LoadSave()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonObject = load(SaveKey.SaveFileName);
            if(!string.IsNullOrEmpty(jsonObject))
            {
                try
                {
                    SaveDatabase deserializedObject = JsonUtility.FromJson<SaveDatabase>(jsonObject);

                    return deserializedObject;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            return new SaveDatabase();
#else
            return SerializationHelper.DeserializePersistent<SaveDatabase>(SaveKey.SaveFileName, useLogs: false);
#endif
        }

        private IEnumerator SaveCoroutine(bool multithreading = false)
        {
            var wait = new WaitForSeconds(0.2f);
            while (SerializationHelper.IsFileLocked(SaveKey.SaveFileName))
            {
                yield return wait;
            }
            if (multithreading)
            {
                var saveThread = new Thread(() => {
                    SerializationHelper.SerializePersistent(SaveDatabase, SaveKey.SaveFileName);
                });
                saveThread.Start();
            }
            else
            {
                SerializationHelper.SerializePersistent(SaveDatabase, SaveKey.SaveFileName);
            }

            Debug.Log("Save file is updated");

            saveCoroutine = null;
        }

        private void ForceSave()
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();
            Debugger.LogError("保存文件===>",JsonUtility.ToJson(SaveDatabase));
            if(saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString(SaveKey.SaveFileName, JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            } else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SaveKey.SaveFileName);
                Debug.Log("Save file is updated");
#else
                if (!SerializationHelper.IsFileLocked(SaveKey.SaveFileName))
                {
                    SerializationHelper.SerializePersistent(SaveDatabase, SaveKey.SaveFileName);

                    Debug.Log("Save file is updated");
                }
#endif
            }
        }

        /// <summary>
        /// Saves the current state of the game to the file system
        /// </summary>
        /// <param name="multithreading"> if true, saves the file in another thread. Do not use multitherading in OnDestroy</param>
        public void Save(bool multithreading = false)
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

            if (saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString(SaveKey.SaveFileName, JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            } else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SaveKey.SaveFileName);
                Debug.Log("Save file is updated");
#else
                if (saveCoroutine == null) saveCoroutine = StartCoroutine(SaveCoroutine(multithreading));
#endif
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void WebGLSave(SaveDatabase saveDatabase, string fileName)
        {
            string jsonObject = JsonUtility.ToJson(saveDatabase);

            save(fileName, jsonObject);
        }
#endif

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSecondsRealtime(autoSaveDelay);

            while (true)
            {
                yield return wait;

                Save(true);
            }
        }

        public static void DeleteSaveFile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            deleteItem(SaveKey.SaveFileName);
#else
            SerializationHelper.DeletePersistent(SaveKey.SaveFileName);
#endif

            PlayerPrefs.DeleteAll();

            Debug.Log("Save file is deleted!");
        }

        private void OnDestroy()
        {
            ForceSave();
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ForceSave();
#endif
        }

        /// <summary>
        /// Android和IOS手机将应用程序最小化，而不是破坏它们
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus) ForceSave();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string load(string keyName);

        [DllImport("__Internal")]
        private static extern void save(string keyName, string data);

        [DllImport("__Internal")]
        private static extern void deleteItem(string keyName);
#endif
    }

    public enum SaveType
    {
        SaveFile,
        PlayerPrefs,
    }
}