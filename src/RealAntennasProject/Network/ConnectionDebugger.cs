﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;


namespace RealAntennas.Network
{
    public class ConnectionDebugger : MonoBehaviour
    {
        const string GUIName = "Connection Debugger";
        public Dictionary<RealAntenna, List<LinkDetails>> items = new Dictionary<RealAntenna, List<LinkDetails>>();
        public Dictionary<RealAntenna, bool> visible = new Dictionary<RealAntenna, bool>();
        public RealAntenna antenna;
        private Rect Window = new Rect(120, 120, 900, 900);
        private Vector2 scrollPos;

        public void Start()
        {
            if (!(antenna is RealAntenna))
            {
                Debug.LogError("ConnectionDebugger started, but nothing requested for debug!");
                Destroy(this);
                gameObject.DestroyGameObject();
            }
            else
            {
                ScreenMessages.PostScreenMessage($"Debugging {antenna}", 2, ScreenMessageStyle.UPPER_CENTER, Color.yellow);
                ((RACommNetScenario.Instance as RACommNetScenario)?.Network?.CommNet as RACommNetwork).connectionDebugger = this;
            }
        }
        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;
            Window = GUILayout.Window(GetHashCode(), Window, GUIDisplay, GUIName, HighLogic.Skin.window);
        }
        private void GUIDisplay(int windowID)
        {
            Vessel parentVessel = (antenna?.ParentNode as RACommNode)?.ParentVessel;
            var style = new GUIStyle(HighLogic.Skin.box);

            GUILayout.BeginVertical(HighLogic.Skin.box);
            GUILayout.Label($"Vessel: {parentVessel?.name ?? "None"}");
            GUILayout.Label($"Antenna: {antenna.Name}");
            GUILayout.Label($"Band: {antenna.RFBand.name}       Power: {antenna.TxPower}dBm");
            if (antenna.CanTarget)
                GUILayout.Label($"Target: {antenna.Target}");
            GUILayout.EndVertical();
            GUILayout.Space(7);

            GUILayout.BeginVertical(HighLogic.Skin.box);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            foreach (var item in items)
            {
                if (!visible.ContainsKey(item.Key))
                    visible[item.Key] = false;
                bool mode = visible[item.Key];
                mode = GUILayout.Toggle(mode, $"{item.Key}", HighLogic.Skin.button, GUILayout.ExpandWidth(true), GUILayout.Height(20));
                visible[item.Key] = mode;
                if (mode)
                {
                    GUILayout.BeginVertical(HighLogic.Skin.box);
                    foreach (var data in item.Value)
                    {
                        // Display Tx and Rx relevant boxes side-by-side.
                        GUILayout.BeginHorizontal(HighLogic.Skin.box);

                        // Display Tx box
                        style.alignment = TextAnchor.UpperRight;
                        GUILayout.BeginVertical("Transmitter", style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        GUILayout.Label($"Antenna: {data.tx.Name}");
                        GUILayout.Label($"Power: {data.txPower}dBm");
                        GUILayout.Label($"Target: {data.tx.Target}");
                        GUILayout.Label($"Position: {data.txPos.x:F0}, {data.txPos.y:F0}, {data.txPos.z:F0}");
                        GUILayout.Label($"Beamwidth: {data.txBeamwidth:F2}");
                        GUILayout.Label($"Antenna AoA: {data.txToRxAngle:F1}");
                        GUILayout.EndVertical();

                        // Display Rx box
                        GUILayout.BeginVertical("Receiver", style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        GUILayout.Label($"Antenna: {data.rx.Name}");
                        GUILayout.Label($"Received Power: {data.rxPower}dBm");
                        GUILayout.Label($"Target: {data.rx.Target}");
                        GUILayout.Label($"Position: {data.rxPos.x:F0}, {data.rxPos.y:F0}, {data.rxPos.z:F0}");
                        GUILayout.Label($"Beamwidth: {data.rxBeamwidth:F2}");
                        GUILayout.Label($"Antenna AoA: {data.rxToTxAngle:F1}");
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                        // Display common stats
                        GUILayout.BeginHorizontal(HighLogic.Skin.box);
                        style.alignment = TextAnchor.UpperCenter;

                        GUILayout.BeginVertical("Noise", style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        GUILayout.Label($"Atmosphere Noise: {data.atmosphereNoise:F0}K");
                        GUILayout.Label($"Body Noise: {data.bodyNoise:F0}K");
                        GUILayout.Label($"Receiver Noise: {data.noiseTemp:F0}K");
                        GUILayout.Label($"N0: {data.N0:F2}dB/Hz");
                        GUILayout.Label($"Total Noise: {data.noise:F2}K");
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("Losses", style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        GUILayout.Label($"Distance: {math.length(data.txPos - data.rxPos):F0}");
                        GUILayout.Label($"Path Loss: {data.pathLoss:F1}dB");
                        GUILayout.Label($"Tx Pointing Loss: {data.txPointLoss:F1}dB");
                        GUILayout.Label($"Rx Pointing Loss: {data.rxPointLoss:F1}dB");
                        GUILayout.Label($"Pointing Loss: {data.pointingLoss:F1}dB");
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        GUILayout.BeginVertical("Link Budget", style);
                        GUILayout.Label("RxPower = TxGain + TxPower - Losses + RxGain");
                        GUILayout.Label($"{data.rxPower:F1} = {data.tx.Gain:F1} + {data.txPower:F1} - {(data.pathLoss + data.pointingLoss):F1} + {data.rx.Gain:F1}");
                        GUILayout.Label($"Encoder: {data.tx.Encoder.BestMatching(data.rx.Encoder)}");
                        GUILayout.Label($"Min Eb: {data.minEb:F2}");
                        GUILayout.Label($"Rates: {data.minDataRate}/{data.dataRate}/{data.maxDataRate}");
                        GUILayout.Label($"Steps: {data.rateSteps}");
                        GUILayout.EndVertical();
                        GUILayout.Space(12);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Space(9);
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(true)))
            {
                Destroy(this);
                gameObject.DestroyGameObject();
            }
            GUI.DragWindow();
        }
    }
}