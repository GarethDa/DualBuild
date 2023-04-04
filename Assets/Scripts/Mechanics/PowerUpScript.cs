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

    //SuperPunch Variables
    [SerializeField] [Range(1.0f, 100.0f)] private float superPunchForce = 30f;

    private bool usedPowerUp = false; //if we have used our powerup
    public float powerUpDuration = 3f; //total duration of ability in seconds
    private float currentPowerUpDuration = 0f; //internal clock for powerup duration

    
   

    PowerUp currentPowerUp;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = gameObject;//the player this script is attached to
        rb = gameObject.GetComponent<Rigidbody>();//our player's rigidbody
        initialJumpForce = playerObject.GetComponent<TpMovement>().GetJumpForce();//starting jump force, so we can reset
        initialSpeed = playerObject.GetComponent<TpMovement>().GetSpeed();//starting speed, so we can reset
    }

    public powerUpList getCurrentPowerUp()
    {
        if(currentPowerUp == null)
        {
            return powerUpList.None;
        }
        return currentPowerUp.type;

    }

    public void clearPowerUp()
    {
        currentPowerUp = null;
    }

    public void PlayerJumped()
    {
       if(currentPowerUp is SuperJump && currentPowerUp.hasClicked)
        {
            currentPowerUp.onEffect();          
        }
    }

    public void PlayerPunched()
    {
        if (currentPowerUp is SuperPunch && currentPowerUp.hasClicked)
        {
            currentPowerUp.onEffect();
        }
    }

    public void setSelectedPowerUp(powerUpList powerUp)
    {
        addPowerUp(powerUp);
    }

    public powerUpList getSelectedPowerUp()
    {
        if(currentPowerUp == null)
        {
            return powerUpList.None;
        }
        return currentPowerUp.type;
    }

    public void OnPowerUp()
    {
        //in the future this should probably be part of a game manager, but for now it's on the player//
        //there should probably also be an observer to check when the player actually uses their boosted ablility and then resets
        //their stats and removes their powerup
        //for now, we'll just have it on a timer
        if (currentPowerUp != null)
        {
            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
            currentPowerUp.onUse();
        }
    }

    public void addPowerUp(powerUpList t)
    {
        //Destroy(playerObject.GetComponent<PowerUp>());
        if(t == powerUpList.Bomb)
        {
            playerObject.AddComponent<Bomb>().setup(bombPrefab, playerObject);
           
        }
        if (t == powerUpList.Dash)
        {
            playerObject.AddComponent<Dash>().setup(initialSpeed,dashSpeed,playerObject);
            
        }
        if (t == powerUpList.SuperJump)
        {
            playerObject.AddComponent<SuperJump>().setup(superjumpForce,initialJumpForce,playerObject);
          
        }
        if (t == powerUpList.SlowFall)
        {
            
            playerObject.AddComponent<SlowFall>().setup(slowfallForce, maxFallSpeed, playerObject);
            
            
        }
        if (t == powerUpList.SuperPunch)
        {
            playerObject.AddComponent<SuperPunch>().setup(superPunchForce, GetComponent<CharacterAiming>().GetPunchForce(), playerObject);
            Debug.Log("Setup assigned super force as " + superPunchForce);
        }
        currentPowerUp = playerObject.GetComponent<PowerUp>();
        
        currentPowerUp.onPickup();
    }



}
public enum powerUpList { None, SuperJump, SlowFall , Dash, Bomb, SuperPunch}
