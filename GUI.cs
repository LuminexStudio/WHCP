using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace WHCP.Gui
{
    [BepInPlugin("com.whcp.gui", "WHCP", "4.6.0")]
    public class PropertyInitializer : BaseUnityPlugin
    {
        private bool showMenu = true;
        private Rect windowRect = new Rect(20, 20, 350, 520);

        private Dictionary<string, string> activeProperties = new Dictionary<string, string>();
        private List<string> blacklistedKeys = new List<string>();

        private string tempKey = "Property_Name";
        private string tempVal = "Value";

        void Update()
        {
            // Toggle menu with Q
            if (UnityInputSafe.GetKeyDown(KeyCode.Q)) showMenu = !showMenu;

            // Network Sync Loop (~5 seconds)
            if (PhotonNetwork.InRoom && Time.frameCount % 500 == 0)
            {
                ApplyNetworkLogic();
            }
        }

        void ApplyNetworkLogic()
        {
            if (PhotonNetwork.LocalPlayer == null) return;
            Hashtable updates = new Hashtable();

            // Force Always Public
            foreach (var prop in activeProperties) updates[prop.Key] = prop.Value;

            // Force Blacklist Deletion
            foreach (var key in blacklistedKeys) updates[key] = null;

            if (updates.Count > 0) PhotonNetwork.LocalPlayer.SetCustomProperties(updates);
        }

        void OnGUI()
        {
            if (!showMenu) return;
            GUI.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.98f);
            windowRect = GUI.Window(0, windowRect, DrawUI, "WHCP Property & Room Manager");
        }

        void DrawUI(int id)
        {
            GUILayout.BeginVertical();

            // --- ROOM CONTROLS ---
            GUILayout.Label($"<b> ROOM STATE: {PhotonNetwork.NetworkClientState} </b>");
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("JOIN RANDOM"))
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                }
                else if (PhotonNetwork.IsConnectedAndReady)
                {
                    PhotonNetwork.JoinRandomRoom();
                }
            }

            if (GUILayout.Button("DISCONNECT"))
            {
                PhotonNetwork.Disconnect();
            }
            GUILayout.EndHorizontal();

            // --- ADD SECTION ---
            GUILayout.Label("<b> ADD NEW PROPERTY </b>");
            tempKey = GUILayout.TextField(tempKey);
            tempVal = GUILayout.TextField(tempVal);
            if (GUILayout.Button("Add to Active Sync"))
            {
                if (!activeProperties.ContainsKey(tempKey)) activeProperties.Add(tempKey, tempVal);
                else activeProperties[tempKey] = tempVal;
            }

            GUILayout.Space(10);

            // --- LISTS ---
            GUILayout.Label("<b> CURRENT PROPERTIES </b>");
            foreach (var key in new List<string>(activeProperties.Keys))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{key}: {activeProperties[key]}", GUILayout.Width(180));
                if (GUILayout.Button("X", GUILayout.Width(30))) activeProperties.Remove(key);
                if (GUILayout.Button("BL", GUILayout.Width(35)))
                {
                    if (!blacklistedKeys.Contains(key)) blacklistedKeys.Add(key);
                    activeProperties.Remove(key);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.Label("<b> BLACKLIST </b>");
            foreach (var key in new List<string>(blacklistedKeys))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(key, GUILayout.Width(200));
                if (GUILayout.Button("Remove BL", GUILayout.Width(100))) blacklistedKeys.Remove(key);
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("FORCE NETWORK REFRESH")) ApplyNetworkLogic();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public static class UnityInputSafe
        {
            public static bool GetKeyDown(KeyCode k) { try { return Input.GetKeyDown(k); } catch { return false; } }
        }
    }
}