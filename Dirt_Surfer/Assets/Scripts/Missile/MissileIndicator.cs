using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissileIndicator : MonoBehaviour
{
    public Camera currentCamera;
    public RawImage missileAlertImage;

    public Text distanceFromCar;
    private SpriteRenderer sprite;

    public Transform car;
    public Transform missile;

    public Vector3 offset;
    
    Vector3 positionMissile;


    private void Start()
    {
        sprite = missileAlertImage.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(missileAlertImage != null)
        {
            // I 
            float xMinMarker = missileAlertImage.GetPixelAdjustedRect().width / 2;
            float xMaxMarker = Screen.width - xMinMarker;

            float yMinMarker = missileAlertImage.GetPixelAdjustedRect().height / 2;
            float yMaxMarker = Screen.height - yMinMarker;



            //I find where the missile is to put my alert on it
            positionMissile = currentCamera.WorldToScreenPoint(missile.position + offset);

            if (Vector3.Dot((missile.position - transform.position), transform.forward) < 0)
            {
                // Target is behind the player
                if (positionMissile.x < Screen.width / 2)
                    positionMissile.x = xMaxMarker;
                else
                    positionMissile.x = xMinMarker;

            }

            positionMissile.x = Mathf.Clamp(positionMissile.x, xMinMarker, xMaxMarker);
            positionMissile.y = Mathf.Clamp(positionMissile.y, yMinMarker, yMaxMarker);

            missileAlertImage.transform.position = positionMissile;
            distanceFromCar.text = ((int)Vector3.Distance(missile.position, car.position)).ToString() + "m";

        }
    }
}
