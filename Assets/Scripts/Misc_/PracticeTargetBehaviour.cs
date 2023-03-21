using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeTargetBehaviour : MonoBehaviour
{
    bool wasHit = false;
    float hitTimer = 0;
    [SerializeField] float downedTime = 3;
    float targetAngle = 90;
    float currentAngle;

    // Start is called before the first frame update
    void Start()
    {
        currentAngle = transform.eulerAngles.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (wasHit)
        {
            hitTimer += Time.fixedDeltaTime;

            if (hitTimer >= downedTime)
            {
                currentAngle = Mathf.LerpAngle(currentAngle, 0, (hitTimer - downedTime) / 1);
                gameObject.transform.eulerAngles = new Vector3(0, 0, currentAngle);

                if (gameObject.transform.eulerAngles.z <= 0.1)
                {
                    wasHit = false;
                    hitTimer = 0;
                    Debug.Log("Practice target reset");

                }
            }
            else
            {
                Debug.Log(currentAngle);
                currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, hitTimer / 1);
                gameObject.transform.eulerAngles = new Vector3(0, 0, currentAngle);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !wasHit)
        {
            wasHit = true;
            Debug.Log("Practice target hit");
        }
    }
}
