using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;
    public const float skinWidth = .015f;
    public const float dstBetweenRays = .3f;
    [HideInInspector]
    public int horizRayCount;
    [HideInInspector]
    public int vertRayCount;

    [HideInInspector]
    public float horizRaySpacing;
    [HideInInspector]
    public float vertRaySpacing;

    [HideInInspector]
    public BoxCollider2D collider;
    public RaycastOrigins raycastOrigins;

    //Makes sure collider gets set ASAP
    public virtual void Awake() {
        collider = GetComponent<BoxCollider2D>();
    }

    public virtual void Start() {
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    protected void CalculateRaySpacing() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        vertRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        horizRaySpacing = bounds.size.y / (horizRayCount - 1);
        vertRaySpacing = bounds.size.x / (vertRayCount - 1);
    }

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
