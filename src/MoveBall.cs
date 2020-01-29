using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class - "MoveBall"
public class MoveBall : MonoBehaviour
{
	// BallStartPosition - keeps track of where the ball is positioned on the screen
	// This is used for reseting the ball to its initial position at the start of the game
	Vector3 ballStartPosition;
	// Rigid Body 2D game object
	Rigidbody2D rb;
    // This is actually a force, not a speed for the ball, but it was named speed 
	// to visualize how fast the ball is moving.
	float speed = 400;
    // Audio files to add a bit more realism to the game
	public AudioSource blip;
	public AudioSource blop;

	// Method - Use this for initialization
	void Start ()
	{
		// Instantiate the rigid body
		rb = this.GetComponent<Rigidbody2D>();
		// Grab the initial position of the ball on the screen
		ballStartPosition = this.transform.position;
		// Put the ball in the starting position and push it away
		ResetBall();
	}
	
	// Method - add sounds when the ball hits the backwall (red wall)
	// if the ball hits anything other than the backwall, make a different
	// sound (i.e. blip).
	void OnCollisionEnter2D(Collision2D col)
    {
		if (col.gameObject.tag == "backwall")
			blop.Play();
		else
			blip.Play();
    }

	// Method - Reset the ball at the starting position in stationary condition
	public void ResetBall()
    {
		// Initial position
		this.transform.position = ballStartPosition;
		// Zero velocity
		rb.velocity = Vector3.zero;
		// Ball directions: first is X-direction, second is Y-direction, and Z-direction is set to 0
		// the y-direction is the angle at which the ball leaves
		// Finally, normalize the vector to unit vector
		// This UNIT vector is used to multiply by the speed to give the speed vector a direction
		Vector3 dir = new Vector3(Random.Range(100, 300), Random.Range(-100, 100), 0).normalized;
		// Multiply the speed by the UNIT vector defined above
		rb.AddForce(dir*speed);
    }


	// Method - Update is called once per frame
	void Update ()
	{
		// Mainly, allow control to start the game over and over again
		// and not let the ball get out of control or get stuck bouncing
		// around in the horizontal or vertical directions
		if(Input.GetKeyDown("space"))
        {
			ResetBall();
        }
		
	}
}
