using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {
    public float radius = 5;

    public List<Action> moves = new List<Action>();

	// Use this for initialization
	void Start () {
        moves.Add(() => AreaAttack(radius));
        moves.Add(() => LineAttack());
	}

    public void OnPause() {
        //Choose and execute move
        moves[UnityEngine.Random.Range(0, moves.Count)].Invoke();
    }

    //====================================================
    //                  Enemy Methods                    =
    //====================================================

    //Fires an area of effect attack of radius r
    public void AreaAttack(float r) {
        Debug.Log("AreaAttack");
    }

    //Chooses a direction close to the player and fires a beam
    public void LineAttack() {
        Debug.Log("LineAttack");
    }
}
