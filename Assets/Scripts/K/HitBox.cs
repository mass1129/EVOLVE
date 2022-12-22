using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Health health;

    public void OnRaycastHit(GM_Bullet bullet, Vector3 direction)
    {
        health.TakeDamage(bullet.damage, direction * 5);

    }

    public void OnLasertHit(float damage, Vector3 direction)
    {
        health.TakeDamage(damage, direction * 5);
    }
}
