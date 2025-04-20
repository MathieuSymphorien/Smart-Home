using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    public Light targetLight; // Référence à la lumière à allumer

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assurez-vous que le joueur a le tag "Player"
        {
            if (targetLight != null)
            {
                if(targetLight.enabled == false){
                    targetLight.enabled = true;
                }
                else{
                    targetLight.enabled = false;
                }
            }
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         if (targetLight != null)
    //         {
    //             targetLight.enabled = false;
    //         }
    //     }
    // }
}
