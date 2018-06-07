using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class SelectionControl : MonoBehaviour
{
    private bool    isSelecting;
    private Vector3 startMousePosition;

    public GameObject SelectionCirclePrefab;

    public void Update()
    {
        // If we press the left mouse button, begin selection and remember the location of the mouse
        if(Input.GetMouseButtonDown(0))
        {
            isSelecting        = true;
            startMousePosition = Input.mousePosition;

            foreach(SelectableUnit selectableObject in UnityEngine.Object.FindObjectsOfType<SelectableUnit>())
            {
                if(selectableObject.SelectionCircle == null)
                {
                    continue;
                }

                Destroy(selectableObject.SelectionCircle.gameObject);
                selectableObject.SelectionCircle = null;
            }
        }

        // If we let go of the left mouse button, end selection
        if(Input.GetMouseButtonUp(0))
        {
            List<SelectableUnit> selectedObjects = new List<SelectableUnit>();

            foreach(SelectableUnit selectableObject in UnityEngine.Object.FindObjectsOfType<SelectableUnit>())
            {
                if(IsWithinSelectionBounds(selectableObject.gameObject))
                {
                    selectedObjects.Add(selectableObject);
                }
            }

            // StringBuilder sb = new StringBuilder();

            // sb.AppendLine(string.Format("Selecting [{0}] Units", selectedObjects.Count));

            // foreach(SelectableUnit selectedObject in selectedObjects)
            //     sb.AppendLine("-> " + selectedObject.gameObject.name);

            // Debug.Log(sb.ToString());

            isSelecting = false;
        }

        // Highlight all objects within the selection box
        if(!isSelecting)
        {
            return;
        }

        {
            foreach(SelectableUnit selectableObject in FindObjectsOfType<SelectableUnit>())
            {
                if(IsWithinSelectionBounds(selectableObject.gameObject))
                {
                    if(selectableObject.SelectionCircle != null)
                    {
                        continue;
                    }

                    selectableObject.SelectionCircle = Instantiate(SelectionCirclePrefab);
                    selectableObject.SelectionCircle.transform.SetParent(selectableObject.transform, false);
                    selectableObject.SelectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                }
                else
                {
                    if(selectableObject.SelectionCircle == null)
                    {
                        continue;
                    }

                    Destroy(selectableObject.SelectionCircle.gameObject);
                    selectableObject.SelectionCircle = null;
                }
            }
        }
    }

    private bool IsWithinSelectionBounds(GameObject gameObjectToCheck)
    {
        if(!isSelecting)
            return false;

        Bounds viewportBounds = Helper.GetViewportBounds(Camera.main, startMousePosition, Input.mousePosition);

        return viewportBounds.Contains(Camera.main.WorldToViewportPoint(gameObjectToCheck.transform.position));
    }

    public void OnGUI()
    {
        if(!isSelecting)
        {
            return;
        }

        // Create a rect from both mouse positions
        Rect rect = Helper.GetScreenRect(startMousePosition, Input.mousePosition);

        Helper.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
        Helper.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
    }
}
