using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace ModManager
{
    public class ModManager : CustomOptionsCategory
    {
        public List<ModInfo> mods;
        public TextMeshProUGUI modInfo;
        public TextMeshProUGUI modName;
        private int index;
        private StandardMenuButton previousButton;
        private StandardMenuButton nextButton;
        private ModInfo current;
        private static bool CheckForHotKey(KeyCode keyCode) =>
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(keyCode);
        public override void Build()
        {
            ModInfo.modManager = this;
            mods = new List<ModInfo>();
            foreach (string file in BasePlugin.GetFilesFromPlugin(Constants.DISABLED_DLL_FORMAT, Constants.ENABLED_DLL_FORMAT))
            {
                mods.Add(new ModInfo(file));
            }
            // Mod info
            modInfo = CreateText("ModInfo", "If you see this text\nSwitch mod to next/previous", new Vector2(0, -30), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one, Color.black);
            modInfo.fontSize = 20;
            // Mod name
            modName = CreateText("ModName", "If you see this text\nSwitch mod to next/previous", new Vector2(0, 30), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one, Color.black);
            modName.fontSize = 20;
            // Buttons
            previousButton = BasePlugin.CreateButtonWithSprite("PreviousButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_2"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_0"), transform, new Vector3(-150, 30));
            previousButton.OnPress = new UnityEngine.Events.UnityEvent();
            previousButton.OnPress.AddListener(() => ChangeMod(false));
            previousButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            nextButton = BasePlugin.CreateButtonWithSprite("NextButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_3"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_1"), transform, new Vector3(150, 30));
            nextButton.OnPress = new UnityEngine.Events.UnityEvent();
            nextButton.OnPress.AddListener(() => ChangeMod(true));
            nextButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            AddTooltip(CreateApplyButton(() =>
            {
                mods.Do(x => x.ChangeState());
                Application.Quit();
            }), "Apply changes");
            // Setup
            mods.Do(x => x.SetActive(false));
            mods.First().SetActive(true);
            modInfo.gameObject.SetActive(true);
            /*
            AddTooltip(CreateTextButton(() => RostFile.LoadPlugins(FileController.OpenFile()), "Import", "Import", new Vector2(-130, -160), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one * 100, Color.black), 
                "Import mods from " + Constants.MULTIPLE_MODS_FILE_FORMAT + " file");
            AddTooltip(CreateTextButton(() => RostFile.SavePlugins(FileController.OpenFile()), "Export", "Export", new Vector2(0, -160), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one * 100, Color.black), 
                "Export all mods to " + Constants.MULTIPLE_MODS_FILE_FORMAT + " file");*/
            AddTooltip(CreateTextButton(() =>
            {
                Chainloader._loaded = false;
                Dictionary<string, string> files = new Dictionary<string, string>();
                foreach (PluginInfo plugin in Chainloader.PluginInfos.Values)
                {
                    files.Add(BasePlugin.RemoveFileExtension(plugin.Location) + Constants.TEMPORARY_DISABLED_DLL_FORMAT, Path.GetFileName(plugin.Location));
                    BasePlugin.RenameFiles(plugin.Location, Path.GetFileNameWithoutExtension(plugin.Location) + Constants.TEMPORARY_DISABLED_DLL_FORMAT);
                }
                Chainloader.Start();
                foreach (var data in files)
                {
                    BasePlugin.RenameFiles(data.Key, data.Value);
                }
            }, "Reload", "Reload", new Vector2(-130, -160), BaldiFonts.BoldComicSans24, TextAlignmentOptions.Center, Vector2.one*100, Color.black), "Load all .dll files from the plugins folder that haven't been loaded yet\n(helps enable mods without restarting the game)\n<color=red>Very broken thing, can broke game</color>");
        }
        void Update()
        {
            if (modName != null) modName.autoSizeTextContainer = true;
            if (modInfo != null)
            {
                modInfo.autoSizeTextContainer = false;
                modInfo.autoSizeTextContainer = true;
            }
            if (modName != null)
            {
                modName.autoSizeTextContainer = false;
                modName.autoSizeTextContainer = true;
            }
            if (CheckForHotKey(KeyCode.C))
            {
                string res = "";
                foreach (ModInfo mod in mods)
                {
                    res += mod.Name + "/" + mod.Value.ToString() + "\n";
                }
                res = res.Substring(0, res.Length - 1);
                GUIUtility.systemCopyBuffer = res;
            }
            if (CheckForHotKey(KeyCode.V))
            {
                string res = GUIUtility.systemCopyBuffer;
                foreach (string l in BasePlugin.Split(res, '\n'))
                {
                    string[] d = BasePlugin.Split(l, '/');
                    mods.Where(x => x.Name == d[0] && !x.IsException).Do(x => x.Value = bool.Parse(d[1]));
                }
            }
            if (CheckForHotKey(KeyCode.M) && current.PluginInfo != null)
            {
                if (current.PluginInfo != null)
                    GUIUtility.systemCopyBuffer = "GUID: " + current.PluginInfo.Metadata.GUID + "\nVersion: " + current.PluginInfo.Metadata.Version + "\nName: " + current.PluginInfo.Metadata.Name;
            }
            if (CheckForHotKey(KeyCode.D)) mods.Do(x => x.Value = false);
            if (CheckForHotKey(KeyCode.E)) mods.Do(x => x.Value = true);
            if (CheckForHotKey(KeyCode.S))
            {
                mods.Do(x => x.ChangeState());
                Application.Quit();
            }
        }

        private void ChangeMod(bool state)
        {
            if (state) index++;
            else index--;
            if (index < 0) index = mods.Count - 1;
            if (index >= mods.Count) index = 0;
            for (int i = 0; i < mods.Count; i++)
            {
                mods[i].SetActive(false);
            }
            current = mods[index];
            current.SetActive(true);
        }
    }
}
