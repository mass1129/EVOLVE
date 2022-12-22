using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_ChangeWeapon : MonoBehaviour
{
    public GameObject weapon0_hand;
    public GameObject weapon0_back;
    public GameObject weapon2_hand;
    public GameObject weapon2_back;

    public GameObject weapon1_hand;
    public GameObject weapon1_back;
    public GameObject weapon_groggy;

    public void ActiveWeapon0()
    {
        weapon0_hand.SetActive(true);
        weapon0_back.SetActive(false);
        if(weapon2_hand != null && weapon2_back != null)
        {
            weapon2_hand.SetActive(true);
            weapon2_back.SetActive(false);
        }

        weapon1_hand.SetActive(false);
        weapon1_back.SetActive(true);
    }

    public void ActiveWeapon1()
    {
        weapon0_hand.SetActive(false);
        weapon0_back.SetActive(true);
        if (weapon2_hand != null && weapon2_back != null)
        {
            weapon2_hand.SetActive(false);
            weapon2_back.SetActive(true);
        }

        weapon1_hand.SetActive(true);
        weapon1_back.SetActive(false);
    }

    public void ActiveGroggyWeapon()
    {
        weapon0_hand.SetActive(false);
        weapon1_hand.SetActive(false);
        if (weapon2_hand != null)
            weapon2_hand.SetActive(false);
        weapon_groggy.SetActive(true);
    }
}
