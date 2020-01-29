using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_OR_AIPaddle : MonoBehaviour
{
    void Update()
    {
        // If human is playing on either side then
        // move three spaces at a time when arrow key is pressed.
        //Detect when the up arrow key is pressed
        if (Input.GetKey("up"))
            this.transform.Translate(0, 0.3f, 0);
        //Detect when the down arrow key is pressed
        else if (Input.GetKey("down"))
            this.transform.Translate(0, -0.3f, 0);
    }
}