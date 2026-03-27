using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mosambi.Editor
{
    public class SaveDebuggerWindow : EditorWindow
    {
        private string displayJson = "";
        private Vector2 scrollPos;

        [MenuItem("Mosambi/Save Data Debugger")]
        public static void ShowWindow()
        {
            GetWindow<SaveDebuggerWindow>("Save Debugger");
        }

        private void OnGUI()
        {
            GUILayout.Label("Mosambi Save Data Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // --- DATA VIEWER ---
            GUILayout.Label("Live Data Viewer", EditorStyles.label);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("View PlayerPrefs", GUILayout.Height(25)))
            {
                ViewPlayerPrefsData();
            }
            if (GUILayout.Button("View Secure File", GUILayout.Height(25)))
            {
                ViewSecureData();
            }
            GUILayout.EndHorizontal();

            // Display the JSON in a scrollable text area
            if (!string.IsNullOrEmpty(displayJson))
            {
                EditorGUILayout.Space(5);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.Height(150));
                EditorGUILayout.TextArea(displayJson, EditorStyles.textArea, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Format JSON (Pretty Print)", GUILayout.Height(25)))
                {
                    displayJson = FormatJson(displayJson);
                }
                if (GUILayout.Button("Clear Viewer", GUILayout.Height(25)))
                {
                    displayJson = "";
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(15);
            DrawHorizontalLine();
            EditorGUILayout.Space(15);

            // --- DATA WIPER ---
            GUILayout.Label("Data Wiper (Reset FTUE)", EditorStyles.label);

            if (GUILayout.Button("Delete PlayerPrefs Save", GUILayout.Height(30)))
            {
                if (PlayerPrefs.HasKey("Mosambi_SaveData_v1"))
                {
                    PlayerPrefs.DeleteKey("Mosambi_SaveData_v1");
                    PlayerPrefs.Save();
                    Debug.Log("<color=green>[Save Debugger]</color> PlayerPrefs save key deleted.");
                    displayJson = "";
                }
            }

            if (GUILayout.Button("Delete Secure File Save", GUILayout.Height(30)))
            {
                string securePath = Path.Combine(Application.persistentDataPath, "mosambi_secure.dat");
                if (File.Exists(securePath))
                {
                    File.Delete(securePath);
                    Debug.Log("<color=green>[Save Debugger]</color> Secure save file deleted.");
                    displayJson = "";
                }
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Open Hidden Save Folder in OS", GUILayout.Height(20)))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        private void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        #region Viewer Logic

        private void ViewPlayerPrefsData()
        {
            if (PlayerPrefs.HasKey("Mosambi_SaveData_v1"))
            {
                displayJson = PlayerPrefs.GetString("Mosambi_SaveData_v1");
            }
            else
            {
                displayJson = "No PlayerPrefs data found for 'Mosambi_SaveData_v1'.";
            }
        }

        private void ViewSecureData()
        {
            // 1. Find the Settings Asset in the Project
            string[] guids = AssetDatabase.FindAssets("t:MosambiSecuritySettings");
            if (guids.Length == 0)
            {
                displayJson = "ERROR: No MosambiSecuritySettings asset found.\nCreate one via 'Create > Mosambi > Settings > Security'.";
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<Core.MosambiSecuritySettings>(path);

            string securePath = Path.Combine(Application.persistentDataPath, settings.saveFileName);

            if (!File.Exists(securePath))
            {
                displayJson = "No secure file found at:\n" + securePath;
                return;
            }

            try
            {
                string encryptedJson = File.ReadAllText(securePath);
                displayJson = Decrypt(encryptedJson, settings.encryptionKey, settings.initializationVector);
            }
            catch (Exception e)
            {
                displayJson = $"DECRYPTION FAILED.\nError: {e.Message}";
            }
        }

        private string Decrypt(string cipherText, string keyString, string ivString)
        {
            // --- DEFENSIVE SIZING ---
            // Force the key to be exactly 32 chars and the IV to be exactly 16 chars.
            // If it's too short, it adds spaces. If it's too long, it chops off the end.
            keyString = keyString.PadRight(32).Substring(0, 32);
            ivString = ivString.PadRight(16).Substring(0, 16);

            byte[] iv = Encoding.UTF8.GetBytes(ivString);
            byte[] key = Encoding.UTF8.GetBytes(keyString);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion

        #region JSON Formatting Utility

        // A lightweight string parser to pretty-print raw JSON without needing Newtonsoft.Json
        private string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();

            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indent * 4));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', --indent * 4));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 4));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}