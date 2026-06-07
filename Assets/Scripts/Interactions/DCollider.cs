using UnityEngine;

public class DCollider : MonoBehaviour
{
    public enum ForwardAxis { X, Z }

    [Header("Settings")]
    public ForwardAxis allowedAxis = ForwardAxis.Z;
    public float snapBackSpeed = 10f;

    private float lockedPosition;

    void Start()
    {
        lockedPosition = GetLockedAxisValue(transform.position);
    }

    void Update()
    {
        Vector3 pos = transform.position;
        float currentLocked = GetLockedAxisValue(pos);

        if (Mathf.Abs(currentLocked - lockedPosition) > 0.001f)
        {
            float corrected = Mathf.Lerp(currentLocked, lockedPosition, snapBackSpeed * Time.deltaTime);
            SetLockedAxisValue(ref pos, corrected);
            transform.position = pos;
        }
    }

    float GetLockedAxisValue(Vector3 v) => allowedAxis == ForwardAxis.Z ? v.x : v.z;
    void SetLockedAxisValue(ref Vector3 v, float value)
    {
        if (allowedAxis == ForwardAxis.Z) v.x = value;
        else v.z = value;
    }
}
