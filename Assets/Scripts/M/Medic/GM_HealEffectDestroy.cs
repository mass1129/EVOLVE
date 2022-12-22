using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_HealEffectDestroy : MonoBehaviourPun
{
    void Start()
    {
        
    }

    void Update()
    {
        StartCoroutine(Destroy());
    }
    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(10);
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
