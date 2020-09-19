using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;
using UnityEngine.XR.ARSubsystems;


public class ApplicationsBehavior : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject placementIndecator;
    public ARRaycastManager arOrigin;
    private Pose placementPose;
    private bool placementPostIsValid = false;
    public CaptureImage snapShot;

    [SerializeField] RectTransform objectsPanel;
    private float easing = 0.5f;

    [SerializeField] bool edit = false;

    public float rotateSpeed = 0.1f;

    public float alpha = 0.5f;
    public float moveSpeed = .0025f;


    [SerializeField] GameObject editAbleObject;

    [SerializeField] GameObject AddObjBTN;
    [SerializeField] GameObject editCanvas;


    [SerializeField] GameObject editPanel;

    public bool modeRotate = false, modeMove = false;

    public bool addObject = false;

    public bool planeScane = false;

 

    void FixedUpdate()
    {
        
        if (planeScane)
        {
           UpdatePlacementPose();
           UpdatePlacementIndecator();
           if (placementPostIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
           {
               PlaceObject();
           }
        
        }
        else
        {
            if (!edit)
            {

                int touchCount = Input.touchCount;
                if (touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.pressure > 6)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);

                        RaycastHit hitInfo;

                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.collider.tag == "MoveAble")
                            {
                                edit = true;
                                editAbleObject = hitInfo.collider.gameObject;
                                EditTag();

                            }
                        }
                    }
                    else if(touch.tapCount == 2 && touch.phase == TouchPhase.Ended)
                    {
                        snapShot.TakePicture();
                        
                        
                    }
                }
            }
            else
            {

                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (modeMove)
                    {
                        MoveObject(touch);
                    }
                    if (modeRotate)
                    {
                        RotareObject(touch);
                    }

                }
            }
        }


    }
    
        private void PlaceObject()
        {
            Transform objTrans = objectToPlace.GetComponent<Transform>();    
            Instantiate(objectToPlace, placementPose.position, objTrans.rotation);
            planeScane = false;
            AddObjBTN.SetActive(true);
            objectToPlace = null;
            placementIndecator.SetActive(false);
        }

        private void UpdatePlacementIndecator()
        {
            if (placementPostIsValid)
            {
                placementIndecator.SetActive(true);
                placementIndecator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                placementIndecator.SetActive(false);
            }
        }

        private void UpdatePlacementPose()
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(.5f, .5f));
            var hits = new List<ARRaycastHit>();
            arOrigin.Raycast(screenCenter, hits, TrackableType.Planes);
            placementPostIsValid = hits.Count > 0;
            if (placementPostIsValid)
            {
                placementPose = hits[0].pose;
                var cameraFroward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraFroward.x, 0, cameraFroward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
        }




    public void EditModeRotate()
    {
        modeRotate = true;
        modeMove = false;
    }

    public void DeleteObject()
    {
        modeRotate = false;
        modeMove = false;
        AddObjBTN.SetActive(true);
        editPanel.SetActive(false);
        Destroy(editAbleObject);
        editAbleObject = null;
        edit = false;
    }

    public void EditModeMove()
    {
        modeRotate = false;
        modeMove = true;
    }

    public void EditCancel()
    {
        Renderer[] rendT = editAbleObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rendT.Length; i++)
        {
            int materialLength = rendT[i].materials.Length;
            foreach (Material material in rendT[i].materials)
            {
                Color oldColor = material.color;
                Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, 1);
                material.SetColor("_Color", newColor);
            }
        }

        editAbleObject = null;
        edit = false;
        AddObjBTN.SetActive(true);
        editPanel.SetActive(false);
        modeRotate = false;
        modeMove = false;

    }

    private void RotareObject(Touch touch)
    {
        if (touch.phase == TouchPhase.Moved)
        {
            Transform trans = editAbleObject.GetComponent<Transform>();
     
            if (editAbleObject.name.Contains("Sofa"))
            {
                trans.Rotate(0, 0, trans.rotation.z - touch.deltaPosition.x * rotateSpeed);
            }
            else
            {
                trans.Rotate(0, trans.rotation.y - touch.deltaPosition.x * rotateSpeed, 0);
            }

        }
    }

    private void MoveObject(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            if (hitInfo.collider.tag == "MoveAble" && hitInfo.collider.name == editAbleObject.name)
            {
                if (touch.phase == TouchPhase.Moved)
                {

                    Transform trans = hitInfo.collider.GetComponent<Transform>();
                    trans.position = new Vector3(
                        trans.position.x + touch.deltaPosition.x * moveSpeed,
                        trans.position.y,
                        trans.position.z + touch.deltaPosition.y * moveSpeed);
                }
            }
        }
    }

    private void EditTag()
    {

        editPanel.SetActive(true);
        AddObjBTN.SetActive(false);

        Renderer[] rendT = editAbleObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rendT.Length; i++)
        {
            int materialLength = rendT[i].materials.Length;
            foreach (Material material in rendT[i].materials)
            {
                Color oldColor = material.color;
                Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
                material.SetColor("_Color", newColor);
            }

        }
    }


    public void AddObjectPanel()
    {

        Vector3 localPosition = objectsPanel.localPosition;
        int xPos = 0;
        if (localPosition.x == 0)
        {
            xPos = Screen.width;
        }

        Vector3 endPos = new Vector3(xPos, 0, 0);
        StartCoroutine(ObjectsPanel(objectsPanel.localPosition, endPos, easing));
    }

    public void SelectedObject()
    {
        Vector3 endPos = new Vector3(Screen.width, 0, 0);
        StartCoroutine(ObjectsPanel(objectsPanel.localPosition, endPos, easing));

    }

    IEnumerator ObjectsPanel(Vector3 startPos, Vector3 endPos, float seconds)
    {

        float t = 0f;
        while (t <= 1.0)
        {

            t += Time.deltaTime / seconds;

            objectsPanel.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

            
        }
        yield return null;

    }


    public void SelectObjReference(GameObject obj)
    {
        objectToPlace = obj;
        planeScane = true;
        SelectedObject();
    }
}
