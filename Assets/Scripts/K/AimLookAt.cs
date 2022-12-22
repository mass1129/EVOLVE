using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AimLookAt : MonoBehaviourPun, IPunObservable
{
    // Start is called before the first frame update
    public Camera monsterCam;

    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position = monsterCam.transform.position + monsterCam.transform.forward * 30;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
