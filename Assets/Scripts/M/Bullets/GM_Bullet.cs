using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class GM_Bullet : MonoBehaviourPun, IPunObservable
{
    public int damage;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // isMine == true
        {
            stream.SendNext(transform.position);

        }
        else if (stream.IsReading) // isMine == false
        {
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}
