using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlowerAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openingClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private float openingLength;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayCoroutine());
    }

    IEnumerator PlayCoroutine()
    {
        audioSource.loop = true;
        audioSource.clip = openingClip;
        audioSource.Play();
        yield return new WaitForSeconds(openingLength);

        audioSource.clip = loopClip;
        audioSource.Play();
    }
}
