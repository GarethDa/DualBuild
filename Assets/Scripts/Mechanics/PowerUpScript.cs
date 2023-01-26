using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpScript : MonoBehaviour
{
    private GameObject playerObject;
    private Rigidbody rb; //player rigidboy

    //Super Jump Vairables
    private bool superjumpEnabled = false;
    private float initialJumpForce; //grabbed value at start
    [SerializeField] [Range(1.0f, 40.0f)] private float superjumpForce = 35.0f;

    //Slowfall variables
    private bool slowfallEnabled = false;
    [SerializeField] [Range(1.0f, 20.0f)] private float slowfallForce = 10.0f;
    [SerializeField] [Range(-1.0f, -10.0f)] private float maxFallSpeed = -4.0f;
    private bool playerGrounded = true;
    [SerializeField] [Range(1.0f, 10.0f)] private float slowfallDuration = 3.0f;

    //Dash variables
    private bool dashEnabled = false;
    private float initialSpeed; //grabbed value at start
    [SerializeField] [Range(1.0f, 50.0f)] private float dashSpeed = 30.0f;
    [SerializeField] [Range(1.0f, 10.0f)] private float dashDuration = 3.0f;

    //Bomb Variables
    private bool bombEnabled = false;
    [SerializeField] private GameObject bombPrefab;

    private bool usedPowerUp = false; //if we have used our powerup
    public float powerUpDuration = 3f; //total duration of ability in seconds
    private float currentPowerUpDuration = 0f; //internal clock for powerup duration

    
    [SerializeField] public powerUpList selectedPowerUp;
    
    // Start is called before the first frame update
    void Start()
    {
        playerObject = gameObject;//the player this script is attached to
        rb = gameObject.GetComponent<Rigidbody>();//our player's rigidbody
        initialJumpForce = playerObject.GetComponent<TpMovement>().GetJumpForce();//starting jump force, so we can reset
        initialSpeed = playerObject.GetComponent<TpMovement>().GetSpeed();//starting speed, so we can reset
    }

    public void PlayerJumped()
    {
        //this is so TpMovement can notify this script for when we jumped and disable our superjump
        if (superjumpEnabled == true)
        {
            usedPowerUp = false;
            superjumpEnabled = false;
            playerObject.GetComponent<TpMovement>().SetJumpForce(initialJumpForce);
            selectedPowerUp = powerUpList.None;
        }
    }

    public void OnPowerUp()
    {
        //in the future this should probably be part of a game manager, but for now it's on the player//
        //there should probably also be an observer to check when the player actually uses their boosted ablility and then resets
        //their stats and removes their powerup
        //for now, we'll just have it on a timer
        if (!usedPowerUp && selectedPowerUp != powerUpList.None)
        {
            usedPowerUp = true;
            currentPowerUpDuration = 0;

            if (selectedPowerUp == powerUpList.SuperJump)
            {
                superjumpEnabled = true;
                playerObject.GetComponent<TpMovement>().SetJumpForce(superjumpForce);
            }
            else if (selectedPowerUp == powerUpList.SlowFall)
            {
                slowfallEnabled = true;
            }
            else if (selectedPowerUp == powerUpList.Dash)
            {
                dashEnabled = true;
                playerObject.GetComponent<TpMovement>().SetSpeed(dashSpeed);
            }
            else if (selectedPowerUp == powerUpList.Bomb)
            {
                Instantiate(bombPrefab, transform.position, transform.rotation);
                usedPowerUp = false;
                selectedPowerUp = powerUpList.None;
            }

            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }
    }

    private void Update()
    {
        //this one goes in update instead of fixed update; causes freezes otherwise
        if (slowfallEnabled)
        {
            playerGrounded = playerObject.GetComponent<TpMovement>().GetIsGrounded();
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

        //only apply the force when the player is in the air
        if (!playerGrounded)
        {
            //only apply forec if the player is actually falling
            if (rb.velocity.y < maxFallSpeed)
            {
                rb.AddForce(Vector3.up * (rb.velocity.y * rb.velocity.y + maxFallSpeed) / 10 * slowfallForce); //apply force as a function of current downward velocity
            }
        }

        //if we've used a powerup, check each one's duration
        if (usedPowerUp)
        {
            if (selectedPowerUp == powerUpList.SlowFall && currentPowerUpDuration >= slowfallDuration)
            {
                slowfallEnabled = false;
                playerGrounded = true;
                currentPowerUpDuration = 0;
                usedPowerUp = false;
                selectedPowerUp = powerUpList.None; //"Consume" powerup when done

            }
            else if (selectedPowerUp == powerUpList.Dash && currentPowerUpDuration >= dashDuration)
            {
                dashEnabled = false;
                playerObject.GetComponent<TpMovement>().SetSpeed(initialSpeed);
                currentPowerUpDuration = 0;
                usedPowerUp = false;
                selectedPowerUp = powerUpList.None; //"Consume" powerup when done
            }

        }
    }
}
public enum powerUpList { None, SuperJump, SlowFall , Dash, Bomb}
