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
        public static ModManager modManager;
        public PluginInfo PluginInfo
        {
            get
            {
                try
                {
                    return Chainloader.PluginInfos.Where(x => BasePlugin.RemoveFileExtension(x.Value.Location.Substring(Constants.PLUGINS_PATH.Length)) == Name).First().Value;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }
        public string OriginalPath { get; }
        public string FilePath { get; }
        public bool IsException
        {
            get
            {
                if (PluginInfo != null) return Constants.EXCEPTIONS_PLUGINS.Contains(PluginInfo.Metadata.GUID) || Constants.EXCEPTIONS_PLUGINS.Contains(Name) || Constants.EXCEPTIONS_PLUGINS.Contains(Path.GetFileNameWithoutExtension(Name));
                return Constants.EXCEPTIONS_PLUGINS.Contains(Name) || Constants.EXCEPTIONS_PLUGINS.Contains(Path.GetFileNameWithoutExtension(Name));
            }
        }
        public bool Value
        {
            get
            {
                if (toggle == null) return IsException;
                if (IsException) return true;
                return toggle.Value;
            }
            set
            {
                Set(value);
            }
        }
        public string Name { get; }
        public string PDBFilePath { get; }
        private MenuToggle toggle;
        public ModInfo(string path)
        {
            OriginalPath = path;
            FilePath = path;
            PDBFilePath = null;
            Name = BasePlugin.RemoveFileExtension(FilePath).Substring(Constants.PLUGINS_PATH.Length);
            if (File.Exists(BasePlugin.RemoveFileExtension(FilePath) + Constants.ENABLED_PDB_FORMAT))
                PDBFilePath = BasePlugin.RemoveFileExtension(FilePath) + Constants.ENABLED_PDB_FORMAT;
            toggle = null;
            if (!IsException)
                modManager.AddTooltip(toggle = modManager.CreateToggle("ModActive", "Active", FilePath.ToLower().EndsWith(Constants.ENABLED_DLL_FORMAT), new Vector2(20, -100), 100), "Is mod active");
        }
        public void Set(bool enable)
        {
            if (toggle == null) return;
            toggle.Set(enable);
            if (IsException) toggle.Set(false);
        }
        public void ChangeState()
        {
            if (toggle == null) return;
            if (!Value)
            {
                if (FilePath.EndsWith(Constants.ENABLED_DLL_FORMAT))
                    BasePlugin.RenameFiles(FilePath, Path.GetFileNameWithoutExtension(FilePath)+Constants.DISABLED_DLL_FORMAT);
            }
            else
            {
                if (FilePath.EndsWith(Constants.DISABLED_DLL_FORMAT))
                    BasePlugin.RenameFiles(FilePath, Path.GetFileNameWithoutExtension(FilePath) + Constants.ENABLED_DLL_FORMAT);
            }
        }
        public void SetActive(bool active)
        {
            if (toggle != null)
            {
                toggle.gameObject.SetActive(active);
                if (IsException)
                    toggle.gameObject.SetActive(false);
            }
            if (active)
            {
                modManager.modName.text = BasePlugin.InsertNewLineEveryNCharacters(Name, 25);
                if (PluginInfo != null)
                {
                    modManager.modInfo.gameObject.SetActive(true);
                    modManager.modInfo.text = "Name: " + PluginInfo.Metadata.Name + "\nGUID: " + PluginInfo.Metadata.GUID + "\nVersion: " + PluginInfo.Metadata.Version; 
                }
                else
                {
                    modManager.modInfo.gameObject.SetActive(false);
                }
            }
        }
    }
}
