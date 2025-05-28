using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialAudio : MonoBehaviour
{
    public AudioClip firstClip;
    public AudioClip secondClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (firstClip != null)
        {
            audioSource.clip = firstClip;
            audioSource.Play();
            StartCoroutine(PlayNextClipAfterCurrent(firstClip.length));
        }
    }

    private IEnumerator PlayNextClipAfterCurrent(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (secondClip != null)
        {
            audioSource.clip = secondClip;
            audioSource.Play();
        }
    }
}
