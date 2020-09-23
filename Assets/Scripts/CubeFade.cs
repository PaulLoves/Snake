using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFade : MonoBehaviour
{
    [SerializeField]
    float fadeInSpeed, fadeOutSpeed;

    float size = 0;

    MeshRenderer rendererComponent;

    private void Awake()
    {
        rendererComponent = GetComponent<MeshRenderer>();
    }

    public void FadeIn()
    {
        StopCoroutine(FadeOutRoutine());
        StartCoroutine(FadeInRoutine());
    }

    public void FadeOut()
    {
        StopCoroutine(FadeInRoutine());
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        rendererComponent.enabled = true;
        while(size < 1)
        {
            size += fadeInSpeed;
            transform.localScale = new Vector3(size, size, size);
            //Clamp vector components due to ocasional floating-point values imprecision and consequent negative scaling.
            transform.localScale = Utils.ClampComponents(transform.localScale, 0, 1);
            yield return new WaitForSeconds(1 / 24);
        }
        yield return null;
    }

    IEnumerator FadeOutRoutine()
    {
        while (size > 0)
        {
            size -= fadeOutSpeed;
            transform.localScale = new Vector3(size, size, size);
            //Clamp vector components due to ocasional floating-point values imprecision and consequent negative scaling.
            transform.localScale = Utils.ClampComponents(transform.localScale, 0, 1);
            yield return new WaitForSeconds(1 / 24);
        }
        rendererComponent.enabled = false;
        yield return null;
    }

    
}
