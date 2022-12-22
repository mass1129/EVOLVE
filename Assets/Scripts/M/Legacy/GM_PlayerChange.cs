using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_PlayerChange : MonoBehaviour
{
    public GameObject assultCam;
    public GameObject medic;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            assultCam.SetActive(false);
            medic.SetActive(true);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
