using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_JetPack : MonoBehaviour
{
    public GameObject jetPackLeft;
    public GameObject jetPackRight;
    ParticleSystem[] particlesLeft;
    ParticleSystem[] particlesRight;
    public AudioSource jetEngineSound;
    bool isJetEngineActive;

    private void Start()
    {
        particlesLeft = jetPackLeft.GetComponentsInChildren<ParticleSystem>();
        particlesRight = jetPackRight.GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (!isJetEngineActive)
            jetEngineSound.volume -= Time.deltaTime;
        else
            jetEngineSound.volume = 1;
    }
    void OnJetPack()
    {
        for (int i = 0; i < particlesLeft.Length; i++)
        {
            var em_L = particlesLeft[i].emission;
            var em_R = particlesRight[i].emission;
            em_L.enabled = true;
            em_R.enabled = true;
            particlesLeft[i].Play();
            particlesRight[i].Play();
        }
        isJetEngineActive = true;
        jetEngineSound.Stop();
        jetEngineSound.Play();
        
    }

    void OffJetPack()
    {
        for (int i = 0; i < particlesLeft.Length; i++)
        {
            var em_L = particlesLeft[i].emission;
            var em_R = particlesRight[i].emission;
            em_L.enabled = false;
            em_R.enabled = false;
        }
        isJetEngineActive = false;
    }
}
