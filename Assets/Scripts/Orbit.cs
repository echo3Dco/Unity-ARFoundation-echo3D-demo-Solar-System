using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // Orbit Component helps to rotate Planet around the sun

    // Transform of the star: Sun
    private Transform aroundBody;

    // Default Orbital Period
    public float orbitalPeriod = 10f;

    /*
    * Game Time Per Day is how much game seconds is One day.
    * Default is 1 second per day
    */
    public float gametimePerDay = 1f;


    /*
    * Function to find Sun GameObject.
    */
    void findSun(){
        GameObject gameObject = GameObject.FindWithTag("Sun");
        if(gameObject != null){
            aroundBody = gameObject.transform;
            this.transform.position = aroundBody.position;
        }
    }
    void Start()
    {
       findSun();
    }

    // Update is called once per frame
    void Update()
    {   

        if(aroundBody == null){
            findSun();
            return;
        }

        /*
        * Calculate Angle of rotation per second based on planets orbital period.
        */
        float deltaAngle = (360 /(gametimePerDay*orbitalPeriod)) * Time.deltaTime;
       
        // Rotate Parent transform around the Sun.
        Vector3 axis = new Vector3 (0, 1, 0);
        this.transform.RotateAround(aroundBody.position,axis,deltaAngle); 
        
    }
}
