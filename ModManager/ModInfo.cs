using BepInEx;
using BepInEx.Bootstrap;
using MTM101BaldAPI;
using MTM101BaldAPI.OptionsAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModManager
{
    public class ModInfo
    {
        /*
         -50
         -80
        */
        private static string[] exceptions = new string[7];
        private string fileName;
        private MenuToggle pdbButton = null;
        private MenuToggle mainButton;

        public bool IsException => exceptions.Contains(name);
        public bool Value
        {
            get
            {
                return mainButton.Value;
            }
            set
            {
                mainButton.Set(value);
            }
        }
        public string name;
        public static string pdbDisabled => ".pdbdisabled";
        public static string pdbEnanled => ".pdb";
        public static string dllEnabled => ".dll";
        public static string dllDisabled => ".disable";
        public static string PluginsPath => Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 11, 11), "BepInEx", "plugins") + "\\";
        public static ModManager modManager;
        public static OptionsMenu menu;
        public static GameObject category;
        public string pdbPath
        {
            get
            {
                string f = name + pdbEnanled;
                if (!File.Exists(PluginsPath + f)) f = name + pdbDisabled;
                if (File.Exists(PluginsPath + f))
                {
                    return PluginsPath + f;
                }
                return "";
            }
        }
        public PluginInfo PluginInfo
        {
            get
            {
                try
                {
                    return Chainloader.PluginInfos.Where(x => Path.GetFileNameWithoutExtension(x.Value.Location) == name).First().Value;
                }
                catch { return null; }
            }
        }
        public void ChangeState()
        {
            if (fileName != GetNewDLLFileName())
            {
                BasePlugin.RenameFiles(PluginsPath + fileName, GetNewDLLFileName());
            }
            if (pdbPath != "" && GetNewPDBName() != "")
            {
                if (Path.GetFileName(pdbPath) != GetNewPDBName())
                {
                    BasePlugin.RenameFiles(pdbPath, GetNewPDBName());
                }
            }
        }
        public void SetActive(bool value)
        {
            if (pdbButton != null) pdbButton.gameObject.SetActive(value);
            mainButton.gameObject.SetActive(value);
            if (value)
            {
                if (PluginInfo != null)
                {
                    modManager.modInfo.textBox.text = "GUID: " + PluginInfo.Metadata.GUID + "\nVersion: " + PluginInfo.Metadata.Version;
                }
                else modManager.modInfo.textBox.text = "";
                modManager.modName.textBox.text = "Mod name: " + name;
            }
        }
        public static bool GetPDBState(string name)
        {
            if (File.Exists(PluginsPath + name + pdbEnanled) && File.Exists(PluginsPath + name + pdbDisabled))
            {
                MTM101BaldiDevAPI.CauseCrash(BasePlugin.Instance.Info, new ArgumentException("Mod state error!\nDelete " + name + pdbDisabled + " or " + name + pdbDisabled + " file in plugins folder!"));
                return false;
            }
            if (File.Exists(PluginsPath + name + pdbEnanled)) return true;
            return false;
        }
        public static bool PDBExists(string name) => File.Exists(PluginsPath + name + pdbDisabled) || File.Exists(PluginsPath + name + pdbEnanled);
        public string GetNewPDBName()
        {
            if (pdbButton == null) return "";
            if (pdbButton.Value) return Path.GetFileNameWithoutExtension(pdbPath) + pdbEnanled;
            return Path.GetFileNameWithoutExtension(pdbPath) + pdbDisabled;
        }
        public string GetNewDLLFileName()
        {
            if (mainButton.Value) return name + dllEnabled;
            return name + dllDisabled;
        }
        public static bool GetDLLState(string name)
        {
            if (File.Exists(PluginsPath + name + dllDisabled) && File.Exists(PluginsPath + name + dllEnabled))
            {
                MTM101BaldiDevAPI.CauseCrash(BasePlugin.Instance.Info, new ArgumentException("Mod state error!\nDelete " + name + dllEnabled + " or " + name + dllDisabled + " file in plugins folder!"));
                return false;
            }
            if (File.Exists(PluginsPath + name + dllDisabled)) return false;
            if (File.Exists(PluginsPath + name + dllEnabled)) return true;
            throw new NullReferenceException("Mod " + name + " doesn't exist in plugins folder!");
        }
        static ModInfo()
        {
            exceptions[0] = "MTM101BaldAPI";
            exceptions[1] = "MTM101BaldAPI_BBCR";
            exceptions[2] = "MTM101BaldAPI_DEBUG";
            exceptions[3] = "Newtonsoft.Json";
            exceptions[4] = "ModManager";
            exceptions[5] = "MTM101BMDE";
            exceptions[6] = "PixelInternalAPI";
        }
        public ModInfo(string file) 
        {
            fileName = file;
            name = RemoveFileExtension(file);
            Vector2 pos = new Vector2(20, -50);
            /*if (PDBExists(name))
            {
                pdbButton = CustomOptionsCore.CreateToggleButton(menu, new Vector2(140, -50), ".pdb file", GetPDBState(name), "If .pdb file is active there will be more detail about error in logs\nLike line of  code, file path, etc");
                pos = new Vector2(-98, -50);
            }*/
            try
            {
                mainButton = CustomOptionsCore.CreateToggleButton(menu, pos, "Active", GetDLLState(name), "Is mod active");
            }
            catch (NullReferenceException) { return; }
            if (exceptions.Contains(name))
            {
                if (pdbButton != null) pdbButton.Disable(true);
                mainButton.Disable(true);
            }
            if (pdbButton != null) pdbButton.transform.SetParent(category.transform, false);
            mainButton.transform.SetParent(category.transform, false);
            modManager.mods.Add(this);
        }
        private static string RemoveFileExtension(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            return Path.Combine(directoryPath, fileNameWithoutExtension);
        }
    }
}
