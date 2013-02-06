using UnityEngine;
using System.Collections;

/** AI controller specifically made for the spider robot.
 * The spider robot (or mine-bot) which is got from the Unity Example Project
 * can have this script attached to be able to pathfind around with animations working properly.\n
 * This script should be attached to a parent GameObject however since the original bot has Z+ as up.
 * This component requires Z+ to be forward and Y+ to be up.\n
 * 
 * It overrides the AIPath class, see that class's documentation for more information on most variables.\n
 * Animation is handled by this component. The Animation component refered to in #anim should have animations named "awake" and "forward".
 * The forward animation will have it's speed modified by the velocity and scaled by #animationSpeed to adjust it to look good.
 * The awake animation will only be sampled at the end frame and will not play.\n
 * When the end of path is reached, if the #endOfPathEffect is not null, it will be instantiated at the current position. However a check will be
 * done so that it won't spawn effects too close to the previous spawn-point.
 */
 
[RequireComponent(typeof(Seeker))]
public class BotAI : AIPath {
	
	/** Animation component.
	 * Should hold animations "awake" and "forward"
	 */
	public Animation anim;
    public AnimationClip IdleAnimation, damageAnimation, walkAnimation, deathAnimation, attackAnimation;
    public float distanceToPlayer = 20;
	/** Minimum velocity for moving */
	public float sleepVelocity = 0.4F;
	
	/** Speed relative to velocity with which to play animations */
	public float animationSpeed = 0.2F;
	
	/** Effect which will be instantiated when end of path is reached.
	 * \see OnTargetReached */
	public GameObject endOfPathEffect;

    public BotState botState;
    
	public int defaultHealth = 100;
    public int health = 100;
	
    /*uncomment to Enable Prefab Pickitems*/
    //public GameObject itemPrefab;
	
	public PhotonView myPhotonView;

    private bool goToHeaven = false, damage = false, attack = false, death = false;

	public new void Start () {
        tag = "Enemy";
		botState = BotState.Idle;
        canMove = true;
        canSearch = true;
		//Call Start in base script (AIPath)
		health = defaultHealth;
		base.Start ();
	}
	
	/** Point for the last spawn of #endOfPathEffect */
	protected Vector3 lastTarget;
	
	/**
	 * Called when the end of path has been reached.
	 * An effect (#endOfPathEffect) is spawned when this function is called
	 * However, since paths are recalculated quite often, we only spawn the effect
	 * when the current position is some distance away from the previous spawn-point
	*/
	public override void OnTargetReached () {
		if (endOfPathEffect != null && Vector3.Distance (tr.position, lastTarget) > 1) {
			GameObject.Instantiate (endOfPathEffect,tr.position,tr.rotation);
			lastTarget = tr.position;
		}
	}
	
	public override Vector3 GetFeetPosition ()
	{
		return tr.position;
	}
	
	protected new void FixedUpdate () {
        if (death)
        {
            return;
        }

        if (botState == BotState.Idle)
            anim.CrossFade(IdleAnimation.name);
        else if (botState == BotState.Walk)
            anim.CrossFade(walkAnimation.name);
        else if (botState == BotState.Damage)
            anim.CrossFade(damageAnimation.name);
        else if (botState == BotState.Attack)
            anim.CrossFade(attackAnimation.name);
        else if (botState == BotState.Death)
            anim.CrossFade(deathAnimation.name);

        if (health <= 0)
        {
            botState = BotState.Death;
        }

        if (health <= 0 && !goToHeaven)
        {
            goToHeaven = true;
            tag = "Untagged";
            target = null;
            canMove = false;
            canSearch = false;
            botState = BotState.Death;
            StartCoroutine(Die());
            return;
        }

        if (!GameObject.FindWithTag("Player")) return;

        if (!death)
            target = GameObject.FindWithTag("Player").transform;
        else
            target = null;


        if (attack || damage) return;

        if (Vector3.Distance(transform.position, target.position) > distanceToPlayer && !attack && health > 0 && !damage)
        {
            anim.CrossFade(IdleAnimation.name);
            return;
        }
        else if (Vector3.Distance(transform.position, target.position) <= distanceToPlayer)
        {
            //Get velocity in world-space
            Vector3 velocity;
            if (canMove)
            {

                //Calculate desired velocity
                Vector3 dir = CalculateVelocity(GetFeetPosition());

                //Rotate towards targetDirection (filled in by CalculateVelocity)
                if (targetDirection != Vector3.zero)
                {
                    RotateTowards(targetDirection);
                }

                if (dir.sqrMagnitude > sleepVelocity * sleepVelocity)
                {
                    //If the velocity is large enough, move
                }
                else
                {
                    //Otherwise, just stand still (this ensures gravity is applied)
                    dir = Vector3.zero;
                }

                if (navController != null)
                    navController.SimpleMove(GetFeetPosition(), dir);
                else if (controller != null)
                    controller.SimpleMove(dir);
                else
                    Debug.LogWarning("No NavmeshController or CharacterController attached to GameObject");

                velocity = controller.velocity;
            }
            else
            {
                velocity = Vector3.zero;
            }
        }

        if (health > 0 && !damage && !attack && Vector3.Distance(transform.position, target.position) > 2)
            botState = BotState.Walk;
	}

    public void Damage()
    {
        if (!attack)
        StartCoroutine(DoDamage());
    }

    IEnumerator DoDamage()
    {
        botState = BotState.Damage;
        yield return new WaitForSeconds(damageAnimation.length);
        damage = false;
    }

    public void Attack()
    {
		//EffectsManager.Instance.Play(Effects.BotAttack);
	    //SoundManager.Instance.Play(SoundEffect.BotAttack, false);
        StartCoroutine(DoAttack());
    }

    IEnumerator DoAttack()
    {
        attack = true;
        botState = BotState.Attack;
        yield return new WaitForSeconds(attackAnimation.length);
        attack = false;
    }

    IEnumerator Die()
    {
        
        yield return new WaitForSeconds(deathAnimation.length + 1);
        GameObject gi = Instantiate(this.gameObject, new Vector3(transform.position.x + Random.Range(-5, 5), transform.position.y, transform.position.z + Random.Range(-5, 5)), Quaternion.identity) as GameObject;
        gi.GetComponent<BotAI>().health = 100;
        
        death = true;
        Destroy(this.gameObject);

	    /*uncomment to Enable Prefab Pickitems*/
        //if (Random.Range(0, 1) == 0)
        // Instantiate(itemPrefab, transform.position, Quaternion.identity);

    }
}

public enum BotState
{
    Idle,
    Walk,
    Damage,
    Attack,
    Death
}