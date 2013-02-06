using UnityEngine;
using System.Collections;

public class inGameMenu : MonoBehaviour
{
    public Texture2D[] health;
    public GameObject localPlayer;
    //public bool OriginalOn = true; //<-- uncomment to Enable PickUpItems
    public GUIStyle myStyle;
    int i = 0;


    void OnGUI()
    {
	
	    
        /*if (OriginalOn == true) //<-- uncomment to Enable PickUpItems
        {
           
		   
            GUI.DrawTexture(new Rect(150, 38, 16, 16), PickUpItems.Instance.potionTexture);
            GUI.Label(new Rect(170, 33, 38, 38), PickUpItems.Instance.Potion.ToString());
			
            GUI.DrawTexture(new Rect(190, 38, 16, 16), PickUpItems.Instance.powerupsTexture);
            GUI.Label(new Rect(210, 33, 38, 38), PickUpItems.Instance.PowerUps.ToString());
        }*/
		
		int i = 0;
		
        while (i < (localPlayer.GetComponent<health>().life / 10))
        {
            i++;
        }
        GUI.Label(new Rect(15, 12, 38, 38), " HP", myStyle);
        GUI.DrawTexture(new Rect(20, -85, 225, 141), health[i]);
		
    }


}



