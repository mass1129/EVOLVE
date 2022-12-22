using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class RockExplosion : MonoBehaviourPun, IPunObservable
{
    //Assignables

    public GameObject explosion;
    public LayerMask whatIsEnemies;
    public AudioSource explosionSFX;


    //Damage
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    //Lifetime
    [Range(0, 2)]
    public float maxLifetime;




    
    private void Start()
    {
        
        explosionSFX = GetComponent<AudioSource>();
    }
    public bool isExploded;
    private void Update()
    {

     
    }
    
    

    private void Delay()
    {
        Destroy(gameObject);
    }



    
    
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (!isExploded)
        {
            photonView.RPC("RpcOnTriggerEnter", RpcTarget.All,transform.position);
            
            

            isExploded = true;
        }


    }
    public float rotSpeed = 100;
    public GameObject ExplosionCol;
    [PunRPC]
    void RpcOnTriggerEnter(Vector3 position)
    {
        var impact = Instantiate(explosion) as GameObject;
        impact.transform.position = position;
        explosionSFX.PlayOneShot(explosionSFX.clip);
        Destroy(impact, 2);
        Invoke("Delay", 2f);
        ExplosionCol.SetActive(true);
        
       
        //Check for enemies 
        //isExploded = true;
        

   

    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
