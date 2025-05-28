using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionAudio : MonoBehaviour
{
    // Prvni Audio Warning, jeho trigger je collider Audio objektu
    public AudioClip radioMissionWarning;
    [HideInInspector]
    public string playerTag = "Player";
    public static bool radioWarnPlayed = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = radioMissionWarning;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && radioWarnPlayed != true)
        {
            audioSource.Play();
            radioWarnPlayed = true;
        }
       
    }
}
