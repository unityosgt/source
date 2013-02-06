using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;


/// <summary>
/// This MonoBehaviour is a basic GUI for the Photon client's statistics features.
/// The shown health values can help identify problems with connection losses or performance.
/// Example: 
/// If the time delta between two consecutive SendOutgoingCommands calls is a second or more,
/// chances rise for a disconnect being caused by this (because acknowledgements to the server
/// need to be sent in due time).
/// </summary>
public class PhotonStatsGui : MonoBehaviour
{
    public Rect statsRect = new Rect(0, 0, 200, 50);
    public float WidthWithText = 400;
    public bool statsWindowOn;
    public bool statsOn;
    public bool healthStatsOn;
    public bool trafficStatsOn;
    public bool buttonsOn;

    void Start()
    {
        float width = statsRect.width;
        if (this.trafficStatsOn)
        {
            width = WidthWithText;
        }

        statsRect = new Rect(Screen.width - width, 0, width, statsRect.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            this.statsWindowOn = !this.statsWindowOn;
            this.statsOn = true;    // enable stats when showing the window
        }
    }

    void OnGUI()
    {
        if (PhotonNetwork.networkingPeer.TrafficStatsEnabled != statsOn)
        {
            PhotonNetwork.networkingPeer.TrafficStatsEnabled = statsOn;
        }

        if (!statsWindowOn)
        {
            return;
        }

        statsRect = GUILayout.Window(0, statsRect, this.TrafficStatsWindow, "Messages (shift+tab)");
    }

    void TrafficStatsWindow(int windowID)
    {
        bool statsToLog = false;
        TrafficStatsGameLevel gls = PhotonNetwork.networkingPeer.TrafficStatsGameLevel;
        long elapsedMs = PhotonNetwork.networkingPeer.TrafficStatsElapsedMs / 1000;
        if (elapsedMs == 0)
        {
            elapsedMs = 1;
        }

        GUILayout.BeginHorizontal();
        this.buttonsOn = GUILayout.Toggle(this.buttonsOn, "buttons");
        this.trafficStatsOn = GUILayout.Toggle(this.trafficStatsOn, "traffic");
        this.healthStatsOn = GUILayout.Toggle(this.healthStatsOn, "health");
        GUILayout.EndHorizontal();

        string total = string.Format("Out|In|Sum:\t{0,4} | {1,4} | {2,4}", gls.TotalOutgoingMessageCount, gls.TotalIncomingMessageCount, gls.TotalMessageCount);
        string elapsedTime = string.Format("{0}sec average:", elapsedMs);
        string average = string.Format("Out|In|Sum:\t{0,4} | {1,4} | {2,4}", gls.TotalOutgoingMessageCount / elapsedMs, gls.TotalIncomingMessageCount / elapsedMs, gls.TotalMessageCount / elapsedMs);
        GUILayout.Label(total);
        GUILayout.Label(elapsedTime);
        GUILayout.Label(average);

        if (this.buttonsOn)
        {
            GUILayout.BeginHorizontal();
            statsOn = GUILayout.Toggle(statsOn, "stats on");
            if (GUILayout.Button("Reset"))
            {
                PhotonNetwork.networkingPeer.TrafficStatsReset();
                PhotonNetwork.networkingPeer.TrafficStatsEnabled = true;
            }
            statsToLog = GUILayout.Button("To Log");
            GUILayout.EndHorizontal();
        }

        string trafficStatsIn = string.Empty;
        string trafficStatsOut = string.Empty;
        if (this.trafficStatsOn)
        {
            trafficStatsIn = "Incoming: " + PhotonNetwork.networkingPeer.TrafficStatsIncoming.ToString();
            trafficStatsOut = "Outgoing: " + PhotonNetwork.networkingPeer.TrafficStatsOutgoing.ToString();
            GUILayout.Label(trafficStatsIn);
            GUILayout.Label(trafficStatsOut);
        }

        string healthStats = string.Empty;
        if (this.healthStatsOn)
        {
            healthStats = string.Format(
                "longest delta between\nsend: {0,4}ms disp: {1,4}ms\nlongest time for:\nev({3}):{2,3}ms op({5}):{4,3}ms",
                gls.LongestDeltaBetweenSending,
                gls.LongestDeltaBetweenDispatching,
                gls.LongestEventCallback,
                gls.LongestEventCallbackCode,
                gls.LongestOpResponseCallback,
                gls.LongestOpResponseCallbackOpCode);
            GUILayout.Label(healthStats);
        }

        if (statsToLog)
        {
            string complete = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}", total, elapsedTime, average, trafficStatsIn, trafficStatsOut, healthStats);
            Debug.Log(complete);
        }

        GUI.DragWindow();
    }
}
