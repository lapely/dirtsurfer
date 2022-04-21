using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.InputSystem;
using UnityEngine;

public class BotCar : MonoBehaviour
{
    public BezierCurveCreator curve;
    private List<Vector3> nodes = new List<Vector3>();
    // Elyse
    private int currentNode = 0;
    private float coeficientTour;
    public float angle = 0f;

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
        curve.generation();
        for (int i = 0; i < curve.getPosition().Count; i++)
        {
            if (i % 10 == 0)
            {
                nodes.Add(curve.getPosition()[i]);
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

        /*print(currentNode);
        print(pos_in_curve[currentNode]);
        print(Vector3.Distance(transform.position, pos_in_curve[currentNode]));*/
        /*print(pos_in_curve[98]);
        print(pos_in_curve[99]);
        print(pos_in_curve[100]);*/
        driftScale = Mathf.Rad2Deg * Mathf.Asin(xSpeed / carRigidbody.velocity.magnitude);
        //print(Vector3.Distance(nodes[1], nodes[2]));

    }
    
    private void Drive()
    {
        if (coeficientTour < 20)
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
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode]);
        // relativeVector /= relativeVector.magnitude; // magnitude = length
        float newSteerInput = (relativeVector.x / relativeVector.magnitude);
        if (Math.Abs(steerAngle) > 6)
        {
            steerInput = newSteerInput * 3.5f;
        }
        else
        {
            steerInput = newSteerInput * 1.5f;
        }
        
    }

    private void getCurve()
    {
        angle = 0f;
        float a = 0f;
        float b = 0f;
        float c = 0f;
        for (int i = 0; i < 12; i++)
        {
            print(nodes.Count);
            a = transform.InverseTransformPoint(nodes[i + currentNode + 1]).magnitude;
            b = transform.InverseTransformPoint(nodes[i + currentNode]).magnitude;
            c = Vector3.Distance(nodes[i + currentNode],nodes[i + currentNode + 1]);
            
            if (a == 0)
            {
                a = 1;
            }else if (b == 0)
            {
                b = 1;
            }
            angle += Mathf.Acos((-(Mathf.Pow(c, 2)) + Mathf.Pow(a, 2) + Mathf.Pow(b, 2))/(2*a*b));
        }

        print(angle);
        
        coeficientTour = angle * carSpeed;
        
        //steerInput *= ((coeficientTour / 10) + 1.3f);
    }
    
    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode]) < 10f)
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
