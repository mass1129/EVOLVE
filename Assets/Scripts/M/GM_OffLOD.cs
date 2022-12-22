using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_OffLOD : MonoBehaviour
{
    void Start()
    {
        for(int i = 0; i < GetComponentsInChildren<LODGroup>().Length; i++)
        {
            GetComponentsInChildren<LODGroup>()[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
