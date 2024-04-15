using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI.OptionsAPI;
using BepInEx.Bootstrap;
using MTM101BaldAPI.Reflection;
using System.Collections.Generic;
using static Rewired.InputMapper;
using TMPro;

namespace ModManager
{
	[BepInPlugin("rost.moment.baldiplus.modmanager", "Baldi Basics Plus Mods Manager", "1.0")]
	public class BasePlugin : BaseUnityPlugin
	{
		void Awake()
		{
			Harmony harmony = new Harmony("rost.moment.baldiplus.modmanager");
			harmony.PatchAll();
			CustomOptionsCore.OnMenuInitialize += OnMenu;
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
            modManager.ModsBar = CustomOptionsCore.CreateAdjustmentBar(menu, new Vector2(-90, 48), "ModsBar", Chainloader.PluginInfos.Count, "Change current mod", 0, () =>
            {
                modManager.GetPlugins();
                modManager.DestroyText();
                modManager.RebuildMenu();
            });
            modManager.transform.SetParent(category.transform, false);
        }
	}
	public class ModManagerMenu : MonoBehaviour
	{
		public GameObject CurrentCategory;
        public OptionsMenu OptionsCurrentMenu;
        public AdjustmentBars ModsBar;
        public TextLocalizer ModVersion;
        public TextLocalizer ModName;
        public TextLocalizer ModGUID;
        public List<string> Versions = new List<string>();
        public List<string> Names = new List<string>();
        public List<string> GUIDs = new List<string>();
        void Start()
		{
            GetPlugins();
            RebuildMenu();
		}

        public void GetPlugins()
		{
			foreach (PluginInfo plugin in Chainloader.PluginInfos.Values)
			{
				Names.Add(plugin.Metadata.Name);
                GUIDs.Add(plugin.Metadata.GUID);
				Versions.Add(plugin.Metadata.Version.ToString());
            }
		}
		public void DestroyText()
		{
			int ToDestroy = 0;
			if (ModsBar.GetRaw() >= 1)
			{
				ToDestroy = ModsBar.GetRaw() - 1;
            }
			Destroy(gameObject.transform.Find("ModGUID").gameObject);
		}
		public void RebuildMenu()
		{
            int current = 0;
            try
			{
				current = ModsBar.GetRaw();
			}
			catch
			{
			}
            ModGUID = CustomOptionsCore.CreateText(OptionsCurrentMenu, new Vector2(0, -10), string.Format("Mod GUID: {0}\nMod name: {1}\nMod version: {2}", GUIDs[current], Names[current], Versions[current]));
            ModGUID.name = "ModGUID";
            TMP_Text NameTextBox = BasePlugin.GetVariable<TMP_Text>(ModGUID, "textBox");
            NameTextBox.fontSize = 16;
			NameTextBox.autoSizeTextContainer = false;
			NameTextBox.autoSizeTextContainer = true;
            NameTextBox.autoSizeTextContainer = false;
            NameTextBox.autoSizeTextContainer = true;
            BasePlugin.SetValue(ModGUID, "textBox", NameTextBox);

            ModGUID.transform.SetParent(CurrentCategory.transform, false);
			ModsBar.transform.SetParent(CurrentCategory.transform, false);
		}
	}
}
