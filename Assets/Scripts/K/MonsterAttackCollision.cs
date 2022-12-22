using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackCollision : MonoBehaviour
{
    public float durationTime;
    public int damage;
    public Transform monster;
    private void OnEnable()
    {
        StartCoroutine("AutoDisable");
    }
    public float rotSpeed = 10;
    public bool pushAttack;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {   
            if(pushAttack)
            {
                Debug.Log("1차충돌");
                Vector3 dir = monster.position - other.transform.position;
                Debug.Log(dir);
                dir.y = 0;
                dir.Normalize();
                other.transform.rotation = Quaternion.RotateTowards(other.transform.rotation, Quaternion.LookRotation(dir), rotSpeed);
                Debug.Log("2차충돌");
                other.GetComponent<GM_Hunter>().ChangeState(HunterStates.Pushed);
            }
            

            
            other.GetComponent<GM_Hunter>().TakeDamage(damage);
            
            
        }
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(durationTime);
        gameObject.SetActive(false);
    }
}
