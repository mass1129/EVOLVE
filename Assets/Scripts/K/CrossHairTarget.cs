using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CrossHairTarget : MonoBehaviourPun, IPunObservable
{

    public Camera monsterCamera;
    //Ray ray;
    //RaycastHit hitInfo;
    [SerializeField]
    private LayerMask Mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = monsterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));



        if (Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, Mask))
        {
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * 1000.0f;
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
