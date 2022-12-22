using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_RifleBullet : GM_Bullet
{
    Transform firePos;                      // 총구 위치
    Transform aimLookAt;                    // 총알 발사 위치
    public GameObject bulletImpactPref;     // 총알 적중 시 시각 효과
    public GameObject[] bulletHoles;        // 총알 적중 시 총알 구멍 효과
    public GameObject muzzleFlashPref;      // 총알 발사 시 발생하는 총구 불꽃xa

    float bulletSpeed = 40;
    Vector3 dir;
    Ray ray;
    RaycastHit hit;
    float currentTime = 0;

    void Start()
    {
        firePos = GameObject.Find("RifleFirePos").transform;
        aimLookAt = GameObject.Find("MedicAimLookAt").transform;
        dir = aimLookAt.position - firePos.position;
        ray = new Ray(firePos.position, dir);
        // 총구 불꽃 생성 후 1초 뒤에 제거
        GameObject muzzleFlash = Instantiate(muzzleFlashPref, firePos.position, Quaternion.LookRotation(-ray.direction));
        Destroy(muzzleFlash, 2);

        damage = 10;
    }

    void Update()
    {
        transform.position += ray.direction * bulletSpeed * Time.deltaTime;
        transform.forward = ray.direction;

        currentTime += Time.deltaTime;
        if (currentTime > 2 && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 총알 몬스터에게 적중 시 데미지 부여
        var hitBox = other.GetComponent<HitBox>();
        if (hitBox)
        {
            Debug.Log("hit");
            hitBox.OnRaycastHit(this, transform.forward);
        }

        GameObject bulletImpact = Instantiate(bulletImpactPref);
        GameObject bulletHole = Instantiate(bulletHoles[Random.Range(0, 4)]);

        // Trigger 되는 순간 총알의 위치에서 transform.foward 만큼의 거리 뺀 위치에서 Ray 쏴서 Raycast된 위치에 bulletImpact 생성
        if (Physics.Raycast(transform.position - transform.forward, ray.direction, out hit))
        {
            bulletImpact.transform.position = hit.point;
            bulletImpact.transform.forward = hit.normal;

            bulletHole.transform.position = hit.point;
            bulletHole.transform.forward = -hit.normal;
        }
        if(photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        Destroy(bulletHole, 5);
    }
}
