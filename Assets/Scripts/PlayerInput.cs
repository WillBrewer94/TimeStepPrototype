using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {
    Player player;
    Animator anim;
    bool isPause = true;

    void Start() {
        player = GetComponent<Player>();
        anim = GetComponentInChildren<Animator>();
	}
	
	void Update() {
        Vector2 dirInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        player.SetDirectionalInput(dirInput);
        player.SetJoyAngle(JoyAngle());

        if(Input.GetButtonDown("X")) {
            player.OnJumpInputDown();
        }

        if(Input.GetButtonUp("X")) {
            player.OnJumpInputUp();
        }

        if(Input.GetButtonDown("Circle") || Input.GetKeyDown(KeyCode.Space)) {
            player.OnCircleInputDown();
        }

        if(Input.GetKeyDown(KeyCode.LeftShift)) {

        }
    }

    //Returns a vector direction of the right joystick
    float JoyAngle() {
        //X, Y, and angle of right joystick
        float joyX = Input.GetAxis("LeftStick_X");
        float joyY = Input.GetAxis("LeftStick_Y");

        //Gets computes angle from origin of right joystick location
        float joyAngle = Mathf.Atan2(joyY, joyX);

        return joyAngle;
    }
}
