using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    public float boostForce;
    PlayerMovement player;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerMovement>();
            if (player != null && !player.isBoosted)
            {
                boostForce = 3f;
                player.maxSpeed += boostForce;
                player.isBoosted = true;
                Debug.Log("Boostpad activated: Boost applied!");
            }
            else
            {
                boostForce = -3f;
                Debug.Log("Boostpad Unactivated: Player already boosted.");
            }
        }
    }
}
