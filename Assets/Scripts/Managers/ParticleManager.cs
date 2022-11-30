using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    ParticleSystem[] childSystems;
    ObjectPool PoolInstance;

    // Start is called before the first frame update
    void Start()
    {
        PoolInstance = ObjectPool.poolManager;
        childSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayEffect(Vector3 position)
    {
        PoolInstance.SpawnFromPool("WhiteParticles", position, true).GetComponent<ParticleSystem>().Play();

        //childSystems[index].transform.position = position;
        //childSystems[index].Play();
        //childSystems[index].is
    }
}
