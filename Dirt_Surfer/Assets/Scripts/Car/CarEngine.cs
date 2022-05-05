using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    private CarBody carBody;

    public float maxForce;
    public float maxRPM;
    public const float idleRPM = 1000;

    public float input;
    public float RPM;
    public float gear;

    public float wheelRPM;
    public float wheelMeanRPM;
    public float efficiency;
    private float outputForce;

    private float targetRPM;
    private float lastRPM;
    private float shiftingState;
    private float targetGear;

    void Start()
    {
        carBody = GetComponent<CarBody>();
        gear = 0f;
        input = 0f;
        RPM = 0f;
        maxRPM = Mathf.Clamp(maxRPM, 1000, 10000);
        maxForce = Mathf.Clamp(maxForce, 10, 10000);
    }
        
    public float GetTorque(float masterInput)
    {
        input = masterInput;
        wheelMeanRPM = Mathf.Clamp(carBody.WheelRPM(), -500f, 2000f); // old, to be removed
        wheelRPM = Mathf.Clamp(carBody.MeanRPM(), -500f, 2000f);
        ShiftGear(); 
        WheelToEngineRPM();
        EngineEfficiency();
        ApplyEfficiency();
        carBody.ApplyTorque(outputForce);
        return outputForce;
    }

    // this needs to be reworked to include the clutch
    private void ShiftGear()
    {
        if(gear > 0)
        {
            gear = Mathf.Clamp(Mathf.CeilToInt(wheelRPM / 200), 1, 10);
        }
        //targetGear = Mathf.Clamp(Mathf.CeilToInt(wheelRPM / 200), 1, 10);
        if(gear == 0 && RPM > 8000)
        {
            gear = 1 * Mathf.Sign(input);
        }


        if (RPM < idleRPM) // prevent engine stall
        {
            gear = 0;
        }
        
        //gear = targetGear;
    }
    private void WheelToEngineRPM()
    {
        
        if (gear > 0)
        {
            targetRPM = Mathf.Clamp(
                //10f * maxRPM * Mathf.Abs(wheelRPM) / (2000f * gear),
                10f * maxRPM * wheelRPM / (2000f * gear),
                0f,
                1.1f * maxRPM
                );
            RPM += (targetRPM - RPM) / (10f/gear);
            //RPM = Mathf.Clamp(RPM, 0f, maxRPM);
        } else if (gear == 0)
        {
            targetRPM = idleRPM;
            RPM = RPM + 500 * Mathf.Abs(input) + (targetRPM - RPM) / 20f;
            RPM = Mathf.Clamp(RPM, 0f, maxRPM);
        } else
        {
            targetRPM = Mathf.Clamp(
                10f * maxRPM * wheelRPM / (2000f * gear),
                0f,
                1.1f * maxRPM
                );
            RPM += (targetRPM - RPM) / 20f;
        }
        
    }
    private void EngineEfficiency()
    {
        if (gear != 0)
        {
            float var1 = Mathf.Abs(gear) * RPM / maxRPM + 0.2f;
            float var2 = 1.2f * (var1 - Mathf.Abs(gear)) / Mathf.Sqrt(Mathf.Abs(gear));
            float var3 = Mathf.Pow((Mathf.Sin(var2) / var2), 5f) - Mathf.Pow(var1 / 10f, 2f);
            float var4 = var3 - Mathf.Pow(10f, (RPM - 1.1f * maxRPM) / 500f);
            efficiency = Mathf.Clamp01(var4);
        }
        else
        {
            efficiency = 0;
        }
        
    }
    private void ApplyEfficiency()
    {
        //if((input*gear)>=0) 
            outputForce = input * efficiency * maxForce;
    }
    
}
