using HarmonyLib;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace ModManager
{
    public class ModManager : CustomOptionsCategory
    {
        private ModInfo current;
        private int index = 0;
        private StandardMenuButton previousButton;
        private StandardMenuButton nextButton;
        public StandardMenuButton upatchButton;
        public TextMeshProUGUI modName;
        public TextMeshProUGUI modInfo;
        public List<ModInfo> mods = new List<ModInfo>();
        private static bool CheckForHotKey(KeyCode keyCode) =>
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(keyCode);
        public override void Build()
        {
            ModInfo.modManager = this;
            string[] files = new string[] { }.AddRangeToArray(Directory.GetFiles(ModInfo.PluginsPath, "*.disable", SearchOption.AllDirectories)).AddRangeToArray(Directory.GetFiles(ModInfo.PluginsPath, "*dll", SearchOption.AllDirectories));
            foreach (string name in files)
            {
                if (name.ToLower().EndsWith(ModInfo.dllEnabled) || name.ToLower().EndsWith(ModInfo.dllDisabled)) new ModInfo(name.Replace(ModInfo.PluginsPath, ""));
            }
            StandardMenuButton apply = CreateApplyButton(() => { 
                mods.Do(x => x.ChangeState()); 
                Application.Quit(); 
            });
            AddTooltip(apply, "Apply changes");
            upatchButton = CreateTextButton(() =>
            {
                if (current.PluginInfo != null && !current.IsException)
                {
                    Harmony.UnpatchID(current.PluginInfo.Metadata.GUID);
                }
            }, "Unpatch", "Unpatch", new Vector2(-20, -160), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one * 100, Color.black);
            AddTooltip(upatchButton, "Disables patches of mod, but not disables mod, applied patches won't disabled\n<color=red>VERY BUGGY, CAN BROKE GAME</color>");
            previousButton = BasePlugin.CreateButtonWithSprite("PreviousButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_2"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_0"), transform, new Vector3(-150, 30));
            previousButton.OnPress = new UnityEngine.Events.UnityEvent();
            previousButton.OnPress.AddListener(() => ChangeMod(false));
            previousButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            nextButton = BasePlugin.CreateButtonWithSprite("NextButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_3"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_1"), transform, new Vector3(150, 30));
            nextButton.OnPress = new UnityEngine.Events.UnityEvent();
            nextButton.OnPress.AddListener(() => ChangeMod(true));
            nextButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            modName = CreateText("ModName", "Wow! Empty page!", new Vector2(0, 30), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one, Color.black);
            modName.fontSize = 20;
            //new Vector2(-20, -100)
            modInfo = CreateText("ModInfo", "ModInfo", new Vector2(0, 30), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, Vector2.one, Color.black);
            modInfo.fontSize = 20;
            modInfo.gameObject.SetActive(false);
            mods.Do(x => x.SetActive(false));
            mods.Where(x => !ModInfo.GetPDBState(x.name) && ModInfo.PDBExists(x.name)).Do(x => BasePlugin.RenameFiles(x.pdbPath, Path.GetFileNameWithoutExtension(x.pdbPath) + ModInfo.pdbEnanled));
            mods.First().SetActive(true);
        }
        public static string InsertNewLineEveryNCharacters(string text, int n)
        {
            if (string.IsNullOrEmpty(text) || n <= 0)
                return text;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                if ((i + 1) % n == 0 && i != text.Length - 1)
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
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
                    res += mod.name + "/" + mod.Value.ToString() + "\n";
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
                    mods.Where(x => x.name == d[0] && !x.IsException).Do(x => x.Value = bool.Parse(d[1]));
                }
            }
            if (CheckForHotKey(KeyCode.M) && current.PluginInfo != null)
            {
                GUIUtility.systemCopyBuffer = "GUID: " + current.PluginInfo.Metadata.GUID + "\nVersion: " + current.PluginInfo.Metadata.Version + "\nName: " + current.PluginInfo.Metadata.Name;
            }
            if (CheckForHotKey(KeyCode.D)) mods.Where(x => !x.IsException).Do(x => x.Value = false);
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
                mods[i].SetActive(i == index);
            }
            current = mods[index];
            upatchButton.gameObject.SetActive(current.Value);
            if (current.IsException || current.PluginInfo == null) upatchButton.gameObject.SetActive(false);
        }
        public MenuToggle FixedCreateToggle(string name, string text, bool value, Vector3 position, float width, string toolTip)
        {
            MenuToggle toggle = CreateToggle(name, text, value, position, width);
            AddTooltip(toggle, toolTip);
            return toggle;
        }
    }
}
