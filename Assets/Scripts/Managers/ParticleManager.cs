using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    ObjectPool PoolInstance;

    // Start is called before the first frame update
    void Start()
    {
        PoolInstance = ObjectPool.poolManager;
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

    public void PlayEffect(Vector3 position, string particleName)
    {
        PoolInstance.SpawnFromPool(particleName, position, true).GetComponent<ParticleSystem>().Play();

    }
}
