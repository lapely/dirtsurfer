using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    // Elyse
    public Transform path;
    private List<Transform> nodes;
    private int currentNode = 0;
    private float maxMotorTorque = 300;
    private float coeficientTour;

    public CarEngine carEngine;
    public CarSteering carSteering;
    private Rigidbody carRigidbody;

    public Vector3 CoM;

    private float forwardInput; // how much you want to accelerate (0 to 1)
    private float reverseInput; // how much you want to decelerate (0 to -1)
    private float throttleInput; // computed result of forward and reverse
    private float steerInput; // steering scale from -1 (left) to 1 (right)
    
    public float carSpeed; // speed parallel to the car body (m/s)
    public float wheelRPM; // average wheel rpm

    public float driftScale;

    private float steerAngle;
    private float wheelTorque;
    private float pedalBrakeForce;
    private float handBrakeForce;

    public WheelCollider wColliderFL;
    public WheelCollider wColliderFR;
    public WheelCollider wColliderBL;
    public WheelCollider wColliderBR;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        //Set the car's center of mass (CoM)
        carRigidbody.centerOfMass = CoM;
        // Elyse
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }

    }

    void FixedUpdate()
    {
        // Elyse
        
        // average the rpm of all 4 wheel
        wheelRPM = (wColliderBR.rpm + wColliderBL.rpm + wColliderFR.rpm + wColliderFL.rpm) / 4;
        
        carSpeed = transform.InverseTransformDirection(carRigidbody.velocity).z;
        float xSpeed = transform.InverseTransformDirection(carRigidbody.velocity).x;

        ConvertThrottle();
        steerAngle = carSteering.GetSteer(steerInput, carSpeed);
        wheelTorque = carEngine.GetTorque(throttleInput, wheelRPM, 60 * carSpeed / (wColliderFR.radius * Mathf.PI));
        getCurve();
        CheckWaypointDistance();
        ApplySteer();
        ApplySteering();
        ApplyTorque();
        ApplyBrake();
        Drive();


        driftScale = Mathf.Rad2Deg * Mathf.Asin(xSpeed / carRigidbody.velocity.magnitude);
        
    }
    
    private void Drive()
    {
        if (coeficientTour < 10)
        {
            reverseInput = 0;
            forwardInput += 0.1f;
            
        }
        else
        {
            forwardInput = 0;
            reverseInput -= 0.01f;
        }
        
    }
    
    private void ApplySteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        // relativeVector /= relativeVector.magnitude; // magnitude = length
        float newSteerInput = (relativeVector.x / relativeVector.magnitude);
        steerInput = newSteerInput;
    }

    private void getCurve()
    {
        float angle = 0f;
        float a = 0f;
        float b = 0f;
        float c = 0f;
        for (int i = 0; i < 3; i++)
        {
            a = transform.InverseTransformPoint(nodes[i + currentNode + 1].position).magnitude;
            b = transform.InverseTransformPoint(nodes[i + currentNode].position).magnitude;
            c = Vector3.Distance(nodes[i + currentNode].position,nodes[i + currentNode + 1].position);
            
            if (a == 0)
            {
                a = 1;
            }else if (b == 0)
            {
                b = 1;
            }
            angle += Mathf.Acos((-(Mathf.Pow(c, 2)) + Mathf.Pow(a, 2) + Mathf.Pow(b, 2))/(2*a*b));
        }

        angle /= 3;

        coeficientTour = angle * carSpeed;


        print(coeficientTour);
    }
    
    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 2f)
        {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
    
    
    
    public void OnRespawn()
    {
        transform.position = new Vector3(0, 11, 0);
    }
    // --------------------------- user input end -----------------
    private void ConvertThrottle()
    {
        throttleInput = forwardInput+reverseInput;

        carSpeed = transform.InverseTransformDirection(carRigidbody.velocity).z;
        if (carSpeed > 5)
        {
            throttleInput = Mathf.Clamp01(throttleInput);
            pedalBrakeForce = -reverseInput;
        } else if (carSpeed < -5)
        {
            throttleInput = Mathf.Clamp(throttleInput, -1, 0);
            pedalBrakeForce = forwardInput;
        } else
        {
            pedalBrakeForce = 0f;
        }
    }
    private void ApplyTorque()
    { 
        wColliderFL.motorTorque = wheelTorque;
        wColliderFR.motorTorque = wheelTorque;
        wColliderBL.motorTorque = wheelTorque;
        wColliderBR.motorTorque = wheelTorque;
    }
    private void ApplyBrake()
    {
        wColliderFL.brakeTorque = pedalBrakeForce * 500;
        wColliderFR.brakeTorque = pedalBrakeForce * 500;
        wColliderBL.brakeTorque = Mathf.Max(pedalBrakeForce*500, handBrakeForce*1000);
        wColliderBR.brakeTorque = Mathf.Max(pedalBrakeForce*500, handBrakeForce*1000);
    }
    private void ApplySteering()
    {
        wColliderFL.steerAngle = steerAngle;
        wColliderFR.steerAngle = steerAngle;
    }


}
