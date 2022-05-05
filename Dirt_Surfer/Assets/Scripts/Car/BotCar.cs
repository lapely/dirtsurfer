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
    private float angle = 0f;
    public int invervalPoint = 20; // 1/n point pour les waypoint
    public float maxCoeficientTour = 20f; // a quel point il ralenti dans les virages (plus haut = moins de ralentissement)
    public float minWaypointDistance = 10f; // distance en m à laquelle le bot change de waypoint


    private CarBody carBody;
    public CarEngine carEngine;
    public CarSteering carSteering;
    public CarBrakes carBrakes;

    private float forwardInput; // how much you want to accelerate (0 to 1)
    private float reverseInput; // how much you want to decelerate (0 to -1)
    private float throttleInput; // computed result of forward and reverse
    private float pedalBrakeInput;
    private float handBrakeInput;
    private float steerInput; // steering scale from -1 (left) to 1 (right)

    public float carSpeed; // speed parallel to the car body (m/s)


    private float steerAngle;
    private float wheelTorque;

    void Start()
    {
        carBody = GetComponent<CarBody>();
        handBrakeInput = 0f;


        // Elyse
        curve.generation();
        //print(curve.getPosition()[2999]);
        for (int i = 0; i < curve.getPosition().Count; i++)
        {
            if (i % invervalPoint == 0)
            {
                nodes.Add(curve.getPosition()[i]);
            }
        }
    }

    void FixedUpdate()
    {
        // Elyse

        carSpeed = carBody.BodyVelocity() * Mathf.Sign(carBody.ForwardSpeed());
        
        CheckWaypointDistance();
        ConvertThrottle();
        steerAngle = carSteering.SetSteer(steerInput);
        wheelTorque = carEngine.GetTorque(throttleInput);
        carBrakes.SetBrake(pedalBrakeInput, handBrakeInput);
        getCurve();
        ApplySteer();
        Drive();
    }

    private void Drive()
    {
        if (coeficientTour < maxCoeficientTour && carSpeed < 33.3f) //33.3 m/s = 120kmh
        {
            reverseInput = 0;
            forwardInput += 0.5f;

        }
        else if (carSpeed > 18f)
        {
            forwardInput = 0;
            reverseInput -= 0.5f;
        }
        else
        {
            reverseInput = 0;
            forwardInput += 0.2f;
        }

    }

    private void ApplySteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode]);
        float newSteerInput = (relativeVector.x / relativeVector.magnitude);
        if (Math.Abs(steerAngle) > 10)
        {
            steerInput = newSteerInput * 4f;
        }
        else
        {
            steerInput = newSteerInput * 2f;
        }

        steerInput = Mathf.Clamp(steerInput, -1, 1);

    }

    private void getCurve()
    {
        angle = 0f;
        float a = 0f;
        float b = 0f;
        float c = 0f;
        for (int i = 1; i < 6; i++)
        {
            if ((i + currentNode + 1) < nodes.Count)
            {
                a = transform.InverseTransformPoint(nodes[i + currentNode + 1]).magnitude;
                b = transform.InverseTransformPoint(nodes[i + currentNode]).magnitude;
                c = Vector3.Distance(nodes[i + currentNode], nodes[i + currentNode + 1]);

                if (a == 0)
                {
                    a = 1;
                }
                else if (b == 0)
                {
                    b = 1;
                }
                angle += ((3.1416f) - (Mathf.Acos((-(Mathf.Pow(a, 2)) + Mathf.Pow(c, 2) + Mathf.Pow(b, 2)) / (2 * c * b)))) / ((float)i / 2);
            }
        }

        coeficientTour = (angle * carSpeed);
    }

    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode]) < minWaypointDistance)
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


    private void ConvertThrottle()
    {
        throttleInput = forwardInput + reverseInput;

        if (carSpeed > 5)
        {
            throttleInput = Mathf.Clamp01(throttleInput);
            pedalBrakeInput = -reverseInput;
        }
        else if (carSpeed < -5)
        {
            throttleInput = Mathf.Clamp(throttleInput, -1, 0);
            pedalBrakeInput = forwardInput;
        }
        else
        {
            pedalBrakeInput = 0f;
        }
    }

}
