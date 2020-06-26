using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    // Component to Spin Planet around it's axis (Rotation).

     /*
    * Game Time Per Day is how much game seconds is One day.
    * Default is 1 second per day
    */
    public float gametimePerday = 1.0f;

    // Update is called once per frame
    void Update()
    {   
        // Calculate Angle for rotation and rotate the planet.
       float deltaAngle = (360f/gametimePerday) * Time.deltaTime;
       this.transform.Rotate(0f,deltaAngle,0f); 
    }
}
