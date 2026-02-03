using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static System.Net.Mime.MediaTypeNames;

[System.Serializable]
public class ImageButtonData
{
    public Sprite Image;
    public Button Button;

    public void SetButton(Button newButton)
    {
        Button = newButton;
    }
}

[System.Serializable]
public class ObjectForPlacementData
{
    public ImageButtonData ImageButtonData;
    public GameObject Object;
}

    public class SandSackPlacementController : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The reference to the action of drawing with this controller.")]
    InputActionReference m_Draw;

    [SerializeField]
    [Tooltip("If true, drawing will be enabled.")]
    bool m_DrawingEnabled = true;


    [SerializeField]
    [Tooltip("Object to place after drawing.")]
    GameObject PatternPlane;
    [SerializeField]
    [Tooltip("Controller Input Action Manager.")]
    ControllerInputActionManager ControllerInputManager;
    [SerializeField]
    [Tooltip("Line renderer.")]
    LineRenderer LineRenderer;

    Vector3 drawingStarts;
    bool bIsDrawingStarted = false;
    bool bIsDrawingEnded = false;

    // UI
    [SerializeField] private GameObject SecondStep;//for pattern
    [SerializeField] private GameObject ThirdStep;//for object
    [SerializeField] private Button PatternButtonTemplate;
    [SerializeField] private Button ObjectForPlacementButtonTemplate;
    // Array of possible patterns 
    [SerializeField] private List<ImageButtonData> PatternsDataBase = new List<ImageButtonData>();
    // Array of possible objects
    [SerializeField] private List<ObjectForPlacementData> ObjectsForPlacementDataBase = new List<ObjectForPlacementData>();

    [SerializeField] private Material PatternMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // check that not null, if null - write an error in the log
        // Drawing Setup
        var drawAction = ControllerInputActionManager.GetInputAction(m_Draw);
        if (drawAction != null)
        {
            drawAction.started += OnStartDrawing;
            drawAction.canceled += OnStopDrawing;
        }
        /*
            var drawAction = GetInputAction(m_Draw);
            if (drawAction != null)
            {
                drawAction.started -= OnStartDrawing;
                drawAction.canceled -= OnStopDrawing;
            }
         */

        // Patterns setup
        foreach (var pattern in PatternsDataBase)
        {
            pattern.SetButton(Instantiate(PatternButtonTemplate, PatternButtonTemplate.transform.parent));
            pattern.Button.image.sprite = pattern.Image;
            pattern.Button.gameObject.SetActive(true);
            pattern.Button.onClick.AddListener(()=>
            {
                OnPatternButtonSelected(pattern);
            });
        }

        foreach (var objectData in ObjectsForPlacementDataBase)
        {
            Debug.Log("Hi");
            objectData.ImageButtonData.SetButton(Instantiate(ObjectForPlacementButtonTemplate, ObjectForPlacementButtonTemplate.transform.parent));
            objectData.ImageButtonData.Button.image.sprite = objectData.ImageButtonData.Image;
            objectData.ImageButtonData.Button.gameObject.SetActive(true);
        }
    }

    void OnPatternButtonSelected(ImageButtonData imageButtonData)
    {
        // choose pattern - activates plane (slowly)
        // pattern image should be along the line - to test
        //imageButtonData.Button.Select();
        PatternMaterial.SetTexture("Texture", imageButtonData.Image.texture);
        PatternPlane.SetActive(true);
        ThirdStep.SetActive(true);
    }
    void OnObjectButtonSelected(ImageButtonData imageButtonData)
    {
        // choose object - will be spawned in front of the user and grabbable
        imageButtonData.Button.Select();
        PatternMaterial.SetTexture("Texture", imageButtonData.Image.texture);
        PatternPlane.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Drawing 
        if (bIsDrawingStarted && !bIsDrawingEnded)
            LineRenderer.SetPosition(1, ControllerInputManager.transform.position);
        // if spline is drawn - enable the 2nd step on ui and make a checkmark
        if(bIsDrawingEnded)
            SecondStep.SetActive(true);
        // if user releases the object - object will be snapped to the spline
        // if initial choosing curve hits a sack - disable teleportation and enable scaling with a joystick and menu to rotate it

            //Draw a Spline on the ground with the Controller
            //Place Sand - Sacks along the spline at the correct orientation and distances to form a wall
            //Modify the Height of the wall using the joystick
            //Use the right placement pattern for each height according to the image


    }


    void OnStartDrawing(InputAction.CallbackContext context)
    {
        // Get the start
        drawingStarts = ControllerInputManager.transform.position;
        LineRenderer.SetPosition(0, drawingStarts);
        LineRenderer.SetPosition(1, drawingStarts);
        LineRenderer.enabled = true;
        bIsDrawingStarted = true;
    }

    void OnStopDrawing(InputAction.CallbackContext context)
    {
        LineRenderer.SetPosition(0, drawingStarts);
        LineRenderer.SetPosition(1, ControllerInputManager.transform.position);

        // Place and rotate pattern
        PatternPlane.transform.position = (ControllerInputManager.transform.position + drawingStarts) / 2f;
        PatternPlane.transform.position -= new Vector3(0.0f, 0.05f, 0.0f);

        Vector3 direction = ControllerInputManager.transform.position - drawingStarts;
        PatternPlane.transform.rotation = Quaternion.LookRotation(direction);

        bIsDrawingEnded = true;
    }
}
