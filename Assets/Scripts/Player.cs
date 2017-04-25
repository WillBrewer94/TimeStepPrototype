using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
    public float playerDeltaTime;

    //Components
    GameObject checkpoint;
    Animator anim;
    SpriteRenderer sprite;

    //Platforming Config Values
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
    public float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    float velocityXSmoothing;

    //Class Objects
    protected Controller2D controller;
    [HideInInspector]
    public Vector2 dirInput;
    public float joyAngle;
    public Vector2 telePos;

    //Player Variables
    public bool isCollide;
    public int health = 3;
    public string[] attacks = { "Blink", "Slam" };
    public string currAttack;

    // Use this for initialization
    void Start() {
        controller = GetComponent<Controller2D>();
        anim = GetComponent<Animator>();

        //Calculate gravity and max/min jump velocity
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        currAttack = attacks[0];
    }

    void Update() {
        //Delta Time Adjustments
        playerDeltaTime = BattleController.Shared().playerDelta;
        //playerDeltaTime = Time.deltaTime;

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

            if(currAttack.Equals("Blink")) {
                Blink();
            } else if(currAttack.Equals("Slam")) {
                Slam();
            }

            BattleController.Shared().SwitchPause();
        }
    }

    public void OnCircleInputUp() {

    }

    public void OnSquareInputDown() {
        ToggleAttack();
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

    public void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "Attack") {
            isCollide = true;
            Debug.Log("Enter");
        }
    }

    public void OnTriggerExit2D(Collider2D collision) {
        isCollide = false;
        Debug.Log("Exit");
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        if(collision.collider.tag == "Enemy") {
            SetHealth(health - 1);
        }
    }

    public void SetHealth(int health) {
        this.health = health;

        if(health < 1) {
            SceneManager.LoadScene(0);
        }
    }

    public int GetHealth() {
        return health;
    }

    public void ToggleGravity() {
        if(gravity == 0) {
            gravity = -81;
        } else {
            gravity = 0;
        }
    }

    //====================================================
    //                  Player Attacks                   =
    //====================================================

    public void Blink() {
        transform.position = GameObject.FindGameObjectWithTag("BlinkCursor").transform.position;
        velocity.x = 0;
        velocity.y = 0;
    }

    public void Slam() {
        GameObject[] enemies = BattleController.Shared().GetEnemyList();

        foreach(GameObject enemy in enemies) {
            if(enemy.GetComponent<Enemy>().isCollide) {
                //Get direction of enemy from player
                enemy.GetComponent<Rigidbody2D>().AddForce(enemy.transform.position - transform.position * 10 * Time.deltaTime);

                Enemy enemyScript = enemy.GetComponent<Enemy>();
                enemyScript.SetHealth(enemyScript.GetHealth() - 1);
                enemyScript.isMoving = false;

                enemy.transform.position = new Vector2(2, 0);
            }
        }
    }

    public void ToggleAttack() {
        if(currAttack.Equals("Blink")) {
            currAttack = "Slam";
        } else if(currAttack.Equals("Slam")) {
            currAttack = "Blink";
        }
    }

    public string GetCurrAttack() {
        return currAttack;
    }
}
