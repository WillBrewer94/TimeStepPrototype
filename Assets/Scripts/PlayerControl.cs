using UnityEngine;
using System.Collections;

//Allows player to move the character
public class PlayerControl : MonoBehaviour {
    private new Rigidbody2D rigidbody;
    private Vector2 move;
    private Animator anim;

    public float speed;
    public float maxSpeed;
    public float jumpForce;
    public bool facingRight;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        move = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update() {
        Vector2 input = GetLeftInput();
        move = Vector2.MoveTowards(move, input, Time.deltaTime);
        anim.SetFloat("Speed", Mathf.Abs(input.x));

        if(move.magnitude > 1) move.Normalize();

        if(rigidbody) {
            rigidbody.MovePosition(rigidbody.position + (input * Time.deltaTime) * speed);
        }

        if(Input.GetButtonDown("PS4_X")) {
            rigidbody.AddForce(Vector2.up * jumpForce * Time.deltaTime);
            Debug.Log("Jump");
        }

        
	}

    void PlayerInputs() {
        
    }

    //Gets directional input of the left analog stick
    protected Vector2 GetLeftInput() {
        return new Vector2(Input.GetAxis("PS4_LeftStickX"), 0);
    }
}
