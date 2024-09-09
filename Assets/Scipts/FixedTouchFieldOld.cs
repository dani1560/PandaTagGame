using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using StarterAssets;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Cinemachine;

public class FixedTouchFieldOld : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
{


    public RectTransform containerRect;

    [Header("Settings")]
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    //Stored Pointer Values
    private Vector2 pointerDownPosition;
    private Vector2 currentPointerPosition;


    [SerializeField] private UICanvasControllerInput uiCanvasControllerInput;
  
    int pid = 0;

    public List<int> touchIds;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (touchIds.Count < 1)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition);
            pid = eventData.pointerId;
        }
       touchIds.Add(eventData.pointerId);
       
    }
    
    Vector2 oldPostion = Vector2.zero;
    [HideInInspector]public Vector2 touchDist = Vector2.zero;
    Vector2 dragVal = Vector2.zero;




    float touchesPrevPosDifference, touchesCurPosDifference, zoomModifier;
    [SerializeField]float minZoom,maxZoom;

    Vector2 firstTouchPrevPos, secondTouchPrevPos;

    [SerializeField]
    float zoomModifierSpeed = 0.1f;

    [SerializeField] CinemachineVirtualCamera camera;
    private void Update()
    {
        if(touchIds.Count<=1)
        {
            touchDist = touchDist - oldPostion;
            oldPostion = touchDist;

            if (touchDist == Vector2.zero)
            {

                uiCanvasControllerInput.VirtualLookInput(Vector2.zero);
            }
            else
            {
                uiCanvasControllerInput.VirtualLookInput(dragVal);
            }
        }
        else
        {
            if (Input.touchCount == 2)
            {
                Touch firstTouch = Input.GetTouch(0);
                Touch secondTouch = Input.GetTouch(1);

                firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

                touchesPrevPosDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                touchesCurPosDifference = (firstTouch.position - secondTouch.position).magnitude;

                zoomModifier = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * zoomModifierSpeed;


                var thirdperson = camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
               

                if (touchesPrevPosDifference > touchesCurPosDifference)
                    thirdperson.CameraDistance += zoomModifier;
                if (touchesPrevPosDifference < touchesCurPosDifference)
                    thirdperson.CameraDistance -= zoomModifier;

                thirdperson.CameraDistance = Mathf.Clamp(thirdperson.CameraDistance, minZoom, maxZoom);

            }

           
          
        }
        

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pid == eventData.pointerId)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition);

            Vector2 positionDelta = GetDeltaBetweenPositions(pointerDownPosition, currentPointerPosition);

            positionDelta = ApplyInversionFilter(positionDelta);

            touchDist = positionDelta;

            dragVal = positionDelta * magnitudeMultiplier * Time.deltaTime;
            pointerDownPosition = currentPointerPosition;
        }
        

    }
    public void OnEndDrag(PointerEventData eventData)
    {

     
   
        uiCanvasControllerInput.VirtualLookInput(Vector2.zero);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        touchIds.Remove(eventData.pointerId);
        if (touchIds.Count == 1)
        {
           
            pid = touchIds[0];
        }
        else if (touchIds.Count < 1)
        {
            pointerDownPosition = Vector2.zero;
            currentPointerPosition = Vector2.zero;
            pid = 0;
            touchIds.Clear();
        }


       



    }

    Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
    {
        return secondPosition - firstPosition;
    }

    Vector2 ApplyInversionFilter(Vector2 position)
    {
        if (invertXOutputValue)
        {
            position.x = InvertValue(position.x);
        }

        if (invertYOutputValue)
        {
            position.y = InvertValue(position.y);
        }

        return position;
    }

    float InvertValue(float value)
    {
        return -value;
    }

}