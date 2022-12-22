using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_MissileBullet : GM_Bullet
{
    Transform firePos;               // �ѱ� ��ġ
    Transform aimLookAt;             // �Ѿ� �߻� ��ġ
    public GameObject bulletImpactPref;     // �Ѿ� ���� �� �ð� ȿ��
    public GameObject explosionPref;        // �Ѿ� ���� �� ���� ȿ��
    public GameObject muzzleFlashPref;      // �Ѿ� �߻� �� �߻��ϴ� �ѱ� �Ҳ�
    
    float bulletSpeed = 30;
    Vector3 dir;
    Ray ray;
    RaycastHit hit;
    float currentTime = 0;

    void Start()
    {
        firePos = GameObject.Find("MissileFirePos").transform;
        aimLookAt = GameObject.Find("AssultAimLookAt").transform;
        dir = aimLookAt.position - firePos.position;
        ray = new Ray(firePos.position, dir);
        // �ѱ� �Ҳ� ���� �� 1�� �ڿ� ����
        GameObject muzzleFlash = Instantiate(muzzleFlashPref, firePos.position, Quaternion.LookRotation(-ray.direction));
        Destroy(muzzleFlash, 2);

        damage = 40;
    }

    void Update()
    {
        transform.position += ray.direction * bulletSpeed * Time.deltaTime;
        transform.forward = ray.direction;

        currentTime += Time.deltaTime;
        if (currentTime > 5 && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �Ѿ� ���Ϳ��� ���� �� ������ �ο�
        var hitBox = other.GetComponent<HitBox>();
        if (hitBox)
        {
            Debug.Log("hit");
            hitBox.OnRaycastHit(this, transform.forward);
        }

        GameObject bulletImpact = Instantiate(bulletImpactPref);
        GameObject explosion = Instantiate(explosionPref);

        // Trigger �Ǵ� ���� �Ѿ��� ��ġ���� transform.foward ��ŭ�� �Ÿ� �� ��ġ���� Ray ���� Raycast�� ��ġ�� bulletImpact ����
        if (Physics.Raycast(transform.position - transform.forward, ray.direction, out hit))
        {
            bulletImpact.transform.position = hit.point;
            bulletImpact.transform.forward = hit.normal;

            explosion.transform.position = hit.point;
            explosion.transform.forward = hit.normal;
        }
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        Destroy(explosion, 3);
    }
}
