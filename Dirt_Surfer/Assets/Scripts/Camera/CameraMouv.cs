using UnityEngine;
using System.Collections.Generic;

public class CameraMouv : MonoBehaviour
{
    public Transform target;
    public Vector3 cameraOffset;
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;

    public Rigidbody targetRigidbody;
    public Camera cameraObj;

    private List<Quaternion> pastRotations = new List<Quaternion>();

    public int cacheSize;
    public float weighIncrement;

    void Start() 
    {
        
        for(int i = 0; i < cacheSize; i++)
        {
            pastRotations.Add(getNewCamRotation(transform));
        }
        
    }

    void FixedUpdate()
    {
        //target.InverseTransformDirection(carRigidbody.velocity).z
        cameraOffset.z = -8 + target.InverseTransformDirection(targetRigidbody.velocity).z / 5.5f;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.TransformPoint(cameraOffset), 
            ref velocity, 
            smoothTime
            );

        transform.rotation = averageRotation();
        //transform.rotation = getNewCamRotation(transform); 
        //transform.LookAt(target);
        //transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 0.3f);
        cameraObj.fieldOfView = Mathf.Lerp(60, 90, targetRigidbody.velocity.magnitude / 60f); // 60 + targetRigidbody.velocity.magnitude * 40 / 80;

    }

    private Quaternion getNewCamRotation(Transform refTransform)
    {
        //Transform baseRotation = Instantiate(refTransform);
        refTransform.LookAt(target);
        //baseRotation.LookAt(target); 
        return Quaternion.Lerp(refTransform.rotation, target.rotation, 0.5f); //refTransform.rotation;  
    }

    private Quaternion averageRotation()
    {
        Quaternion smoothedRotation = pastRotations[0];
        pastRotations.RemoveAt(0);
        pastRotations.Add(getNewCamRotation(transform));
        float avgWeith = 2;
        foreach (Quaternion item in pastRotations)
        {
            smoothedRotation = Quaternion.Lerp(smoothedRotation, item, 1/avgWeith);
            avgWeith += weighIncrement;
        }
        return smoothedRotation;
    }
}
