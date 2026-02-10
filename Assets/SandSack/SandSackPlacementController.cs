using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

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
public class PatternData
{
    public ImageButtonData PatternButtonData;
    public Vector3[] Pattern;
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
    [Tooltip("Controller Input Action Manager.")]
    private ControllerInputActionManager m_controllerInputManager;

    // Resize
    [SerializeField]
    [Tooltip("The reference to the action of scaling (originally moving) the XR Origin with this controller.")]
    private InputActionReference m_scaleHeight;
    private InputAction m_scaleHeightAction;
    [SerializeField]
    [Tooltip("The reference to the action of scaling (originally moving) the XR Origin with this controller.")]
    private InputActionReference m_scaleWidth;
    private InputAction m_scaleWidthAction;

    // Drawing
    [SerializeField]
    [Tooltip("The reference to the action of drawing with this controller.")]
    private InputActionReference m_draw;
    [SerializeField]
    [Tooltip("Line renderer.")]
    private LineRenderer m_lineRenderer;

    private InputAction m_DrawAction;
    private Vector3 drawingStarts;
    private bool bIsDrawingStarted = false;
    private bool bIsDrawingEnded = false;

    // UI
    [SerializeField] private GameObject m_secondChoosePatternStep; // for pattern
    [SerializeField] private GameObject m_thirdChooseObjectStep;// for object
    [SerializeField] private Button m_patternButtonTemplate;
    [SerializeField] private Button m_objectForPlacementButtonTemplate;

    // Pattern
    [SerializeField]
    [Tooltip("Object to place after drawing.")]
    GameObject m_patternPlane;
    // Array of possible patterns 
    [SerializeField] private List<PatternData> m_patternsDataBase = new();
    [SerializeField] private Material m_patternMaterial;
    private Vector3[] m_currentPattern = {
        new(2.0f, 7.0f, 0.0f),
        new(4.0f, 3.0f, 1.0f),
        new(1.0f, 7.0f, 0.0f),
        new(4.0f, 1.0f, 1.0f),
    };

    // Array of possible objects
    [SerializeField] private List<ObjectForPlacementData> m_objectsForPlacementDataBase = new();

    List<GameObject> m_spawnedParentObjects = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_secondChoosePatternStep.SetActive(false);
        m_thirdChooseObjectStep.SetActive(false);
        // check that not null, if null - write an error in the log
        // Drawing Setup
        m_DrawAction = ControllerInputActionManager.GetInputAction(m_draw);
        if (m_DrawAction != null)
        {
            m_DrawAction.started += OnStartDrawing;
            m_DrawAction.canceled += OnStopDrawing;
        }

        // Scaling setup
        m_scaleHeightAction = ControllerInputActionManager.GetInputAction(m_scaleHeight);
        m_scaleHeightAction?.Enable();
        if (m_scaleHeightAction != null)
        {
            m_scaleHeightAction.started += OnStartHeightScaling;
            m_scaleHeightAction.canceled += OnStopHeightScaling;
        }

        m_scaleWidthAction = ControllerInputActionManager.GetInputAction(m_scaleWidth);
        m_scaleWidthAction?.Enable();
        if (m_scaleWidthAction != null)
        {
            m_scaleWidthAction.started += OnStartWidthScaling;
            m_scaleWidthAction.canceled += OnStopWidthScaling;
        }

        foreach (var pattern in m_patternsDataBase)
        {
            pattern.PatternButtonData.SetButton(Instantiate(m_patternButtonTemplate, m_patternButtonTemplate.transform.parent));
            pattern.PatternButtonData.Button.image.sprite = pattern.PatternButtonData.Image;
            pattern.PatternButtonData.Button.gameObject.SetActive(true);
            pattern.PatternButtonData.Button.onClick.AddListener(() =>
            {
                OnPatternButtonSelected(pattern);
            });
        }
        // Objects to spawn setup
        foreach (var objectData in m_objectsForPlacementDataBase)
        {
            objectData.ImageButtonData.SetButton(Instantiate(m_objectForPlacementButtonTemplate, m_objectForPlacementButtonTemplate.transform.parent));
            objectData.ImageButtonData.Button.image.sprite = objectData.ImageButtonData.Image;
            objectData.ImageButtonData.Button.gameObject.SetActive(true);
            objectData.ImageButtonData.Button.onClick.AddListener(() =>
            {
                OnObjectButtonSelected(objectData);
            });
        }
    }

    public void Restart()
    {
        foreach(var spawnedObj in m_spawnedParentObjects)
        {
            Destroy(spawnedObj);
        }
        m_spawnedParentObjects.Clear();
    }

    private void OnDestroy()
    {
        if (m_DrawAction != null)
        {
            m_DrawAction.started -= OnStartDrawing;
            m_DrawAction.canceled -= OnStopDrawing;
        }

        if (m_scaleHeightAction != null)
        {
            m_scaleHeightAction.started -= OnStartHeightScaling;
            m_scaleHeightAction.canceled -= OnStopHeightScaling;
        }

        if (m_scaleWidthAction != null)
        {
            m_scaleWidthAction.started -= OnStartWidthScaling;
            m_scaleWidthAction.canceled -= OnStopWidthScaling;
        }
    }
    [SerializeField]
    private GameObject template;
    private void OnStartHeightScaling(InputAction.CallbackContext context)
    {
        Scale(context.ReadValue<Vector2>());
    }

    private void OnStopHeightScaling(InputAction.CallbackContext context)
    {

    }

    private void OnStartWidthScaling(InputAction.CallbackContext context)
    {
            Scale(context.ReadValue<Vector2>());
        
    }

    private void OnStopWidthScaling(InputAction.CallbackContext context)
    {
    }

    private void Scale(Vector2 scaleData)
    {
        if (m_spawnedParentObjects.Count != 0)
        {
            Vector3 scaleValue = new Vector3(0.0f, scaleData.y, 0.0f);
            m_patternPlane.transform.localScale += new Vector3(0.01f, 0.0f, 0.01f);

            foreach (var obj in m_spawnedParentObjects)
            {
                obj.transform.localScale += scaleValue;
            }
        }
    }

    // First Step - draw line
    void OnStartDrawing(InputAction.CallbackContext context)
    {
        //Draw a Spline on the ground with the Controller
        drawingStarts = m_controllerInputManager.transform.position;
        m_lineRenderer.SetPosition(0, drawingStarts);
        m_lineRenderer.SetPosition(1, drawingStarts);
        m_lineRenderer.enabled = true;
        bIsDrawingStarted = true;
    }

    void OnStopDrawing(InputAction.CallbackContext context)
    {
        // Finish the line
        m_lineRenderer.SetPosition(0, drawingStarts);
        m_lineRenderer.SetPosition(1, m_controllerInputManager.transform.position);

        // Place and rotate pattern
        m_patternPlane.transform.position = (m_controllerInputManager.transform.position + drawingStarts) / 2.0f;
        m_patternPlane.transform.position -= new Vector3(0.0f, 0.005f, 0.0f);

        var direction = m_controllerInputManager.transform.position - drawingStarts;
        direction.y = 0f;
        m_patternPlane.transform.rotation = Quaternion.LookRotation(direction);

        bIsDrawingEnded = true;
    }

    // Second Step - select pattern
    void OnPatternButtonSelected(PatternData patternData)
    {
        // choosing pattern - activates plane (for future - slowly)
        // pattern image should be along the line - to test
        m_thirdChooseObjectStep.SetActive(true);
        patternData.PatternButtonData.Button.Select();
        // scale according to the texture
        m_patternMaterial.SetTexture("_BaseMap", patternData.PatternButtonData.Image.texture);
        m_patternPlane.SetActive(true);
        m_currentPattern = patternData.Pattern;
    }

    // Third Step - select objects to spawn
    void OnObjectButtonSelected(ObjectForPlacementData objectForPlacementData)
    {
        // Select the button
        if(objectForPlacementData.ImageButtonData.Button != null)
            objectForPlacementData.ImageButtonData.Button.Select();

        // Get collider data
        Vector3 boxColliderWorldSize = GetBoxColliderWorldSize(objectForPlacementData.Object.GetComponent<BoxCollider>());

        if (boxColliderWorldSize != Vector3.zero)
        {
            for (int levelCounter = 0; levelCounter < m_currentPattern.Length; levelCounter++)
            {
                GameObject layerParent = new GameObject();
                SpawnObjects(objectForPlacementData, ref boxColliderWorldSize, ref levelCounter, layerParent);

                // Turn layers with objects towards line
                layerParent.transform.position += m_lineRenderer.GetPosition(0);
                layerParent.transform.LookAt(m_lineRenderer.GetPosition(1), GetLineDirection());
                m_spawnedParentObjects.Add(layerParent);
            }
        }
    }

    Vector3 GetLineDirection()
    {
        var heading = m_lineRenderer.GetPosition(0) - m_lineRenderer.GetPosition(1);
        var distance = heading.magnitude;
        return heading / distance;
    }

    private Vector3 GetBoxColliderWorldSize(BoxCollider boxCollider)
    {
        if(boxCollider != null)
            return Vector3.Scale(boxCollider.size, boxCollider.gameObject.transform.lossyScale);
        return Vector3.zero;
    }

    [ContextMenu("Spawn Pattern")]
    void TestPattern()
    {
        m_currentPattern = m_patternsDataBase[1].Pattern;
        OnObjectButtonSelected(m_objectsForPlacementDataBase[1]);
    }
    private void SpawnObjects(ObjectForPlacementData objectForPlacementData, ref Vector3 boxColliderWorldSize, ref int levelCounter, GameObject layerParent)
    {
        for (int rowCounter = 0; rowCounter < m_currentPattern[levelCounter].x; rowCounter++)
        {
            for (int columnCounter = 0; columnCounter < m_currentPattern[levelCounter].y; columnCounter++)
            {
                // Calculate offset Z depending from the size of box collider and amount of objects in a layer
                // set new position
                Vector3 pos = GetPositionWithOffset(ref boxColliderWorldSize, ref levelCounter, ref rowCounter, ref columnCounter);
                // Spawn new object in the calculated position
                var newObj = Instantiate(objectForPlacementData.Object, pos, Quaternion.identity, layerParent.transform);
                // Rotate the object
                if (m_currentPattern[levelCounter].z == 1.0f)
                    newObj.transform.Rotate(0.0f, 90.0f, 0.0f);
            }
        }
    }

    private Vector3 GetPositionWithOffset(ref Vector3 worldSize, ref int levelCounter, ref int rowCounter, ref int columnCounter)
    {
        Vector3 pos;
        var offsetZ = 0.0f;
        var offsetX = worldSize.x / 2 * levelCounter;

        if (m_currentPattern[levelCounter].z == 1.0f)
        {
            offsetZ = worldSize.x + Mathf.Abs(m_currentPattern[0].x * worldSize.z - m_currentPattern[levelCounter].y * worldSize.x) / 2;
            pos = new Vector3(worldSize.z * rowCounter + offsetX, worldSize.y * levelCounter, worldSize.x * columnCounter + offsetZ);
        }
        else
        {
            if (levelCounter != 0)
            {
                offsetZ = Mathf.Abs((m_currentPattern[0].x - m_currentPattern[levelCounter].x) * worldSize.z / 2);
            }
            pos = new Vector3(worldSize.x * columnCounter + offsetX, worldSize.y * levelCounter, worldSize.z * rowCounter + offsetZ);
        }

        return pos;
    }

    // Update is called once per frame
    void Update()
    {
        // Drawing 
        if (bIsDrawingStarted && !bIsDrawingEnded)
            m_lineRenderer.SetPosition(1, m_controllerInputManager.transform.position);
        // if spline is drawn - enable the 2nd step on ui and make a checkmark
        if (bIsDrawingEnded)
            m_secondChoosePatternStep.SetActive(true);
    }
}
