using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpScript : MonoBehaviour
{
    private GameObject playerObject;
    private float initialJumpForce; //grabbed value at start
    private bool usedPowerUp = false; //if we have used our powerup
    public float powerUpDuration = 3f; //total duration of ability in seconds
    private float currentPowerUpDuration = 0f; //internal clock for powerup duration

    enum powerUpList { None, SuperJump, SlowFall}
    [SerializeField] powerUpList selectedPowerUp;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = gameObject;
        initialJumpForce = playerObject.GetComponent<TpMovement>().GetJumpForce();
    }

    void OnPowerUp()
    {
        //in the future this should probably be part of a game manager, but for now it's on the player//
        //there should probably also be an observer to check when the player actually uses their boosted ablility and then resets
        //their stats and removes their powerup
        //for now, we'll just have it on a timer
        if (!usedPowerUp)
        {
            usedPowerUp = true;
            currentPowerUpDuration = 0;

            if (selectedPowerUp == powerUpList.SuperJump)
            {
                playerObject.GetComponent<TpMovement>().SetJumpForce(20);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //constantly adds up INGAME time, in case we want to use slow motion or something
        if (usedPowerUp)
        {
            currentPowerUpDuration += Time.fixedDeltaTime;
        }

        //if we're over our timer and we actually used a powerup
        if (currentPowerUpDuration >= powerUpDuration && usedPowerUp)
        {
            //reset our used state and our duration
            usedPowerUp = false;
            currentPowerUpDuration = 0;

            if (selectedPowerUp == powerUpList.SuperJump)
            {
                playerObject.GetComponent<TpMovement>().SetJumpForce(initialJumpForce);

            }
        }
    }
}