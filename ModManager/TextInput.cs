using MTM101BaldAPI.OptionsAPI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.XR.Interaction;
using static UnityEngine.Rendering.DebugUI;

namespace ModManager
{
    public class TextInput : MonoBehaviour
    {
        public TMP_Text tmp;

        private string value = "";

        public int MaxLen;

        private bool UseField = false;

        public bool CanUseLetters = true;
        
        public bool CanUseNumbers = true;

        public string Tip;

        public string Value => value;


        public void Initialize(TMP_Text tmp)
        {
            this.tmp = tmp;
            UpdateText();
        }

        public void SetActivity(bool activity)
        {
            UseField = activity;
        }

        private void Update()
        {
            if (!UseField) return;
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (value.Length > 0)
                {
                    value = value.Remove(value.Length - 1, 1);
                }
                UpdateText();
            }
            if (value.Length >= MaxLen) return;
            else if ((Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) && Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.RightControl))
            {
                value = "";
                UpdateText();
            }
            else if (Input.inputString.Length > 0 && !Input.GetKey(KeyCode.Backspace))
            {
                if ((char.IsLetter(Input.inputString[0]) && CanUseLetters) || (char.IsNumber(Input.inputString[0]) && CanUseNumbers))
                {
                    value += Input.inputString[0];
                    UpdateText();
                }
            }
            else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(KeyCode.V))
            {
                string systemCopyBuffer = GUIUtility.systemCopyBuffer;
                value = systemCopyBuffer;
                UpdateText();
            }
        }

        public void SetValue(string value)
        {
            this.value = value;
            UpdateText();
        }

        private void UpdateText()
        {
            tmp.text = Singleton<LocalizationManager>.Instance.GetLocalizedText(Tip) + value;
        }
    }

    public static class Creator
    {
        public static TextInput CreateTextInput(string Tip, Vector2 pos, bool CanUseLetters = true, bool CanUseNumbers = true, int maxLen = int.MaxValue)
        {
            GameObject text = new GameObject("textInput");
            text.layer = 5;
            text.tag = "Button";

            TextInput fieldInput = text.AddComponent<TextInput>();
            TextMeshProUGUI tmpText = text.AddComponent<TextMeshProUGUI>();
            tmpText.color = new Color(0, 0, 0, 1);
            tmpText.alignment = TextAlignmentOptions.Top;
            fieldInput.Tip = Tip;
            fieldInput.Initialize(tmpText);
            fieldInput.CanUseLetters = CanUseLetters;
            fieldInput.CanUseNumbers = CanUseNumbers;
            fieldInput.MaxLen = maxLen;
            text.transform.position = new Vector3(pos.x, pos.y, 0);
            return fieldInput;
        }
    }
}
