using Stupid.Utilities;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Stupid.stujam01 {
    public class SettingsManager : SingletonMonoBehaviour<SettingsManager> {
        [Serializable]
        public class SaveData {
            public string Disclaimer => "Hiya! Sorry to make you edit settings in this dinky text file here. I just ran out of time and couldn't do a proper menu... " +
                "Anyways, once you're finished editing, please restart your game to apply these settings. " +
                "Much love, Pelle (Stupid++) (✿◡‿◡)";
            public string KeysInstruction => "Change your keybinds by changing the numbers in this file to one of the ones listed on the url below.";
            public string KeyListURL => "https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Key.html";

            public InputManager.Settings InputSettings = new InputManager.Settings();
            public bool FullScreen = true;
        }
        private const string SAVE_NAME = "GameSettings";
        private const string FORMAT_SUFFIX = ".json";
        private const bool FORMAT_PRETTY = true;

        private string saveFolder => Application.isEditor ? Application.dataPath : Application.persistentDataPath;
        private string savePath => $"{saveFolder}/{SAVE_NAME}{FORMAT_SUFFIX}";

        private InputManager inputManager => InputManager.Instance;

        public void Save() {
            SaveData saveData = new SaveData();
            saveData.InputSettings = inputManager.InputSettings;

            string saveDataJson = JsonConvert.SerializeObject(saveData, FORMAT_PRETTY ? Formatting.Indented : Formatting.None);
            File.WriteAllText(savePath, saveDataJson);
        }

        public void Load() {
            SaveData saveData = new SaveData();
            bool saveDataExists = File.Exists(savePath);

            if (!saveDataExists) {
                Save();
            }

            try {
                saveData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(savePath));
            }
            catch {
                Save();
                return;
            }

            if(saveData == null) {
                Save();
                return;
            }

            inputManager.InputSettings = saveData.InputSettings;

            if (saveData.FullScreen) {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
            }
            else {
                Screen.fullScreen = false;
            }
        }

        public void OpenSaveFile() => Application.OpenURL(savePath);
    }
}