using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
    public bool autoAttach;
    public Vector3 cameraRotation;
    [Range(0f, 1f)]
    public float zoomLevel;

    /// <summary>
    /// First part of the gameobject's name
    /// </summary>
    public string PointName => gameObject.name.Split(' ')[0];
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    private Vector3 _initialPosition;
    private CameraController controller;

    private void Start()
    {
        controller ??= FindFirstObjectByType<CameraController>();
    }

    public void Attach()
    {
        controller ??= FindFirstObjectByType<CameraController>();
        if (controller == null)
            return;

        _initialPosition = transform.localPosition;
        controller.FollowTarget = this;
        //Debug.Log($"Attached {gameObject.name}");
    }

    public void Detach()
    {
        controller ??= FindFirstObjectByType<CameraController>();
        if (controller == null || controller.FollowTarget != this)
            return;

        controller.FollowTarget = null;
        ResetTransform();
        //Debug.Log($"Detached {gameObject.name}");
    }

    public void ResetTransform()
    {
        transform.localPosition = _initialPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 forwardDirection = Quaternion.Euler(cameraRotation) * Vector3.forward;
        Vector3 camPos = transform.position - forwardDirection * Mathf.Lerp(10f, .1f, zoomLevel);
        Gizmos.DrawLine(transform.position, camPos);
        Gizmos.DrawIcon(camPos, "Projector Gizmo");
    }
}
