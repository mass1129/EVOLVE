using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MonsterHealth : Health
{
    Ragdoll ragdoll;
    public UIHealthBar healthBar;
    public MonoBehaviour[] disableCom;
    protected override void OnStart()
    {
        if (!photonView.IsMine) return;
            healthBar.gameObject.SetActive(true);
        ragdoll = GetComponent<Ragdoll>();
        healthBar.photonView.RPC("SetActiveHealthBar", RpcTarget.Others, false);
    }


    protected override void OnDeath(Vector3 direction)
    {   
        ragdoll.ActivateRagdoll();
        for (int i = 0; i < disableCom.Length; i++)
            disableCom[i].enabled = false;
    }
    
    protected override void OnDamage(float amount, Vector3 direction)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("RBCCurrentHealth", RpcTarget.All, amount);
        currentTime = 0;
        healthBar.photonView.RPC("SetActiveHealthBar", RpcTarget.Others, true);
        photonView.RPC("RPCSetHealthBar", RpcTarget.All);
        //healthBar.SetHealthBarPercentage(currentHealth / maxHealth);

    }

    float delayHealing = 5;
    float currentTime = 0;
    float healAmount = -10f;
    protected override void OnUpdate()
    {
        if (!photonView.IsMine) return;
        currentTime += Time.deltaTime;
        if (currentTime > delayHealing && currentHealth > 0&&currentHealth<=maxHealth-10)
        {
            photonView.RPC("RBCCurrentHealth", RpcTarget.All, healAmount);
            photonView.RPC("RPCSetHealthBar", RpcTarget.All);
            healthBar.photonView.RPC("SetActiveHealthBar", RpcTarget.Others, false);
            currentTime = 0;
        }
        if (GameManager.instance.playerWin[0] || GameManager.instance.playerWin[1])
        {
            photonView.RPC("RPCMonActiveFalse", RpcTarget.All);
        }
    }
    [PunRPC]
    void RPCMonActiveFalse()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    void RPCSetHealthBar()
    {   
        
        healthBar.SetHealthBarPercentage(currentHealth / maxHealth);
    }
    [PunRPC]
    void RBCCurrentHealth(float amount)
    {
        currentHealth -= amount;


    }
}
