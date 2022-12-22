using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_Test : MonoBehaviour
{
    public Transform firePos;

    void Start()
    {
        
    }

    
    void Update()
    {

        Debug.DrawRay(firePos.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction);
    }
}
