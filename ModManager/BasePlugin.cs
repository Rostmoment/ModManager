using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.OptionsAPI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModManager
{
    [BepInPlugin("rost.moment.baldiplus.modmanager", "Mods Manager", "2.1")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class BasePlugin : BaseUnityPlugin
    {
        public static BasePlugin Instance;
        public static string[] Split(string input, char delimiter) // For some reason string.split doesnt work
        {
            List<string> substrings = new List<string>();
            string current = "";
            foreach (char c in input)
            {
                if (c == delimiter)
                {
                    substrings.Add(current);
                    current = "";
                }
                else current += c;
            }
            if (current != "") substrings.Add(current);
            return substrings.ToArray(); 
        }
        private void OnMenu(OptionsMenu menu)
        {
            GameObject category = CustomOptionsCore.CreateNewCategory(menu, "Mods\nManager");
            ModManager.optionsMenu = menu;
            ModManager.category = category;
            ModManager modManager = category.AddComponent<ModManager>();
            modManager.transform.SetParent(category.transform, false);
        }
        private void Awake()
        {
            Harmony harmony = new Harmony("rost.moment.baldiplus.modmanager");
            harmony.PatchAllConditionals();
            if (Instance == null)
            {
                Instance = this;
            }
            CustomOptionsCore.OnMenuInitialize += OnMenu;
        }
        public static void UseCmd(string command)
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
        public static void RenameFiles(string fullPath, string newName) => UseCmd("rename \"" + fullPath + "\" \"" + Path.GetFileName(newName) + "\"");
        public static T LoadAsset<T>(string name) where T : Object
        {
            return (from x in Resources.FindObjectsOfTypeAll<T>()
                    where x.name.ToLower() == name.ToLower()
                    select x).First();
        }
        public static StandardMenuButton CreateButtonWithSprite(string name, Sprite sprite, Sprite spriteOnHightlight = null, Transform parent = null, Vector3? positon = null)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.layer = 5;
            gameObject.tag = "Button";
            StandardMenuButton res = gameObject.AddComponent<StandardMenuButton>();
            res.image = gameObject.AddComponent<Image>();
            res.image.sprite = sprite;
            res.unhighlightedSprite = sprite;
            res.OnPress = new UnityEvent();
            res.OnRelease = new UnityEvent();
            if (spriteOnHightlight != null)
            {
                res.OnHighlight = new UnityEvent();
                res.swapOnHigh = true;
                res.highlightedSprite = spriteOnHightlight;
            }
            res.transform.SetParent(parent);
            res.transform.localPosition = positon ?? new Vector3(0, 0, 0);
            return res;
        }
    }
}