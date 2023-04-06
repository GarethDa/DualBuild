using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdSpeedRandomizer : MonoBehaviour
{
    [SerializeField]
    private List<Texture> textures; // A list of textures to choose from

    [SerializeField] private List<Texture> rareBoys;

    void Start()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetFloat("SpeedScalar", Random.Range(0.0f, 2.0f));

        Renderer rend = GetComponent<Renderer>();

        int rareInt = Random.Range(0, 10);

        Texture rando;

        if (rareInt == 9)
            rando = rareBoys[Random.Range(0, rareBoys.Count)];

        else
            rando = textures[Random.Range(0, textures.Count)];

        rend.material.mainTexture = rando;
    }
}
