using UnityEngine;

using System.Collections;

public class CameraController : MonoBehaviour

{

    private Transform target;

    private Vector3 offset;

    private Vector3 originalLocalPosition;

    private bool isShaking = false;
    private Vector3 newPosition = Vector3.zero;

    void Start()

    {

        target = GameObject.FindGameObjectWithTag("Player").transform;

        offset = transform.position - target.position;

        originalLocalPosition = transform.localPosition;

    }

    void LateUpdate()

    {

        if (!isShaking)
        {
            newPosition.x = transform.position.x;
            newPosition.y = transform.position.y;
            newPosition.z = offset.z + target.position.z;
            transform.position = newPosition;
        }

    }

    public void Shake(float duration, float magnitude)

    {

        if (!isShaking)

            StartCoroutine(ShakeCoroutine(duration, magnitude));

    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)

    {

        isShaking = true;

        float elapsed = 0f;

        Vector3 basePosition = transform.position;

        while (elapsed < duration)

        {

            float percentComplete = elapsed / duration;

            float damper = 1.0f - Mathf.Clamp01(percentComplete * 1.5f);

            

            float x = Random.Range(-1f, 1f) * magnitude * damper;

            float y = Random.Range(-1f, 1f) * magnitude * damper;

            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, offset.z + target.position.z);

            transform.position = targetPos + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime;

            yield return null;

        }

        isShaking = false;

    }

}
