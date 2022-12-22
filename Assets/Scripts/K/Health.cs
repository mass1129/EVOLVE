using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Health : MonoBehaviourPun
{
    public float maxHealth;
    public float currentHealth;

    public float blinkIntensity = 0.6f;
    public float blinkDuration = 0.3f;
    float blinkTimer;

    SkinnedMeshRenderer skinnedMeshRenderer;







    private void Start()
    {

        if (!photonView.IsMine) return;
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();


        currentHealth = maxHealth;
        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodies)
        {
            HitBox hitBox = rigidBody.gameObject.AddComponent<HitBox>();
            hitBox.health = this;



        }
        OnStart();
    }

    public void TakeDamage(float amount, Vector3 direction)
    {
        if (!photonView.IsMine) return;
        

        OnDamage(amount, direction);
        if (currentHealth <= 0.0f)
        {
            Die(direction);
        }
    }
    
    public void Die(Vector3 direction)
    {
        if (!photonView.IsMine) return;
        GameManager.instance.GameOver(0);
        OnDeath(direction);
        //Destroy(gameObject, 5f);
    }



    private void Update()
    {
        if (!photonView.IsMine) return;
        OnUpdate();



    }





    protected virtual void OnStart()
    {

    }

    protected virtual void OnDeath(Vector3 direction)
    {

    }
    protected virtual void OnDamage(float amount, Vector3 direction)
    {

    }
    protected virtual void OnUpdate()
    {

    }
}
