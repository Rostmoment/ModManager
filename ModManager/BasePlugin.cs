using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.AssetTools;
using BepInEx.Bootstrap;
using MTM101BaldAPI.SaveSystem;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Diagnostics;
using System.Configuration;
using System.Data;
using JetBrains.Annotations;
using MTM101BaldAPI;

namespace ModManager
{
	[BepInPlugin("rost.moment.baldiplus.modmanager", "Baldi Basics Plus Mods Manager", "1.0")]
	public class BasePlugin : BaseUnityPlugin
	{
		public static BasePlugin Instance = null;
        public string PluginsPath = null;
        public string GetSavePath(string name)
        {
            string text = ModdedSaveSystem.GetSaveFolder(BasePlugin.Instance, "Plugins.txt");
            text = text.Replace("\\Plugins.txt\\", "\\");
            text = text.Replace("rost.moment.baldiplus.modmanager", name + "\\" + "rost.moment.baldiplus.modmanager");
            return text;
        }
        void Awake()
		{
			Harmony harmony = new Harmony("rost.moment.baldiplus.modmanager");
			CustomOptionsCore.OnMenuInitialize += OnMenu;
            ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string path) => { });
            if (Instance == null)
            {
                Instance = this;
            }
            if (PluginsPath == null)
            {
                PluginsPath = Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 11, 11), "BepInEx", "plugins");
            }
        }

        public static T GetVariable<T>(object instance, string fieldName)
        {
            return Traverse.Create(instance).Field(fieldName).GetValue<T>();
        }
        public static void SetValue(object instance, string fieldName, object setVal)
        {
            Traverse.Create(instance).Field(fieldName).SetValue(setVal);
        }
        
		private void OnMenu(OptionsMenu menu)
		{
			GameObject category = CustomOptionsCore.CreateNewCategory(menu, "Mods\nManager");
			ModManagerMenu modManager = category.AddComponent<ModManagerMenu>();
			modManager.OptionsCurrentMenu = menu;
			modManager.CurrentCategory = category;
            modManager.GetPlugins();
            modManager.transform.SetParent(category.transform, false);
        }
    }
    public class ModStateError : Exception
    {
        public ModStateError()
        {
        }

        public ModStateError(string message)
            : base(message)
        {
        }

        public ModStateError(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ModManagerMenu : MonoBehaviour
	{
		public GameObject CurrentCategory;
        public GameObject TrashCategory = null;
        public OptionsMenu OptionsCurrentMenu;
        public TextLocalizer ModGUID = null;
        public StandardMenuButton ApplyButton = null;
        public StandardMenuButton NextButton = null;
        public StandardMenuButton PreviousButton = null;
        public StandardMenuButton LoadFromFileButton = null;
        public StandardMenuButton SaveToFileButton = null;
        public TextInput FileNameInput = null;
        public readonly Dictionary<string, string> FilesData = new Dictionary<string, string>();
        public readonly Dictionary<string, MenuToggle> ButtonsList = new Dictionary<string, MenuToggle>();
        public List<string> Versions = new List<string>();
        public List<string> Names = new List<string>();
        public List<string> GUIDs = new List<string>();
        public List<string> FileNames = new List<string>();
        public List<string> Exceptions = new List<string>() { "MTM101BaldAPI.dll", "MTM101BaldAPI_BBCR.dll", "MTM101BaldAPI_DEBUG.dll", "Newtonsoft.Json.dll", "ModManager.dll", "MTM101BMDE.dll", "PixelInternalAPI.dll" };
        public string CurrentMod = "Nothing";
        public int current = 0;

        void Start()
		{
            TrashCategory = CustomOptionsCore.CreateNewCategory(OptionsCurrentMenu, "IDK");
            TrashCategory.transform.SetParent(null, true);
            GetPlugins();
            RebuildMenu();
		}
        public void GetPlugins()
		{
            foreach (string name in Directory.GetFiles(BasePlugin.Instance.PluginsPath))
            {
                bool isMode = name.EndsWith(".dll") || name.EndsWith(".disable");
                if (!FileNames.Contains(name) && isMode)
                {
                    bool state = false;
                    if (name.EndsWith(".dll"))
                    {
                        state = true;
                    }
                    try
                    {
                        ButtonsList.Add(Path.GetFileName(name), CustomOptionsCore.CreateToggleButton(OptionsCurrentMenu, new Vector2(20, -50), "Active", state, "Is the mod enabled"));
                        FileNames.Add(name);
                        FilesData.Add(Path.GetFileNameWithoutExtension(name), Path.GetExtension(name));
                    }
                    catch 
                    {
                        MTM101BaldiDevAPI.CauseCrash(BasePlugin.Instance.Info, new ModStateError("Mod " + Path.GetFileNameWithoutExtension(name) + " is both enabled and disabled at the same time. Delete the file " + Path.GetFileNameWithoutExtension(name) + ".dll or " + Path.GetFileNameWithoutExtension(name) + ".disable to fix this error")); 
                    }
                    
                }
            }
		}
        public void LoadFromFile()
        {
            string path = BasePlugin.Instance.GetSavePath(Singleton<PlayerFileManager>.Instance.fileName) + "\\" + FileNameInput.Value + ".txt";
            if (!File.Exists(path))
            {
                TMP_Text NameTextBox = BasePlugin.GetVariable<TMP_Text>(ModGUID, "textBox");
                NameTextBox.fontSize = 16;
                NameTextBox.color = Color.red;
                NameTextBox.autoSizeTextContainer = false;
                NameTextBox.autoSizeTextContainer = true;
                NameTextBox.autoSizeTextContainer = false;
                NameTextBox.autoSizeTextContainer = true;
                NameTextBox.text = "File not found!";
                BasePlugin.SetValue(ModGUID, "textBox", NameTextBox);
                return;
            }
            string fileData = File.ReadAllText(path);
            string fullKey = "";
            string[] data = new string[] { };
            foreach (var modData in fileData.Split('\n'))
            {
                try
                {
                    data = modData.Split('/');
                    fullKey = data[0] + FilesData[data[0]];
                    ButtonsList[fullKey].Set(bool.Parse(data[1]));
                }
                catch
                {

                }
            }
        }
        public void SaveToFile()
        {
            string fileData = "";
            foreach (var data in ButtonsList)
            {
                fileData += Path.GetFileNameWithoutExtension(data.Key) + "/" + data.Value.Value.ToString() + "\n";
            }
            string path = BasePlugin.Instance.GetSavePath(Singleton<PlayerFileManager>.Instance.fileName) + "\\" + FileNameInput.Value + ".txt";
            File.WriteAllText(path, fileData);
        }
        public void ChangeCurrentIndex(bool state)
        {
            if (state) current++;   
            else current--;
            if (current < 0) current = FileNames.Count-1;
            if (current >= FileNames.Count) current = 0;
        }
        public void ApplyChange()
        {
            foreach (var data in ButtonsList)
            {
                string pathToMod = BasePlugin.Instance.PluginsPath + "\\" + data.Key;
                string command = "rename \"" + pathToMod + "\" \"";
                string newName = Path.GetFileNameWithoutExtension(pathToMod);
                if (!data.Value.Value)
                {
                    newName += ".disable";
                }
                else
                {
                    newName += ".dll";
                }
                command += newName + "\"";
                if (Path.GetFileName(pathToMod) != newName)
                {
                    UseCmd(command);
                }
            }
        }
        private void UseCmd(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            Process process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                UnityEngine.Debug.Log("Output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                UnityEngine.Debug.Log("Error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            UnityEngine.Debug.Log($"ExitCode: {process.ExitCode}");
            process.Close();
        }
		public void RebuildMenu()
		{
            if (FileNameInput == null)
            {
                FileNameInput = Creator.CreateTextInput("Use keyboard to input file name\n", new Vector2(0, -85), CanUseNumbers: false, maxLen: 20);
                FileNameInput.SetActivity(true);
            }
            if (NextButton == null)
            {
                NextButton = CustomOptionsCore.CreateTextButton(OptionsCurrentMenu, new Vector2(100, 30), "Next", "Switch to the next mod", () =>
                {
                    ChangeCurrentIndex(true);
                    RebuildMenu();
                });
            }
            if (PreviousButton == null)
            {
                PreviousButton = CustomOptionsCore.CreateTextButton(OptionsCurrentMenu, new Vector2(-100, 30), "Previous", "Switch to the previous mod", () =>
                {
                    ChangeCurrentIndex(false);
                    RebuildMenu();
                });
            }
            if (LoadFromFileButton == null)
            {
                LoadFromFileButton = CustomOptionsCore.CreateTextButton(OptionsCurrentMenu, new Vector2(-20, -160), "Load", "Loads mod status from a file", () =>
                {
                    LoadFromFile();
                });
            }
            if (SaveToFileButton == null)
            {
                SaveToFileButton = CustomOptionsCore.CreateTextButton(OptionsCurrentMenu, new Vector2(-170, -160), "Save", "Saves the status of mods to a file", () =>
                {
                    SaveToFile();
                });
            }
            if (ApplyButton == null)
            {
                ApplyButton = CustomOptionsCore.CreateApplyButton(OptionsCurrentMenu, "Apply changes", () =>
                {
                    ApplyChange();
                    Application.Quit();
                });
            }
            CurrentMod = FileNames[current];
            string text = "";
            text += "Mod name: " + Path.GetFileNameWithoutExtension(CurrentMod);
            if (ModGUID == null)
            {
                ModGUID = CustomOptionsCore.CreateText(OptionsCurrentMenu, new Vector2(0, -10), text);
            }
            ModGUID.name = "ModGUID";
            TMP_Text NameTextBox = BasePlugin.GetVariable<TMP_Text>(ModGUID, "textBox");
            NameTextBox.fontSize = 16;
			NameTextBox.autoSizeTextContainer = false;
			NameTextBox.autoSizeTextContainer = true;
            NameTextBox.autoSizeTextContainer = false;
            NameTextBox.autoSizeTextContainer = true;
            FileNameInput.tmp.autoSizeTextContainer = false;
            FileNameInput.tmp.autoSizeTextContainer = true;
            FileNameInput.tmp.autoSizeTextContainer = false;
            FileNameInput.tmp.autoSizeTextContainer = true;
            NameTextBox.text = text;
            NameTextBox.color = Color.black;
            BasePlugin.SetValue(ModGUID, "textBox", NameTextBox);
            ApplyButton.transform.SetParent(CurrentCategory.transform, false);
            foreach (var data in ButtonsList)
            {
                if (data.Key == Path.GetFileName(CurrentMod))
                {
                    data.Value.transform.SetParent(CurrentCategory.transform, false);
                    if (Exceptions.Contains(data.Key))
                    {
                        data.Value.Disable(true);
                    }
                }
                else
                {
                    data.Value.transform.SetParent(TrashCategory.transform, false);
                }
            }
            FileNameInput.transform.SetParent(CurrentCategory.transform, false);
            PreviousButton.transform.SetParent(CurrentCategory.transform, false);
            NextButton.transform.SetParent(CurrentCategory.transform, false);
            LoadFromFileButton.transform.SetParent(CurrentCategory.transform, false);
            SaveToFileButton.transform.SetParent(CurrentCategory.transform, false);
            ModGUID.transform.SetParent(CurrentCategory.transform, false);
		}
	}
}
