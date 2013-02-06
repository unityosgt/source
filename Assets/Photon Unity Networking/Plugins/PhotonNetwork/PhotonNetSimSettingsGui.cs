using UnityEngine;


/// <summary>
/// This MonoBehaviour is a basic GUI for the Photon client's network-simulation feature.
/// It can modify lag (fixed delay), jitter (random lag) and packet loss.
/// </summary>
public class PhotonNetSimSettingsGui : MonoBehaviour
{
    public void OnGUI()
    {
        GUILayout.Label(string.Format("Rtt: {0,3} +/- {1,3}", PhotonNetwork.networkingPeer.RoundTripTime, PhotonNetwork.networkingPeer.RoundTripTimeVariance));

        bool simEnabled = PhotonNetwork.networkingPeer.NetworkSimulationSettings.IsSimulationEnabled;
        bool newSimEnabled = GUILayout.Toggle(simEnabled, "sim");
        if (newSimEnabled != simEnabled)
        {
            PhotonNetwork.networkingPeer.NetworkSimulationSettings.IsSimulationEnabled = newSimEnabled;
        }

        float inOutLag = PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingLag;
        GUILayout.Label("lag " + inOutLag);
        inOutLag = GUILayout.HorizontalSlider(inOutLag, 0, 500);
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingLag = (int)inOutLag;
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.OutgoingLag = (int)inOutLag;

        float inOutJitter = PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingJitter;
        GUILayout.Label("jit " + inOutJitter);
        inOutJitter = GUILayout.HorizontalSlider(inOutJitter, 0, 100);
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingJitter = (int)inOutJitter;
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.OutgoingJitter = (int)inOutJitter;

        float loss = PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingLossPercentage;
        GUILayout.Label("loss " + loss);
        loss = GUILayout.HorizontalSlider(loss, 0, 10);
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.IncomingLossPercentage = (int)loss;
        PhotonNetwork.networkingPeer.NetworkSimulationSettings.OutgoingLossPercentage = (int)loss;
    }
}