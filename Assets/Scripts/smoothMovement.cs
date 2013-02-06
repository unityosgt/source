using UnityEngine;
using System.Collections;

public class SmoothMovement : Photon.MonoBehaviour
{


    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this
    public string playerName = "";
    move moveScript;
    health healthScript;

    void Start()
    {
        if (photonView.isMine)
        {
            //photonView.RPC("namePlayer", PhotonTargets.All);
            playerName = PhotonNetwork.playerName;
            tag = "Player";
        }
        else
        {
            tag = "Other";
            Destroy(GetComponent<HeroCamera>());
        }
    }

    void Awake()
    {
        moveScript = GetComponent<move>();
        healthScript = GetComponent<health>();

    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext((int)moveScript.point);
            stream.SendNext((int)moveScript.playerTeam);
            stream.SendNext((int)moveScript.skinColor);

            stream.SendNext((bool)moveScript.idleA);
            stream.SendNext((bool)moveScript.runA);
            stream.SendNext((bool)moveScript.jumpA);
            stream.SendNext((bool)moveScript.punch1A);
            stream.SendNext((bool)moveScript.punch2A);
            stream.SendNext((bool)moveScript.punch3A);
            stream.SendNext((bool)moveScript.deathA);

            stream.SendNext((bool)healthScript.isDeath);
            stream.SendNext((bool)healthScript.death);
            stream.SendNext((int)healthScript.life);



            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

        }
        else
        {

            moveScript.point = (int)stream.ReceiveNext();
            moveScript.playerTeam = (int)stream.ReceiveNext();
            moveScript.skinColor = (int)stream.ReceiveNext();

            moveScript.idleA = (bool)stream.ReceiveNext();
            moveScript.runA = (bool)stream.ReceiveNext();
            moveScript.jumpA = (bool)stream.ReceiveNext();
            moveScript.punch1A = (bool)stream.ReceiveNext();
            moveScript.punch2A = (bool)stream.ReceiveNext();
            moveScript.punch3A = (bool)stream.ReceiveNext();

            moveScript.deathA = (bool)stream.ReceiveNext();

            healthScript.death = (bool)stream.ReceiveNext();
            healthScript.isDeath = (bool)stream.ReceiveNext();
            healthScript.life = (int)stream.ReceiveNext();

            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }

    }

    void Update()
    {


        if (!photonView.isMine)
        {

            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 15);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 15);
        }
    }

}
