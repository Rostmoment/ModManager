using HarmonyLib;
using MTM101BaldAPI.OptionsAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModManager
{
    public class ModManager : MonoBehaviour
    {
        private ModInfo current;
        private int index = 0;
        private StandardMenuButton previousButton;
        private StandardMenuButton nextButton;
        public StandardMenuButton upatchButton;
        public static OptionsMenu optionsMenu;
        public static GameObject category;
        public TextLocalizer modName;
        public TextLocalizer modInfo;
        public List<ModInfo> mods = new List<ModInfo>();
        private static bool CheckForHotKey(KeyCode keyCode) =>
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(keyCode);
        void Start()
        {
            ModInfo.menu = optionsMenu;
            ModInfo.category = category;
            ModInfo.modManager = this;
            string[] files = new string[] { }.AddRangeToArray(Directory.GetFiles(ModInfo.PluginsPath, "*.disable", SearchOption.AllDirectories)).AddRangeToArray(Directory.GetFiles(ModInfo.PluginsPath, "*dll", SearchOption.AllDirectories));
            foreach (string name in files)
            {
                if (name.ToLower().EndsWith(ModInfo.dllEnabled) || name.ToLower().EndsWith(ModInfo.dllDisabled)) new ModInfo(name.Replace(ModInfo.PluginsPath, ""));
            }
            CustomOptionsCore.CreateApplyButton(optionsMenu, "Apply changes", () => { mods.Do(x => x.ChangeState()); Application.Quit(); }).transform.SetParent(category.transform, false);
            upatchButton = CustomOptionsCore.CreateTextButton(optionsMenu, new Vector2(-20, -160), "Unpatch", "Disables patches of mod, but not disables mod, applied patches won't disabled\n<color=red>VERY BUGGY, CAN BROKE GAME</color>", () =>
            {
                if (current.PluginInfo != null && !current.IsException)
                {
                    Harmony.UnpatchID(current.PluginInfo.Metadata.GUID);
                }
            });
            upatchButton.transform.SetParent(category.transform, false);
            previousButton = BasePlugin.CreateButtonWithSprite("PreviousButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_2"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_0"), category.transform, new Vector3(-150, 30));
            previousButton.OnPress = new UnityEngine.Events.UnityEvent();
            previousButton.OnPress.AddListener(() => ChangeMod(false));
            previousButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            nextButton = BasePlugin.CreateButtonWithSprite("NextButton", BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_3"), BasePlugin.LoadAsset<Sprite>("MenuArrowSheet_1"), category.transform, new Vector3(150, 30));
            nextButton.OnPress = new UnityEngine.Events.UnityEvent();
            nextButton.OnPress.AddListener(() => ChangeMod(true));
            nextButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            modName = CustomOptionsCore.CreateText(optionsMenu, new Vector2(0, 30), "");
            modName.transform.SetParent(category.transform, false);
            modName.textBox.fontSize = 20;
            modInfo = CustomOptionsCore.CreateText(optionsMenu, new Vector2(-20, -100), "");
            modInfo.transform.SetParent(category.transform, false);
            modInfo.textBox.fontSize = 20;
            mods.Do(x => x.SetActive(false));
            mods.First().SetActive(true);
            mods.Where(x => !ModInfo.GetPDBState(x.name) && ModInfo.PDBExists(x.name)).Do(x => BasePlugin.RenameFiles(x.pdbPath, Path.GetFileNameWithoutExtension(x.pdbPath) + ModInfo.pdbEnanled));
        }
        void Update()
        {
            if (modName != null) modName.textBox.autoSizeTextContainer = true;
            if (modInfo != null)
            {
                modInfo.textBox.autoSizeTextContainer = false;
                modInfo.textBox.autoSizeTextContainer = true;
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
    }
}
