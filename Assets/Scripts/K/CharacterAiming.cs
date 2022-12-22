using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterAiming : MonoBehaviourPun
{
    public float turnSpeed = 15;
    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    public Transform cameraLookAt;

    public Camera monsterCamera;
    CharacterMotion charMove;

    public GameObject camPos;
    public bool lockRot = false;
    public List<GameObject> falseObject = new List<GameObject>();
    bool[] setactivefalse = new bool[5];
    void Start()
    {   
        if (photonView.IsMine)
        {
            falseObject.Add(GameObject.Find("ArenaGenerator"));
            falseObject.Add(GameObject.Find("Revive"));
            falseObject.Add(GameObject.Find("Arena"));
            falseObject.Add(GameObject.Find("CatchMonster"));
            
            
           
            charMove = GetComponent<CharacterMotion>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            camPos.gameObject.SetActive(true);
        }
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            
            for (int i = 0; i < falseObject.Count; i++)
            {
                
                    falseObject[i].SetActive(false);
                    
                
            }
            GameObject scan = GameObject.Find("Scan");
            if(scan != null)
            scan.SetActive(false);


            xAxis.Update(Time.deltaTime);
            yAxis.Update(Time.deltaTime);

            cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);

            float yawCamera = monsterCamera.transform.rotation.eulerAngles.y;
            lockRot = charMove.isJumping || charMove.isRushing;
            if (!lockRot)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.fixedDeltaTime);
        }
    }

    private void FixedUpdate()
    {
        //if (photonView.IsMine)
        //{
        //    xAxis.Update(Time.fixedDeltaTime);
        //    yAxis.Update(Time.fixedDeltaTime);

        //    cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);

        //    float yawCamera = monsterCamera.transform.rotation.eulerAngles.y;
        //    lockRot = charMove.isJumping || charMove.isRushing;
        //    if (!lockRot)
        //        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.fixedDeltaTime);
        //}
    }
 
}
