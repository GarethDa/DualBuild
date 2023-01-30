using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    public powerUpList type;
    public bool hasClicked = false;

    public virtual void onPickup()//when the player picks it up (add to inv and set transparency)
    {
        UIManager.instance.setPowerUpIconImageByPowerUpType(type, 0.5f);

    }
    public abstract void onUse();//when the player presses the use key (change transparency for some)

    public abstract void onEffect();//when the powerups action actually takes place (this is where the action code happens) (change transparency for some)

    public virtual void onEnd()//cleanup
    {
        UIManager.instance.getOnScreenPowerUpIcon().queueFadeTransparency(0, 0.5f);
        EventManager.onPlayerUsePowerup?.Invoke(null, System.EventArgs.Empty);
        Destroy(this);
    }

}

public class Bomb : PowerUp
{
    private GameObject bombPrefab;
    GameObject playerObject;

    public void setup(GameObject prefab, GameObject player)
    {
        bombPrefab = prefab;
        playerObject = player;
        type = powerUpList.Bomb; 

    }
    public override void onEffect()
    {
        onEnd();
    }

    public override void onUse()
    {
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        GameObject bomb = Instantiate(bombPrefab, transform.position, transform.rotation);
        bomb.GetComponent<BombBehaviour>().setPlayer(playerObject);
        onEffect();
    }
}

public class Dash : PowerUp
{
    private float initialSpeed; //grabbed value at start
    private float dashSpeed = 30.0f;
    float duration = 3f;
    float timeElapsed = 0f;
    bool used = false;
    GameObject playerObject;

    public void setup(float initial, float dash, GameObject player)
    {
        playerObject = player;
        initialSpeed = initial;
        dashSpeed = dash;
        type = powerUpList.Dash;
    }
    public override void onEffect()
    {
        //throw new System.NotImplementedException();
        UIManager.instance.getOnScreenPowerUpIcon().queueFadeTransparency(0, duration);
    }

    public override void onUse()
    {
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        playerObject.GetComponent<TpMovement>().SetSpeed(dashSpeed);
        used = true;
        onEffect();
    }

    public void Update()
    {
        if (!used)
        {
            return;
        }
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= duration)
        {
            onEnd();
        }
    }

    public override void onEnd()
    {
        used = false;
        playerObject.GetComponent<TpMovement>().SetSpeed(initialSpeed);
        EventManager.onPlayerUsePowerup?.Invoke(null, System.EventArgs.Empty);
        Destroy(this);
    }

}

public class SlowFall : PowerUp
{
     float slowfallForce = 10.0f;
     float maxFallSpeed = -4.0f;
    float duration = 3f;
    float currentElapsedTime = 0f;
    bool usedPowerUp = false;
    bool playerGrounded = false;
    GameObject playerObject;
    Rigidbody rb;

    public void setup(float fallForce, float fallSpeed, GameObject player)
    {
        slowfallForce = fallForce;
        maxFallSpeed = fallSpeed;
        type = powerUpList.SlowFall;
        playerObject = player;
        rb = player.GetComponent<Rigidbody>();
    }
    public override void onEffect()
    {
      
       //nothing cause everything gets handled in fixed update
    }



    public override void onUse()
    {
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        usedPowerUp = true;
        UIManager.instance.getOnScreenPowerUpIcon().queueFadeTransparency(0, duration);
    }

    private void FixedUpdate()
    {
        //constantly adds up INGAME time, in case we want to use slow motion or something
        if (!hasClicked)
        {
            return;
        }
        
        if (usedPowerUp)
        {
            currentElapsedTime += Time.fixedDeltaTime;
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
    }
    private void Update()
    {
        if (!hasClicked)
        {
            return;
        }
        if (!usedPowerUp)
        {
            return;
        }
       // currentElapsedTime += Time.deltaTime;
        playerGrounded = playerObject.GetComponent<TpMovement>().GetIsGrounded();
        if(currentElapsedTime >= duration)
        {
            onEnd();
        }
    }

    public override void onEnd()
    {
        usedPowerUp = false;
        
        EventManager.onPlayerUsePowerup?.Invoke(null, System.EventArgs.Empty);
        Destroy(this);

    }
}

public class SuperJump : PowerUp
{

    float superjumpForce = 0f;
    float normalJumpForce = 0f;
    GameObject playerObject;

    public void setup(float superForce, float normalForce, GameObject player)
    {
        superjumpForce = superForce;
        normalJumpForce = normalForce;
        playerObject = player;
        type = powerUpList.SuperJump;
    }
    public override void onEffect()
    {
        playerObject.GetComponent<TpMovement>().SetJumpForce(normalJumpForce);
        onEnd();
    }


    public override void onUse()
    {
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        playerObject.GetComponent<TpMovement>().SetJumpForce(superjumpForce);
        
    }

    void Update()
    {
        if (hasClicked)
        {
            bool grounded = playerObject.GetComponent<TpMovement>().GetIsGrounded();
            if (!grounded)
            {
                onEffect();
            }
        }
    }


}
