/**************************************************************************
* Copyright (C) echoAR, Inc. 2018-2020.                                   *
* echoAR, Inc. proprietary and confidential.                              *
*                                                                         *
* Use subject to the terms of the Terms of Service available at           *
* https://www.echoar.xyz/terms, or another agreement                      *
* between echoAR, Inc. and you, your company or other organization.       *
***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CustomBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Entry entry;

    public Data data;

    float tilt;

    public string identifier;

    // Default Orbital Speed
    float OrbitalSpeed = 1f;
    private GameObject parent;
    /// <summary>
    /// EXAMPLE BEHAVIOUR
    /// Queries the database and names the object based on the result.
    /// </summary>


    public static List<string> planets = new List<string>(){"Sun.glb","Mars.glb","Earth.glb","Jupiter.glb","Venus.glb","Neptune.glb",
    "Pluto.glb","Saturn.glb","Mercury.glb","Uranus.glb"};

    // List of Meta Data Fields for the Planets
    public static List<string> planetFields = new List<string>(){"Information","Type","Temp","Rotation","Atmospheric","Moons","Revolution","Diameter"};


    // Use this for initialization

    /*
     * Attaches a MeshCollider to object and its children (recursive)
     * GameObject toAttach: the object that will receive the MeshCollider
     * bool convex: whether or not the MeshCollider should be convex
     */
    void recursivelyCollide(GameObject toAttach, bool convex, string tag)
    {
        if (toAttach.GetComponent<MeshCollider>() == null)
        {
            toAttach.AddComponent<MeshCollider>();
            toAttach.GetComponent<MeshCollider>().convex = convex;
            toAttach.tag = tag;
        }       

        foreach (Transform t in toAttach.transform)
        {
                recursivelyCollide(t.gameObject, convex,tag);
        }
    }


    void Start()
    {   
        // If the gameObject is Planet then destroy.
        if(!planets.Contains(this.gameObject.name)){
            Destroy(this.gameObject);
        }
        // Add RemoteTransformations script to object and set its entry
        this.gameObject.AddComponent<RemoteTransformations>().entry = entry;

        /*
        * Adds a dummy parent between this.gameObject and echoAR root object
        * i.e. 
        *         [echoAR]
        *            /
        *       [parent]
        *          /
        *  [this.gameObject]
        *    
        * It is difficult to apply transformations directly to the 3D model since the position/rotation are controlled by the echoAR console
        * We can easily apply transformations to the 3D model by applying the transformations to the dummy parent instead
        */
        this.parent = new GameObject();
        this.parent.transform.parent = this.gameObject.transform.parent;
        this.parent.name = this.gameObject.name;
        this.gameObject.transform.parent = parent.transform;
        this.gameObject.transform.localRotation = Quaternion.identity;

        /*
        * Use Data Struct to store all the metaData Information about the planets.
        */
        this.data = new Data();
        this.data.planetData = new Dictionary<string, string>();
        

        /*
        * Get All metadata information from EchoAR Console.
        */
        string value = "";
        if (entry.getAdditionalData() != null && entry.getAdditionalData().TryGetValue("Tag", out identifier))
        {
            this.parent.tag = identifier;
            this.data.name = identifier;
        }

        foreach(string dataKey in planetFields){
            
            if (entry.getAdditionalData() != null && entry.getAdditionalData().TryGetValue(dataKey, out value))
            {   
                this.data.planetData.Add(dataKey,value);
                this.data.planetData.TryGetValue(dataKey, out value);
            } 
        }

        /*
        * If it is a planet Attach Orbit and Spin component. 
        * This helps to rotatet and revolve planet around the sun.
        */
        if(!this.gameObject.name.Equals("Sun.glb")){
            this.parent.AddComponent<Orbit>();
            this.gameObject.AddComponent<Spin>();
        }
        
        /*
        * Query addition data:
        * Tilt: How much the planet is tilted towards the sun
        * Orbital Speed: It controls the revolution speed of the planet around the sun.
        */
        if (entry.getAdditionalData() != null && entry.getAdditionalData().TryGetValue("OrbitalSpeed", out value))
        {
            OrbitalSpeed = float.Parse(value, CultureInfo.InvariantCulture);
            this.parent.GetComponent<Orbit>().orbitalPeriod = OrbitalSpeed;
            
        }

        if (entry.getAdditionalData() != null && entry.getAdditionalData().TryGetValue("Tilt", out value))
        {
            tilt = float.Parse(value, CultureInfo.InvariantCulture);
            Debug.Log("this.gameObject.name" +   tilt);
            this.parent.transform.rotation = Quaternion.AngleAxis(tilt, Vector3.left);
        }

    }

    // Update is called once per frame
    void Update()
    {   
        // Attach MeshCollider and Tag to the Game Object.
        recursivelyCollide(this.parent,true,this.parent.tag);
    }
}