using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Class - Brain - this call the ANN class to actually create, train and use the neural network
public class Brain : MonoBehaviour {
    // The paddle (keep track of the paddle)
    public GameObject paddle;
    // The ball OBJECT (Graphics) (keep track of the position of the ball)
    public GameObject ball;
    public bool human = false;
    public string backwallTag = "backwallr";
    public Text score;

    // The ball RIGIDBODY (Kinematics) to know where the ball is at all times
    // Position, Direction and Speed
    Rigidbody2D brb;
    // y-velocity = the output of the Neural Network
    float yvel;
    // Upper and Lower limits of the paddle travel (stay inside the court)
    float paddleMinY = 8.8f;
    float paddleMaxY = 17.4f;

    // How fast is the paddle is allowed to move
    float paddleMaxSpeed = 15;

    // Keep track of the number of balls actually hit
    public float numSaved = 0;

    // Keep track of the number of balls missed
    public float numMissed = 0;


    // IMPORTANT PARAMETERS FOR THE NEURAL NETWORK
    // --------------------------------------------
    // 
    // INPUTS
    // ------
    // Given the known input data, in order for the neural network to function,
    // the following inputs make sense:
    //
    // Ball x position
    // Ball y position
    // Ball x velocity
    // Ball y velocity
    // Paddle x position
    // Paddle y position
    //
    // OUTPUTS
    // -------
    //
    // What do we need the neural network to calculate?
    // 
    // The paddle position and velocity at a future time;
    // Hence, we need to know the paddle y velocity to
    // position the paddle at the correct location at
    // a future time.

    // So, the neural network can now be created/instantiated

    ANN ann;

    // Start is called before the first frame update
    void Start()
    {
        // Constructor
        // Rule of thumb: A good starting number of neurons is a number between zero and the number of inputs.
        // Six inputs, one output, one hidden layer, four neurons, learning rate
        ann = new ANN(6, 1, 1, 4, 0.05);  // 0.11 learning rate worked well
        // ann = new ANN(6, 1, 1, 4, 0.001);  // 0.001 the ANN with this learning rate performed very poorly

        // Now, capture the rigid body on the ball to get the ball speed to feed to the neural network
        brb = ball.GetComponent<Rigidbody2D>();
    }

    // METHOD - This method either does training or does calculations without affecting the training
    List<double> Run(double bx, double by, double bvx, double bvy, double px, double py, double pv, bool train)
    {
        // Six inputs and One Output
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();
        inputs.Add(bx);  // Ball x position
        inputs.Add(by);  // Ball y position
        inputs.Add(bvx); // Ball x velocity
        inputs.Add(bvy); // Ball y velocity
        inputs.Add(px);  // Paddle x position
        inputs.Add(py);  // Paddle y position
        outputs.Add(pv); // Paddle velocity, this is ignored when we are calculating and not training
        // If training is selected, then go ahead and perform the training
        if(train)
            return (ann.Train(inputs,outputs));
        else
        // Otherwise, only calculate the output without affecting the training
            return (ann.CalcOutput(inputs,outputs));
    }


    // METHOD - This performs all the actions for the paddle
    // Update is called once per frame
    void Update()    {
       if(!human)
       {
            // Need to figure out the y-position for the paddle
            // Also clamp to max and min values
            //
            // IMPORTANT
            // ---------
            //
            // Formula for calculating the y-position for the paddle:
            // posy = (current y-position)*(y-velocity*Time.deltaTime*paddleMaxSpeed);
            // The Time.deltaTime multiplication is supposed to smoothout the resulting value
            // The paddleMaxSpeed was defined above.
            // The yvelocity is used in the multiplication because we are using TanH activation functions
            // which give smooth values between -1 and 1; consequently, the yvelocity will have smooth 
            // positive and negative values; for example, if the network returns a value of 0.5, then this
            // value is multiplied by the paddleMaxSpeed; this works as a proportional control for the speed.
            // This can also be viewed as a form of normalization on the y-velocity to prevent the values
            // from blowing out.
            // float posy = Mathf.Clamp(paddle.transform.position.y + (yvel * Time.deltaTime * paddleMaxSpeed), 8.8f, 17.4f);
            float posy = Mathf.Clamp(paddle.transform.position.y + (yvel * Time.deltaTime * paddleMaxSpeed), paddleMinY, paddleMaxY);
            // Notice that 'posy' is the only variable to essentially move the paddle up/down
            paddle.transform.position = new Vector3(paddle.transform.position.x, posy, paddle.transform.position.z);
            // Output values list to feed to the neural network
            List<double> output = new List<double>();
            // Use a raycast on the ball, so we can use this to predict where the ball is going to hit
            // We only want the raycast to detect when it hits a specific layer; in this case
            // the layer is 'layerMask' as defined below:
            int layerMask = 1 << 9;  // Pick out the backwall of the court; this statement picks layer 9 (bitwise leftshift operator)
            RaycastHit2D hit = Physics2D.Raycast(ball.transform.position, brb.velocity, 1000, layerMask);

            // ERROR CALCULATION
            // -----------------
            //
            // The error is calculated as the difference between where the paddle should have been (as predicted by
            // the raycast) AND the where the paddle actually ended up.  This is what gets backpropagated through the ANN
            // CHECK IF WE HIT ANYTHING
            // ------------------------
            // --------------------------------
            // Added the following code for additional intelligence
            if (hit.collider != null)
            {
                // (1)
                // IMPACT TO TOP OR BOTTOM SURFACES
                // This basically is used to determine the incidence and departure angles of the ball as it hits either the 
                // top or bottom surfaces (both tagged as "tops").
                if (hit.collider.gameObject.tag == "tops")    // ball reflecting off the top or bottom surfaces
                {
                    // This is the reflection vector: calculate the angle between the impact angle and the normal to the surface
                    Vector3 reflection = Vector3.Reflect(brb.velocity, hit.normal);
                    // Now perform another raycast for the reflection
                    hit = Physics2D.Raycast(hit.point, reflection, 1000, layerMask);
                }

                // (2)
                // IMPACT TO either BACKWALL
                // ------------------
                // 
                if (hit.collider != null && hit.collider.gameObject.tag == backwallTag)
                //if (hit.collider != null)
                {
                    // Delta-y value between the actual value were the ball hit and the paddle location (i.e. the ERROR)
                    // This is also a velocity because it is the change from position to another position (change in position vs. time)
                    float dy = (hit.point.y - paddle.transform.position.y);

                    output = Run(ball.transform.position.x,
                                 ball.transform.position.y,
                                 brb.velocity.x,
                                 brb.velocity.y,
                                 paddle.transform.position.x,
                                 paddle.transform.position.y,
                                 dy,
                                 true);
                    //  True = do training: example of ANN getting better over time because it is training and running at the same time

                    // Set the y-velociy to the output
                    yvel = (float) output[0];
                }
            }
            else
                // This is for the case when the ball is not going to hit the back wall, then do not move the paddle
                yvel = 0;
        }
       // Accumulate the score
        score.text = numMissed + "";
    }
}
