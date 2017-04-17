using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Controller for battle scenes
public class BattleController : MonoBehaviour {
    //Singleton battle manager
    private static BattleController shared;

    //Configuration Values
    public float cursorSpeed = 10;

    //Components
    public LineRenderer lineRender;

    //Battle UI
    public Text turnText;
    public GameObject blinkCursor;
    public GameObject areaAttack;
    public GameObject lineAttack;

    //Characters
    public GameObject player;
    public GameObject enemy;

    //Delta time values for gameobjects
    public float playerDeltaTarget = 0;
    public float playerDelta = 0;
    public float enemyDelta;
    public float pauseSmoothTime = 0.5f;
    public float timer = 1;
    public float turnTime = 1;

    //Is Time Paused
    public bool isPause = false;

    void Awake() {
        //Make sure only one battle manager exists at a time
        if(shared == null) {
            shared = this;
        } else {
            Destroy(this.gameObject);
        }
    }

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        lineRender = GetComponent<LineRenderer>();
    }

    void Update() {
        if(isPause) {
            //Stop animations and player movement
            turnText.text = "Player Turn";
            player.GetComponent<Animator>().speed = 0;
            Mathf.SmoothDamp(playerDelta, playerDeltaTarget, ref playerDelta, pauseSmoothTime);

            //Move Blink Cursor
            blinkCursor.SetActive(true);
            blinkCursor.transform.Translate(player.GetComponent<Player>().dirInput * Time.deltaTime * cursorSpeed);

            //Draw Line
            lineRender.enabled = true;
            lineRender.SetPosition(0, player.GetComponent<BoxCollider2D>().bounds.center);
            lineRender.SetPosition(1, blinkCursor.GetComponent<BoxCollider2D>().bounds.center);

        } else {
            //Start animations and player movement
            playerDelta = Time.deltaTime;
            turnText.text = timer.ToString("F2");
            player.GetComponent<Animator>().speed = 1;

            //Run timer until 2 seconds have passed, then pause again
            Countdown();

            //Hide Line and Cursor
            lineRender.enabled = false;
            blinkCursor.SetActive(false);
        }
    }

    //Singleton Access Method
    public static BattleController Shared() {
        return shared;
    }

    //Getter method for isPaused
    public bool IsPaused() {
        return isPause;
    }

    //Switches pause state and runs relevant code
    public void SwitchPause() {
        isPause = !isPause;

        //Run logic for enemies
        GameObject[] enemies = GetEnemyList();

        foreach(GameObject enemy in enemies) {
            enemy.GetComponent<Enemy>().OnPause();
        }
    }

    //====================================================
    //                  Helper Methods                   =
    //====================================================

    //Countdown timer for x seconds, stored in the timer variable
    public void Countdown() {
        if(timer > 0) {
            timer -= Time.deltaTime;
        } else {
            isPause = true;
            timer = turnTime;
        }
    }

    //Finds all the active enemies on the scene
    public GameObject[] GetEnemyList() {
        return GameObject.FindGameObjectsWithTag("Enemy");
    }

    //Returns the angle of the line to be drawn by the LineRenderer
    public void Angle() {
        lineRender.enabled = true;
        lineRender.SetPosition(0, player.GetComponent<BoxCollider2D>().bounds.center);

        //Trig Shit
        float x = player.GetComponent<BoxCollider2D>().bounds.center.x + 8 * Mathf.Cos(player.GetComponent<Player>().joyAngle);
        float y = player.GetComponent<BoxCollider2D>().bounds.center.y + 8 * Mathf.Sin(player.GetComponent<Player>().joyAngle);

        lineRender.SetPosition(1, new Vector2(x, y));
    }

    //====================================================
    //                  Enemy Attacks                    =
    //====================================================
    
    //Run logic for area attack collisions, spawning, and despawning
    public void AreaAttack() {

    }

    //Run logic for line attack collisions, spawning, and despawning
    public void LineAttack() {

    }
}
