using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float distance = 5;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = +45;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] float RotationSpeed = 2;
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;
    float rotationY;
    float rotationX;
    float invertXVal;
    float invertYVal;
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;
        rotationY += Input.GetAxis("Mouse X") * RotationSpeed * invertYVal;
        
        rotationX += Input.GetAxis("Mouse Y") * RotationSpeed * invertXVal;
        rotationX = Mathf.Clamp(rotationX,minVerticalAngle,maxVerticalAngle);
        //Quaternion.Euler(0, 45, 0);
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        transform.position = focusPosition - targetRotation * new Vector3(0,0,distance);
        transform.rotation = targetRotation;

    }

    public Quaternion PlanerRotation => Quaternion.Euler(0, rotationY, 0);
}
