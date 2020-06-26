using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class CustomSceneManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField]
    private Camera arCamera; //Used for Raycasting

    [SerializeField]
    private TextMeshProUGUI text;

    [SerializeField]
    private GameObject InformationPanel; // Panel to display information

    [SerializeField]
    private Button panelCloseButton;

    [SerializeField]
    private Button dragButton;

    private bool dragging = false;

    private CustomBehaviour[] planets; // Storing GameObject for all planets

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    public static List<string> planetTags = new List<string>(){"Sun","Mars","Earth","Jupiter","Venus","Neptune",
    "Pluto","Saturn","Mercury","Uranus"};

    public static List<string> planetFields = new List<string>(){"Information","Type","Temp","Rotation","Atmospheric","Moons","Revolution","Diameter"};

    [SerializeField]
    private List<TextMeshProUGUI> fields; 

    void Awake()
    {   
        m_RaycastManager = GetComponent<ARRaycastManager>();

        // Add Onclick Listners to both the buttons
        panelCloseButton.onClick.AddListener(TogglePanel);
        dragButton.onClick.AddListener(ToggleDragging);
    }

    void Start() {
        InformationPanel.SetActive(false);
    }

    /*
     * Detects if a touch is over UI
     * Without this, a UI touch could register as a touch on a plane from AR Plane Manager
     */
    bool isOverUI(Vector2 touchPosition)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(touchPosition.x, touchPosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }


    void Update()
    {   

        if (Input.touchCount <= 0 || InformationPanel.activeSelf)
            return;
        else if (Input.touchCount > 1) {
            if (spawnedObject != null) {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);
                // Calculate previous position
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                float pinchAmount = deltaMagnitudeDiff * 0.02f * Time.deltaTime;
                spawnedObject.transform.localScale -= new Vector3(pinchAmount, pinchAmount, pinchAmount);
                // Clamp scale
                float Min = 0.005f;
                float Max = 3f;
                spawnedObject.transform.localScale = new Vector3(
                    Mathf.Clamp(spawnedObject.transform.localScale.x, Min, Max),
                    Mathf.Clamp(spawnedObject.transform.localScale.y, Min, Max),
                    Mathf.Clamp(spawnedObject.transform.localScale.z, Min, Max)
                );
            }
        }
        else if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if(isOverUI(touch.position)) return;
            if (touch.phase == TouchPhase.Began) //If first touch
            { 
                Ray ray = arCamera.ScreenPointToRay(touch.position); //If the touch hit something i.e. anything with collider (including Planes)
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject)) {
                    if (!dragging && planetTags.Contains(hitObject.transform.gameObject.tag)) //If a Planet was touched and Dragging was not active
                    {
                        findParent(hitObject.transform.gameObject.tag); // Find Parent of the touched object and populate Information Panel
                    }else if(m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon)) //If a plane was touched
                    {
                        var hitPose = s_Hits[0].pose;
                        if (spawnedObject == null)
                        {
                            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                        }
                    }
                }
            }

            if(touch.phase == TouchPhase.Moved && dragging) // If touch.phase is moved and Dragging is activated
            {
                // Move the spawned object with the touch.
                m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon);
                var hitPose = s_Hits[0].pose;
                spawnedObject.transform.position = hitPose.position;
            }
        }
    }

    /*
    * Whenever the Planet is touched, Information Panel is set active and all the
    * text fields are populated with the planet metadata.
    */
    void populatePanel(CustomBehaviour planet){
        InformationPanel.SetActive(true);

        string value = "";
        TextMeshProUGUI dataValue = InformationPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        dataValue.text = planet.data.name;
        for(int i = 0; i < planetFields.Count; i++){
            if(planet.data.planetData.TryGetValue(planetFields[i],out value)){
                fields[i].text = value;
            }
        }
        text.text = planet.tag;
        return;
    }

    /*
    * Function to Find Parents of the Touched GameObject and Invoke Populate Panel.
    */
    void findParent(string Tag){
        if(planets == null || planets.Length < 10){
            planets = GameObject.FindObjectsOfType<CustomBehaviour>();
        }
    
        foreach (CustomBehaviour planet in planets){
            if(planet.identifier.Equals(Tag)){
                populatePanel(planet);
                break;
            }
        }
        return;
    }

    /*
    * This toggle the dragging option. 
    * If inforamtion panel is active, then dragging can't be active
    */
    void ToggleDragging(){
        if(InformationPanel.activeSelf) return;
        if(dragging) {
            dragging = false;
        }else {
            dragging = true;
        }
    }

    // To Close Information Panel
    void TogglePanel(){
        InformationPanel.SetActive(false);
        text.text = "Click on the Planet to Get more information";
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
