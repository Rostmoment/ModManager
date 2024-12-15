using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ModManager
{
    class Constants
    {
        public const string MULTIPLE_MODS_FILE_FORMAT = ".rost";
        public const string ENABLED_DLL_FORMAT = ".dll";
        public const string DISABLED_DLL_FORMAT = ".disable";
        public const string TEMPORARY_DISABLED_DLL_FORMAT = ".tmpdsbdll";
        public const string ENABLED_PDB_FORMAT = ".pdb";
        public const string DISAVLED_PDB_FORMAT = ".pdbdisabled";
        public const string TEMPORARY_FILE_FORMAT = ".tmp";
        // Just constants simulation, I don't know why c# prohibits creating constants that are defined by methods and initialization
        public static readonly string MODDED_FOLDER_PATH = Path.Combine(Application.streamingAssetsPath, "Modded") + "\\";
        public static readonly string PLUGINS_PATH = Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 11, 11), "BepInEx", "plugins") + "\\";
        public static readonly string[] EXCEPTIONS_PLUGINS = new string[]
        {
            "mtm101.rulerp.bbplus.baldidevapi",
            "Newtonsoft.Json",
            "rost.moment.baldiplus.modmanager",
            "pixelguy.pixelmodding.baldiplus.pixelinternalapi",
            "ModManager",
            "discord_game_sdk",
            "PixelInternalAPI",
            "MTM101BaldAPI_BBCR",
            "MTM101BaldAPI",
            "MTM101BMDE",
            "MTM101BaldAPI_DEBUG"
        };
    }
}
