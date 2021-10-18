using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
    public bool follow = true;

    // Target to follow.
    private Transform target = null;
    public Transform Target
    {
        get
        {
            return target;
        }

        set
        {
            if (target == null)
            {
                target = value;
                DampToTarget(500);
            }
            else
            {
                target = value;
            }
        }
    }
    public float height = 20.0f;
    public float smoothDampTime = 0.5f;

    // The point in at which the camera will be set in full view.
    [SerializeField] private Transform allMapViewPosition = null;

    private Vector3 smoothDampVel;
    [SerializeField] private bool thirdPersonMode = false;

    void LateUpdate()
    {
        if (!target || !follow)
            return;
        SmoothDampToTarget();
    }

    void DampToTarget(int fraction)
    {
        var targetPosition = target.position + Vector3.up * height;
        if (thirdPersonMode)
        {
            targetPosition = transform.TransformPoint(transform.InverseTransformPoint(targetPosition) - new Vector3(0, 0, 12));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, 1 * fraction);
        }
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothDampVel,
            smoothDampTime * fraction
        );
    }

    void SmoothDampToTarget()
    {
        DampToTarget(1);
    }

    public void GotoAllMapView()
    {
        if (allMapViewPosition == null)
            return;

        transform.position = allMapViewPosition.position;
    }
}
