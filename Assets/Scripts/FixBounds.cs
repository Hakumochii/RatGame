using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FixBounds : MonoBehaviour
{
    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        if (mf != null && mf.mesh != null)
        {
            mf.mesh.bounds = new Bounds(
                Vector3.zero,
                Vector3.one * 15000f
            );
        }
    }
}
