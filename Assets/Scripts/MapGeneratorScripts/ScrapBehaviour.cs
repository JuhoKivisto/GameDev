﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBehaviour : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Player")
        {
            // play the sound from the audio manager
            FindObjectOfType<AudioController>().play("Scrap");
            Destroy(gameObject);
            other.GetComponent<PlayerControllerMapTut>().gun.ammoBuffer++;
        }
    }
}