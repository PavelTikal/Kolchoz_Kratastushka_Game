using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionEnvScript : MonoBehaviour
{
    // Prubeh mise A konce
    public AudioClip firstKillAudio;
    public AudioClip lastKillAudio;
    public AudioClip playerIsHitByAIAudio;
    private AudioSource audioSource;
    [HideInInspector]
    private bool firstKillPlayed = false;
    public static bool lastKillPlayed = false;
    public static bool playerHitPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (VerbiszkyAI.deadAI == 1 && !firstKillPlayed && audioSource != null && firstKillAudio != null)
        {
            audioSource.clip = firstKillAudio;
            audioSource.Play();
            firstKillPlayed = true;
        }
        if (VerbiszkyAI.deadAI == VerbiszkyAI.totalAI && !lastKillPlayed && audioSource != null && lastKillAudio != null)
        {
            audioSource.clip = lastKillAudio;
            audioSource.Play();
            lastKillPlayed = true;
        }
        if (VerbiszkyAI.playerIsHitByAI && !playerHitPlayed && audioSource != null && playerIsHitByAIAudio != null)
        {
            audioSource.clip = playerIsHitByAIAudio;
            audioSource.Play();
            playerHitPlayed = true;
            StartCoroutine(StopGameAfterAudio(playerIsHitByAIAudio.length));
        }
    }
    private IEnumerator StopGameAfterAudio(float audioLength)
    {
        yield return new WaitForSeconds(audioLength);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a build
        Application.Quit();
#endif
    }
}
