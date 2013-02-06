using UnityEngine;
using System.Collections;

public class PickUpItems : MonoBehaviour
{
    public Texture potionTexture, powerupsTexture;
    private bool superPower = false;
    public bool SuperPower
    {
        get { return superPower; }
        set { superPower = value; }
    }
    private int potion = 0;
    public int Potion
    {
        get { return potion; }
    }
    private int powerUps = 0;
    public int PowerUps
    {
        get { return powerUps; }
    }

    Vector3 fwd;
    RaycastHit hit;

    private static PickUpItems instance;
    public static PickUpItems Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Health")
        {
            SoundManager.Instance.Play(SoundEffect.PickUp, false);
			Destroy(collision.gameObject);
            potion++;
            
        }
        else if (collision.gameObject.tag == "PowerUp")
        {
            SoundManager.Instance.Play(SoundEffect.PickUp, false);
			Destroy(collision.gameObject);
            powerUps++;
            
        }
    }

    void Update()
    {
        if (!GameObject.FindWithTag("Player")) return;
        if (this.gameObject.tag != "Player") Destroy(gameObject.GetComponent<PickUpItems>());

        fwd =  transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out hit, 0.7f))
        {
            if (hit.collider.gameObject.tag == "Health")
            {
			    SoundManager.Instance.Play(SoundEffect.PickUp, false);
                Destroy(hit.collider.gameObject);
                potion++;
            }
            else if (hit.collider.gameObject.tag == "PowerUp")
            {
			    SoundManager.Instance.Play(SoundEffect.PickUp, false);
                Destroy(hit.collider.gameObject);
                powerUps++;
            }
        }

        if (Input.GetKeyUp(KeyCode.F) && potion > 0)
        {
            SoundManager.Instance.Play(SoundEffect.Potion, false);
            EffectsManager.Instance.Play(Effects.PotionUse);
            GetComponent<health>().life = 100;
            potion--;
        }

        if ((Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.G)) && powerUps > 0)
        {
            SoundManager.Instance.Play(SoundEffect.PowerUp, false);
            EffectsManager.Instance.Play(Effects.PowerUpUse);
            superPower = true;
            powerUps--;
        }
    }
}
