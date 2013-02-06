using UnityEngine;
using System.Collections;

public class AIDetect : MonoBehaviour
{
    bool danger;
    private GameObject attacker;
    void Start()
    {
        InvokeRepeating("LoseLife", 0, 2);
    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit h;
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward), Color.red);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward), out h, 2) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.back), out h, 2) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.left), out h, 2) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.right), out h, 2)
            )
        {
            if (h.collider.gameObject.tag == "Enemy")
            {
                attacker = h.collider.gameObject;
				// uncomment below line to enable PickupItems
				//if (Input.GetMouseButtonUp(0) && !PickUpItems.Instance.SuperPower)
                if (Input.GetMouseButtonUp(0))
                {
                    h.collider.gameObject.GetComponent<BotAI>().health--;
                    h.collider.gameObject.GetComponent<BotAI>().Damage();
					
                }
				// uncomment below line to enable PickupItems
				//else if (Input.GetMouseButtonUp(0) && PickUpItems.Instance.SuperPower)
                else if (Input.GetMouseButtonUp(0))
                {
                    EffectsManager.Instance.Play(Effects.AdditionalAttack);
                    h.collider.gameObject.GetComponent<BotAI>().health = -1;
                    h.collider.gameObject.GetComponent<BotAI>().Damage();
                    PickUpItems.Instance.SuperPower = false;
                }

                danger = true;
            }
            else danger = false;
        }
        else danger = false;
    }

    void LoseLife()
    {
        if (danger)
        {
            Debug.Log("hahah");
            attacker.GetComponent<BotAI>().Attack();
            GetComponent<health>().life-=10;
        }
    }
	
}
