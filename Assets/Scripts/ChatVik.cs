using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This simple chat example showcases the use of RPC targets and targetting certain players via RPCs.
/// </summary>
public class ChatVik : Photon.MonoBehaviour
{

    public static ChatVik SP;
    public List<string> messages = new List<string>();

    private int chatHeight = (int)100;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";
    private float lastUnfocusTime = 0;

    void Awake()
    {
        SP = this;
    }

    void OnGUI()
    {        
        GUI.SetNextControlName("");
      //  GUI.DrawTexture(new Rect(0, Screen.height - 230, 280, 330), Resources.Load("gray") as Texture);
        GUILayout.BeginArea(new Rect(0, Screen.height - 230, 280, 200));
        
        //Show scroll list of chat messages
        //scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUI.color = Color.white;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(messages[i]);
        }
        //GUILayout.EndScrollView();
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(0, Screen.height - 30, 280, 30));
        GUI.SetNextControlName("ChatField");
    	chatInput = GUILayout.TextField(chatInput,1000, GUILayout.MinWidth(270) );
       
        if (Event.current.type == EventType.keyDown && Event.current.character == '\n'){
            if (GUI.GetNameOfFocusedControl() == "ChatField")
            {                
                SendChat(PhotonTargets.All);
                lastUnfocusTime = Time.time;
                GUI.FocusControl("");
                GUI.UnfocusWindow();
            }
            else
            {
                if (lastUnfocusTime < Time.time - 0.1f)
                {
                    GUI.FocusControl("ChatField");
                }
            }
        }

        //if (GUILayout.Button("SEND", GUILayout.Height(17)))
         //   SendChat(PhotonTargets.All);
       

    

        GUILayout.EndArea();
    }

    public void AddMessage(string text)
    {
        SP.messages.Add(text);
        if (SP.messages.Count > 8)
            SP.messages.RemoveAt(0);
    }


    [RPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[" + info.sender + "] " + text);
    }

    void SendChat(PhotonTargets target)
    {
        if (chatInput != "")
        {
            photonView.RPC("SendChatMessage", target, chatInput);
            chatInput = "";
        }
    }

    void SendChat(PhotonPlayer target)
    {
        if (chatInput != "")
        {
            chatInput = "[PM] " + chatInput;
            photonView.RPC("SendChatMessage", target, chatInput);
            chatInput = "";
        }
    }

    void OnLeftRoom()
    {
        this.enabled = false;
    }

    void OnJoinedRoom()
    {
        this.enabled = true;
    }
    void OnCreatedRoom()
    {
        this.enabled = true;
    }
}
