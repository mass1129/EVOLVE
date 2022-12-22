using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_Shoot : MonoBehaviourPun
{
    public GameObject weapon0_Bullet;
    public Transform weapon0_FirePos;
    public GameObject weapon2_Bullet;
    public Transform weapon2_FirePos;

    public GameObject weapon1_Bullet;
    public Transform weapon1_FirePos;
    public GameObject groggy_Bullet;
    public Transform groggy_FirePos;

    public GM_Hunter entityClass;

    void Shoot_Weapon0()
    {
        if(photonView.IsMine)
            PhotonNetwork.Instantiate(weapon0_Bullet.name, weapon0_FirePos.position, weapon0_FirePos.rotation);
    }

    void Shoot_Weapon1()
    {
        if (photonView.IsMine)
            PhotonNetwork.Instantiate(weapon1_Bullet.name, weapon1_FirePos.position, weapon1_FirePos.rotation);
    }

    void Shoot_Weapon2()
    {
        if (photonView.IsMine)
            PhotonNetwork.Instantiate(weapon2_Bullet.name, weapon2_FirePos.position, weapon2_FirePos.rotation);
    }

    void Shoot_Groggy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Instantiate(groggy_Bullet.name, groggy_FirePos.position, groggy_FirePos.rotation);
    }

    void ConsumeBullet(int weaponType)
    {
        if (weaponType == 0)
            entityClass.weapon0_currentAmo--;
        else
            entityClass.weapon1_currentAmo--;
    }
}
