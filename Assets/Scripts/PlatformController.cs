using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {
    public LayerMask passengerMask;
    public Vector2 move;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start() {
        base.Start();
    }

    void Update() {
        UpdateRaycastOrigins();

        Vector2 velocity = move * Time.deltaTime;
        CalculatePassengersMove(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach(PassengerMovement passenger in passengerMovement) {
            if(!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            if(passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengersMove(Vector2 velocity) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float dirX = Mathf.Sin(velocity.x);
        float dirY = Mathf.Sin(velocity.x);

        //Vertically moving platform
        if(velocity.y != 0) {
            float rayLen = Mathf.Abs(velocity.y) + skinWidth;

            for(int i = 0; i < vertRayCount; i++) {
                Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (vertRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLen, passengerMask);

                if(hit && hit.distance != 0) {
                    if(!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = (dirY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * dirY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), dirY == 1, true));
                    } 
                }
            }
        }

        //Horizontally moving platform
        if(velocity.x != 0) {
            float rayLen = Mathf.Abs(velocity.x) + skinWidth;

            for(int i = 0; i < horizRayCount; i++) {
                Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLen, passengerMask);

                if(hit && hit.distance != 0) {
                    if(!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * dirX; ;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), false, true));
                    }
                }
            }
        }

        //Passenger on top of a horizontally or downward moving platform
        if(dirY == -1 || velocity.y == 0 && velocity.x != 0) {
            float rayLen = skinWidth * 2;

            for(int i = 0; i < vertRayCount; i++) {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (vertRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLen, passengerMask);

                if(hit && hit.distance != 0) {
                    if(!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                    }
                }
            }

        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector2 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector2 _velocity, bool _stanidngOnPlatform, bool _moveBeforePlatform) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _stanidngOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
}
