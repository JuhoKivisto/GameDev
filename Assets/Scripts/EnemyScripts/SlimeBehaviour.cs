﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// without proper knowledge about state machines and game ai in general i defien the ai as follows:
/* 
    - it has a set of bools that define each state
    - for each state we create a function that in depth describes the behaviour for said state
    - each state is sorted in a hierarchy in a if - else if - else statement,
        where the most important state is on top and least important is at the bottom
    - after excecuting the die method, this enemy will be Destroyed and deleted from its room
    - this is the basic slime ai hierarchy,
        it bleeds to death if at 1 health
        it dies if health >= 0,
        it attacks if it collides with the player and deals damage,
        it follows the player if he/she enters within a given radius,
        if none of these states are true: it's idle
I also propose that we make it possible for the enemy to have a set of sub states outside of the hierarchy
 e.g. this enemy will begin running away from the player if its health == 1 and the enemy will get a change in color.
    should the player collide with this slime the player will "eat" it up an recieve a slight boost in health or ammo
    if collision occurs within the bleemTime, or else the enemy will bleed out and die
*/ 
public class SlimeBehaviour : MonoBehaviour {

    // primary states
    //bool followPlayer = false, idle = false, die = false, attack = false, bleed;
    State followPlayer = new State("FollowPlayer");
    State idle = new State("idle");
    State die = new State("die");
    State attack = new State("attack");
    State bleed = new State("bleed");
    State previousState;

    public float followDistance, bleedTimer = 1.5f;
    
    private float bleedBuffer = 0;

    EnemyBehaviour basicBehaviour;
    Renderer renderer;

	// Use this for initialization
	void Start () {
        idle.active = true;
        previousState = idle;
        basicBehaviour = GetComponent<EnemyBehaviour>();
        renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        // primary states in hierarchy(bleed > die > attack > follow > idle)
        if (basicBehaviour.health == 1)
        {
            setStateFalse();
            bleed.active = true;
            previousState = bleed;
        }
        else if (basicBehaviour.health <= 0)
        {
            setStateFalse();
            die.active = true;
            previousState = die;
        }
        else if(basicBehaviour.collidingWithPlayer && basicBehaviour.health > 1)
        {
            setStateFalse();
            attack.active = true;
            previousState = attack;
        }
        else if (basicBehaviour.inSameRoomAsPlayer())
        {
            if(basicBehaviour.getDistanceFromPlayer() <= followDistance)
            {
                setStateFalse();
                
                if(previousState == idle) FindObjectOfType<AudioController>().play("EnemyCry"); // notice player from idle state
                followPlayer.active = true;
                previousState = followPlayer;
            }
        }
        else
        {
            setStateFalse();
            idle.active = true;
            previousState = idle;
        }

        manageStateMachine();
	}

    // Set every primary state to false
    void setStateFalse()
    {
        followPlayer.active = false; idle.active = false; die.active = false; attack.active = false; bleed.active = false;
    }

    // act according to the current state
    void manageStateMachine()
    {
        if (die.active) Destroy(gameObject);

        if (attack.active) basicBehaviour.player.takeDamage(basicBehaviour.damage);

        if(followPlayer.active)
        {
            moveTowardsPlayer();
        }
        if (bleed.active)
        {
            // set sprite color to blue
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            bleedBuffer += Time.deltaTime;
            if (bleedBuffer >= bleedTimer) basicBehaviour.health = 0;
            if (basicBehaviour.getDistanceFromPlayer() <= followDistance) moveAwayFromPlayer();
            if (basicBehaviour.collidingWithPlayer)
            {
                basicBehaviour.health = 0;
                basicBehaviour.player.heal(1);
            }
        }
    }

    // In all farness these should probably not be here. These methods are a biproduct of the admittedly poor colision detection I wrote
    // in desperation when the physics-engine started letting the player leave the rooms if it went fast enough.
    public void moveTowardsPlayer()
    {
        transform.Translate(basicBehaviour.getVelocity());
    }
    public void moveAwayFromPlayer()
    {
        basicBehaviour.inverseAxes();
        transform.Translate(basicBehaviour.getVelocity());
    }
}
