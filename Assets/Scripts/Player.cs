using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
    public float playerDeltaTime;

    //public Vector2 moveDir = new Vector2(0, 1);
    GameObject checkpoint;
    Animator anim;
    SpriteRenderer sprite;

    public bool isVariableJump = false;
    public int maxJump = 1;
    public int jumps = 0;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 1f;
    public float accTimeAir= 0.2f;
    public float accTimeGrounded = 0.1f;

    public float wallSpeedSlideMax = 3;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallStickTime = .25f;
    public float timeToWallUnstick;
    bool wallSliding;
    int wallDirX;

    //Calculated Values
    public Vector2 velocity;
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    float velocityXSmoothing;

    //Class Objects
    protected Controller2D controller;
    [HideInInspector]
    public Vector2 dirInput;
    public float joyAngle;
    public Vector2 telePos;

    // Use this for initialization
    void Start() {
        controller = GetComponent<Controller2D>();
        anim = GetComponent<Animator>();

        //Calculate gravity and max/min jump velocity
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update() {
        //Delta Time Adjustments
        playerDeltaTime = BattleController.Shared().playerDelta;

        CalculateVelocity();
        HandleWallSliding();

        //Pass movement vector to Controller2D
        controller.Move(velocity * playerDeltaTime, dirInput);

        //Animation Stuff
        anim.SetFloat("Speed", Mathf.Abs(velocity.x));
        
        //Reset Gravity if colliding with ground
        if(controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if(controller.collisions.below || controller.collisions.right || controller.collisions.left) {
            jumps = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input) {
        dirInput = input;
    }

    public void SetJoyAngle(float angle) {
        joyAngle = angle;
    }

    public void SetPosition(Vector2 pos) {
        transform.position = pos;
    }

    public void OnJumpInputDown() {
        dirInput = new Vector2(0, 0);
        
        if(wallSliding) {
            if(wallDirX == dirInput.x) { //Moving into wall
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            } else {
                if(dirInput.x == 0) {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                } else {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
        }

        if(jumps < 2) {
            jumps++;
            velocity.y = maxJumpVelocity;
        }
    }

    public void OnJumpInputUp() {
        if(velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    public void OnCircleInputDown() {
        //Execute move chosen
        if(BattleController.Shared().IsPaused()) {
            transform.position = GameObject.FindGameObjectWithTag("BlinkCursor").transform.position;
            velocity.x = 0;
            velocity.y = 0;

            BattleController.Shared().SwitchPause();
        }
    }

    public void OnCircleInputUp() {

    }

    void CalculateVelocity() {
        //Calculate target velocity
        float targetVelocityX = dirInput.x * moveSpeed;

        //Apply left/right movement
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            controller.collisions.below ? accTimeGrounded : accTimeAir);

        //Apply gravity to player
        velocity.y += gravity * playerDeltaTime;
    }

    void HandleWallSliding() {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if(controller.collisions.left || controller.collisions.right && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;
            if(velocity.y < -wallSpeedSlideMax) {
                velocity.y = -wallSpeedSlideMax;
            }

            if(timeToWallUnstick > 0) {
                velocityXSmoothing = 0;
                if(dirInput.x != wallDirX && dirInput.x != 0) {
                    timeToWallUnstick -= playerDeltaTime;
                } else {
                    timeToWallUnstick = wallStickTime;
                }
            } else {
                timeToWallUnstick = wallStickTime;
            }
        }
    }
}
