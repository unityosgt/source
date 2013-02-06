using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
    public AudioSource runSource, attackSource, botAttackSource, pickUpSource, potionSource, powerUpSource;
    public AudioClip run, attack, botAttack, pickUp, potion, powerUp;
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        runSource.clip = run;
        attackSource.clip = attack;
        botAttackSource.clip = botAttack;
        pickUpSource.clip = pickUp;
        potionSource.clip = potion;
        powerUpSource.clip = powerUp;
    }

    public void Play(SoundEffect effect, bool loop)
    {
        if (effect == SoundEffect.Walk)
        {
            if (!runSource.isPlaying)
            {
                runSource.loop = loop;
                runSource.Play();
            }
        }
        else if (effect == SoundEffect.Attack)
        {
            if (!attackSource.isPlaying)
            {
                attackSource.loop = loop;
                attackSource.Play();
            }
        }
        else if (effect == SoundEffect.BotAttack)
        {
            if (!botAttackSource.isPlaying)
            {
                botAttackSource.loop = loop;
                botAttackSource.Play();
            }
        }
        else if (effect == SoundEffect.PickUp)
        {
            if (!pickUpSource.isPlaying)
            {
                pickUpSource.loop = loop;
                pickUpSource.Play();
            }
        }
        else if (effect == SoundEffect.Potion)
        {
            if (!potionSource.isPlaying)
            {
                potionSource.loop = loop;
                potionSource.Play();
            }
        }
        else if (effect == SoundEffect.PowerUp)
        {
            if (!powerUpSource.isPlaying)
            {
                powerUpSource.loop = loop;
                powerUpSource.Play();
            }
        }
    }

    public void Stop(SoundEffect effect)
    {
        if (effect == SoundEffect.Walk)
        {
            if (runSource.isPlaying)
            {
                runSource.Stop();
            }
        }
        else if (effect == SoundEffect.Attack)
        {
            if (attackSource.isPlaying)
            {
                attackSource.Stop();
            }
        }
        else if (effect == SoundEffect.BotAttack)
        {
            if (botAttackSource.isPlaying)
            {
                botAttackSource.Stop();
            }
        }
        else if (effect == SoundEffect.PickUp)
        {
            if (pickUpSource.isPlaying)
            {
                pickUpSource.Stop();
            }
        }
        else if (effect == SoundEffect.Potion)
        {
            if (potionSource.isPlaying)
            {
                potionSource.Stop();
            }
        }
        else if (effect == SoundEffect.PowerUp)
        {
            if (powerUpSource.isPlaying)
            {
                powerUpSource.Stop();
            }
        }
    }

    void Update()
    {
        if (!GameObject.FindWithTag("Player")) return;

        transform.position = GameObject.FindWithTag("Player").transform.position;
    }
}

public enum SoundEffect
{
    Walk = 0,
    Attack = 1,
    BotAttack = 2,
    PickUp = 3,
    Potion = 4,
    PowerUp = 5
}