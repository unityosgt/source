using UnityEngine;
using System.Collections;

public class AttackSFX : MonoBehaviour
    {

        void Update()
        {

            if (Input.GetMouseButtonUp(0) && !isDelayingAttack)
            {
			   StartCoroutine(DelayAttack());
               SoundManager.Instance.Play(SoundEffect.Attack, false);

            }

	    }
		
		    bool isDelayingAttack = false;
        	public float attackDelayTime = 1.0f;
        	IEnumerator DelayAttack()
            {
        	   if(!isDelayingAttack)
        	    {
		          isDelayingAttack = true;
		          yield return new WaitForSeconds(attackDelayTime);
		          isDelayingAttack = false;
		        }
                yield break;
            }   


    } 