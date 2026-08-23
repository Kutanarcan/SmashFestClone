using UnityEngine;

public class PlatformView : MonoBehaviour
{
    [SerializeField] private BoxCollider box;

    [SerializeField] private Transform visual;

    public void SetSize(Vector3 size)
    {
        if (box != null)
        {
            box.size = size;
            box.center = Vector3.zero;
        }

        if (visual != null) visual.localScale = size;
    }
}
