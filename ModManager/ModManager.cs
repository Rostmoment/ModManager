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
        private int index = 0;
        private StandardMenuButton previousButton;
        private StandardMenuButton nextButton;
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
            foreach (string name in Directory.GetFiles(ModInfo.PluginsPath))
            {
                if (name.ToLower().EndsWith(ModInfo.dllEnabled) || name.ToLower().EndsWith(ModInfo.dllDisabled)) new ModInfo(Path.GetFileName(name));
            }
            CustomOptionsCore.CreateApplyButton(optionsMenu, "Apply changes", () => { mods.Do(x => x.ChangeState()); Application.Quit(); }).transform.SetParent(category.transform, false);
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
        }
    }
}
