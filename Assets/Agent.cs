using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class Agent : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float rotationSpeed;

    public BoidDefinition boidDefinition;

    [SerializeField]
    private float offScreenPadding;

    [SerializeField]
    private float neighborhoodRadius;

    private bool OOB = false;

    private void Start() {
        boidDefinition = new BoidDefinition(transform.position.x, transform.position.y, transform.eulerAngles.z);
        AgentManager.RegisterAgent(this); 
    }

    private float alignment(List<BoidDefinition> boidDefinitions)
    {
        float desiredHeading = boidDefinition.heading;
        int neighborhoodCount = 1;
        foreach(BoidDefinition boidDef in boidDefinitions)
        {
            if(Vector2.Distance(boidDef.position, this.boidDefinition.position) < neighborhoodRadius)
            {
                neighborhoodCount++;
                desiredHeading += boidDef.heading;
            }
        }
        desiredHeading = desiredHeading/neighborhoodCount;
        float deltaAngle = desiredHeading - boidDefinition.heading; 
        return deltaAngle;
    }

    public void UpdatePosition(List<BoidDefinition> bds) {
        float heading = transform.eulerAngles.z * Mathf.Deg2Rad;
        float total_movement = movementSpeed * Time.deltaTime;

        transform.Translate(0, total_movement, 0);
        
        
        float deltaAngle = alignment(bds);
        float totalRotation = rotationSpeed * Time.deltaTime;

        deltaAngle = Mathf.Clamp(deltaAngle, -1*totalRotation, totalRotation);
        transform.Rotate(0,0,deltaAngle, Space.World);


        this.boidDefinition.position = new Vector2(transform.position.x, transform.position.y);
        this.boidDefinition.heading = this.boidDefinition.heading + deltaAngle;

        if(isOutOfBounds())
        {
            if(OOB == false)
            {
                reflectPosition();
                OOB = true;
            }                
        }
        else{
            if(OOB)
            {
                OOB = false;
            }
        }
    }

    private void reflectPosition()
    {    
        if(OOBX())
        {
            transform.position = new Vector3(transform.position.x*-1, transform.position.y, 0);
        }
        else if(OOBY()){
            transform.position = new Vector3(transform.position.x, transform.position.y * -1, 0);        
        }
        //should never happen
        else{
            Assert.IsTrue(false); 
        }
    }

    private bool isOutOfBounds()
    {        
        return OOBX() || OOBY();
    }

    private bool OOBX()
    {
        Bounds bounds = CameraUtility.GetCameraBounds(Camera.main);
        return transform.position.x < bounds.min.x-offScreenPadding || transform.position.x > bounds.max.x+offScreenPadding;
    }

    private bool OOBY()
    {
        Bounds bounds = CameraUtility.GetCameraBounds(Camera.main);
        return transform.position.y < bounds.min.y-offScreenPadding || transform.position.y > bounds.max.y+offScreenPadding;
    }

}
