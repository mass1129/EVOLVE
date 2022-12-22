using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_CharacterAiming : MonoBehaviour
{
    public float turnSpeed = 15;
    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    public Transform cameraLookAt;

    public Camera monsterCamera;
    GM_CharacterMotion charMove;

    public bool lockRot = false;
    void Start()
    {
        charMove = GetComponent<GM_CharacterMotion>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        xAxis.Update(Time.fixedDeltaTime);
        yAxis.Update(Time.fixedDeltaTime);

        cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);

        float yawCamera = monsterCamera.transform.rotation.eulerAngles.y;
        lockRot = charMove.isJumping || charMove.isRushing;
        if(!lockRot)
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,yawCamera,0), turnSpeed*Time.fixedDeltaTime);
    }

}
