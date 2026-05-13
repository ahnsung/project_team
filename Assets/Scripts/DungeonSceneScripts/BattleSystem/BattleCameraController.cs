using System.Collections;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Impact Zoom")]
    public float impactZoomSize = 2.7f;

    [Header("Timing")]
    public float zoomInTime = 0.1f;
    public float holdTime = 0.08f;
    public float zoomOutTime = 0.18f;

    [Header("Shake")]
    public float shakeTime = 0.12f;
    public float shakePower = 0.12f;

    [Header("Focus Offset")]
    public float focusOffsetX = 2.0f;
    public float focusOffsetY = 0.3f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public IEnumerator AttackImpactZoom(Transform attacker, Transform target)
    {
        if (targetCamera == null)
            yield break;

        Vector3 startPos = targetCamera.transform.position;
        float startSize = targetCamera.orthographicSize;

        Vector3 focusPos = startPos;

        if (attacker != null && target != null)
        {
            Vector3 focusTarget = target.position;

            // 공격받는 대상 방향으로 카메라 치우침
            if (target.position.x > attacker.position.x)
                focusTarget.x += focusOffsetX;
            else
                focusTarget.x -= focusOffsetX;

            focusPos = new Vector3(
                focusTarget.x,
                focusTarget.y + focusOffsetY,
                startPos.z
            );
        }

        float t = 0f;

        // 줌인
        while (t < zoomInTime)
        {
            t += Time.deltaTime;

            float ratio = t / zoomInTime;

            targetCamera.orthographicSize =
                Mathf.Lerp(startSize, impactZoomSize, ratio);

            targetCamera.transform.position =
                Vector3.Lerp(startPos, focusPos, ratio);

            yield return null;
        }

        // 흔들림
        yield return StartCoroutine(Shake());

        // 잠깐 유지
        yield return new WaitForSeconds(holdTime);

        t = 0f;

        // 줌아웃
        while (t < zoomOutTime)
        {
            t += Time.deltaTime;

            float ratio = t / zoomOutTime;

            targetCamera.orthographicSize =
                Mathf.Lerp(impactZoomSize, startSize, ratio);

            targetCamera.transform.position =
                Vector3.Lerp(focusPos, startPos, ratio);

            yield return null;
        }

        targetCamera.orthographicSize = startSize;
        targetCamera.transform.position = startPos;
    }

    private IEnumerator Shake()
    {
        if (targetCamera == null)
            yield break;

        Vector3 startPos = targetCamera.transform.position;

        float t = 0f;

        while (t < shakeTime)
        {
            t += Time.deltaTime;

            float x = Random.Range(-shakePower, shakePower);
            float y = Random.Range(-shakePower, shakePower);

            targetCamera.transform.position =
                startPos + new Vector3(x, y, 0f);

            yield return null;
        }

        targetCamera.transform.position = startPos;
    }
}