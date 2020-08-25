using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float speedZoom = 20.0f;
    private float minZoom = 0.5f;
    public float maxZoom { get; set; } = 5.0f;
    private float zoom = 10000.0f;

    private Vector3 dragPrevPosition;
    private bool isDragging;
    private LayerMask groundMask;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        groundMask = LayerMask.GetMask("Ground", "UI");
        isDragging = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Zoom();
        Drag();
    }

    private void Zoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            zoom -= Time.deltaTime * speedZoom;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            zoom += Time.deltaTime * speedZoom;
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            cam.orthographicSize = zoom;
        }
        
    }

    private bool IsMouseOnScreen()
    {
        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        return mousePosition.x >= 0.0f && mousePosition.x <= 1.0f
                && mousePosition.y >= 0.0f && mousePosition.y <= 1.0f;
    }
    private void Drag()
    {
        if (!Input.GetMouseButton(0))
        {
            isDragging = false;
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && IsMouseOnScreen())
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, groundMask))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                dragPrevPosition = Input.mousePosition;
                isDragging = true;
            }
        }

        if (isDragging)
        {
            if (IsMouseOnScreen())
            {
                Vector3 move = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(dragPrevPosition);
                transform.Translate(-move, Space.World);
            }
            dragPrevPosition = Input.mousePosition;
        }

    }
}
