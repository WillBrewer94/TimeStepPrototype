using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : RaycastController {
    public bool facingRight;

    public float maxClimbAngle = 80f;
    public float maxDescendAngle = 75f;
    public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 playerInput;

    //Calls start method in RaycastController
    public override void Start() {
        base.Start();
        collisions.faceDir = 1;
    }

    public void Move(Vector2 deltaMove, bool standingOnPlatform) {
        Move(deltaMove, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 deltaMove, Vector2 input, bool standingOnPlatform = false) {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.deltaMoveOld = deltaMove;
        playerInput = input;

        if(deltaMove.y < 0) {
            DescendSlope(ref deltaMove);
        }

        if(deltaMove.x != 0) {
            collisions.faceDir = (int)Mathf.Sign(deltaMove.x);
        }

        HorizCollisions(ref deltaMove);        

        if(deltaMove.y != 0) {
            VertCollisions(ref deltaMove);
        }

        transform.Translate(deltaMove);

        if(standingOnPlatform) {
            collisions.below = true;
        }
    }

    void HorizCollisions(ref Vector2 deltaMove) {
        float dirX = collisions.faceDir;
        float rayLen = Mathf.Abs(deltaMove.x) + skinWidth;

        if(Mathf.Abs(deltaMove.x) < skinWidth) {
            rayLen = 2 * skinWidth;
        }

        for(int i = 0; i < horizRayCount; i++) {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLen, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * dirX, Color.red);

            if(hit) {
                if(hit.distance == 0) {
                    continue;
                }

                //Gets angle of incline
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                //Checks if slope is climbable
                if(i == 0 && slopeAngle <= maxClimbAngle) {
                    //If encountering another slope directly after descent
                    if(collisions.descendingSlope) {
                        //New slope encountered, set descent to false
                        collisions.descendingSlope = false;
                        deltaMove = collisions.deltaMoveOld;

                    }
                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collisions.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance = skinWidth;
                        deltaMove.x -= distanceToSlopeStart * dirX;
                    }

                    ClimbSlope(ref deltaMove, slopeAngle);
                    deltaMove.x += distanceToSlopeStart * dirX;
                }

                if(!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                    deltaMove.x = (hit.distance - skinWidth) * dirX;
                    rayLen = hit.distance;

                    //Recalculates vertical deltaMove if side collision on slope
                    if(collisions.climbingSlope) {
                        deltaMove.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad * Mathf.Abs(deltaMove.x));
                    }

                    collisions.left = dirX == -1;
                    collisions.right = dirX == 1;
                }
            }
        }
    }

    void VertCollisions(ref Vector2 deltaMove) {
        float dirY = Mathf.Sign(deltaMove.y);
        float rayLen = Mathf.Abs(deltaMove.y) + skinWidth;

        for(int i = 0; i < vertRayCount; i++) {
            Vector2 rayOrigin = (dirY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (vertRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLen, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * dirY, Color.red);

            if(hit) {
                //Goes up or through platform if possible
                if(hit.collider.tag == "Through") {
                    if(dirY == 1 || hit.distance == 0) {
                        continue;
                    }
                    if(playerInput.y == -1) {
                        continue;
                    }
                }

                deltaMove.y = (hit.distance - skinWidth) * dirY;
                rayLen = hit.distance;

                //Recalculates vertical deltaMove if above collision on slope
                if(collisions.climbingSlope) {
                    deltaMove.x = deltaMove.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad * Mathf.Sign(deltaMove.x));
                }

                collisions.below = dirY == -1;
                collisions.above = dirY == 1;
            }
        }

        if(collisions.climbingSlope) {
            float dirX = Mathf.Sign(deltaMove.x);
            rayLen = Mathf.Abs(deltaMove.x) + skinWidth;

            //Checks for change in slope 
            Vector2 rayOrigin = ((dirX == -1)
                ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * deltaMove.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLen, collisionMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                //Colliding with a new different slope
                if(slopeAngle != collisions.slopeAngle) {
                    deltaMove.x = (hit.distance - skinWidth) * dirX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 deltaMove, float slopeAngle) {
        float moveDistance = Mathf.Abs(deltaMove.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        //Checks if vertical deltaMove is lower than new climb deltaMove, 
        //allowing jumping on slopes
        if(deltaMove.y <= climbVelocityY) {
            deltaMove.y = climbVelocityY;
            deltaMove.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaMove.x);

            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        } 
    }

    void DescendSlope(ref Vector2 deltaMove) {
        float dirX = Mathf.Sign(deltaMove.x);

        Vector2 rayOrigin = (dirX == -1)
            ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if(hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                if(Mathf.Sign(hit.normal.x) == dirX) { //moving down slope
                    if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x)) { //Slope will affect character
                        float moveDistance = Mathf.Abs(deltaMove.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        deltaMove.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaMove.x);
                        deltaMove.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;

                    }
                }
            }
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizRayCount = Mathf.Clamp(horizRayCount, 2, int.MaxValue);
        vertRayCount = Mathf.Clamp(horizRayCount, 2, int.MaxValue);

        horizRaySpacing = bounds.size.y / (horizRayCount - 1);
        vertRaySpacing = bounds.size.x / (vertRayCount - 1);
    }

    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 deltaMoveOld;
        public int faceDir;

        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
