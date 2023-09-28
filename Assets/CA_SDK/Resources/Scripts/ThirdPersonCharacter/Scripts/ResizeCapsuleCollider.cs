using UnityEditor.SceneManagement;
using UnityEngine;

public class ResizeCapsuleCollider : MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public Renderer characterRenderer;

    void Start()
    {
        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }
        else
        {
            this.gameObject.AddComponent<CapsuleCollider>();
        }

        if (characterRenderer == null)
        {
            characterRenderer = GetComponentInChildren<Renderer>();
        }

        AdjustCapsuleColliderSize();
    }

    void AdjustCapsuleColliderSize()
    {
        if (capsuleCollider && characterRenderer)
        {
            Bounds bounds = characterRenderer.bounds;

            capsuleCollider.height = bounds.size.y;
            capsuleCollider.radius = Mathf.Max(bounds.size.x, bounds.size.z) / 2f;
            capsuleCollider.center = new Vector3(0, bounds.extents.y, 0);
        }
    }
}