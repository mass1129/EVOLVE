using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MonsterRushCollision : MonoBehaviourPun
{
    CharacterMotion cm;
    void Awake()
    {   
        gameObject.SetActive(false);
        cm = GetComponentInParent<CharacterMotion>();
    }
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("面倒1");
        if (other.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
                if (body == null || !other.CompareTag("Player"))
                    return;

                body.isKinematic = false;

                // Calculate push direction from move direction,
                // we only push objects to the sides never up and down

                Vector3 pushDir = new Vector3(other.gameObject.transform.position.x - transform.position.x, 0,
                    other.gameObject.transform.position.z - transform.position.z)+Vector3.up;
                pushDir.Normalize();
                Debug.Log("面倒5");
            // If you know how fast your character is trying to move,
            // then you can also multiply the push velocity by that.

            // Apply the push
            //body.AddForce(pushDir * cm.pushPower, ForceMode.Impulse);
            photonView.RPC("RpcAddForce", RpcTarget.All, body, pushDir);
            Debug.Log("面倒6");
        }
        }
    [PunRPC]
    void RpcAddForce(Rigidbody rg, Vector3 pushDir)
    {
        Debug.Log("面倒7");
        rg.AddForce(pushDir * cm.pushPower, ForceMode.Impulse);
        Debug.Log("面倒8");
    }
    
}
