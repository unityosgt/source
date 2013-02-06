using UnityEngine;
using System.Collections;

public class EffectsManager : MonoBehaviour {
    public GameObject attack, attackAdditional, botAttack, potion, powerUp;

    private static EffectsManager instance;
    public static EffectsManager Instance
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
    public void Play(Effects effect)
    {
        GameObject go = null;
        if (effect == Effects.Attack)
            go = GameObject.Instantiate(attack, transform.position, Quaternion.identity) as GameObject;
        else if (effect == Effects.AdditionalAttack)
            go = GameObject.Instantiate(attackAdditional, transform.position, Quaternion.identity) as GameObject;
		else if (effect == Effects.BotAttack)
            go = GameObject.Instantiate(botAttack, transform.position, Quaternion.identity) as GameObject;
        else if (effect == Effects.PotionUse)
            go = GameObject.Instantiate(potion, transform.position, Quaternion.identity) as GameObject;
        else if (effect == Effects.PowerUpUse)
            go = GameObject.Instantiate(powerUp, transform.position, Quaternion.identity) as GameObject;

        go.transform.parent = this.transform;
        go.transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        if (!GameObject.FindWithTag("Player")) return;

        transform.position = GameObject.FindWithTag("Player").transform.position;
    }
}

public enum Effects
{
	Attack = 0,
    AdditionalAttack = 1,
	BotAttack= 2,
	PotionUse = 3,
    PowerUpUse = 4,
	Destroy = 5
}
