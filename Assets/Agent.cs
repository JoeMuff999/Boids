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


    [Header("Boid Paramters")]

    [SerializeField]
    private float neighborhoodRadius;
    [SerializeField]
    public float separationRadius;
    [SerializeField]
    private float separationWeight;
    [SerializeField]
    private float alignmentWeight;
    [SerializeField]
    private float cohesionWeight;

    private bool OOB = false;

    private void Start() {
        boidDefinition = new BoidDefinition(transform.position.x, transform.position.y, transform.eulerAngles.z, movementSpeed);
        AgentManager.RegisterAgent(this); 
    }

    private float alignment(List<BoidDefinition> neighborhood)
    {
        float desiredHeading = boidDefinition.heading;
        int neighborhoodCount = 1;
        foreach(BoidDefinition boidDef in neighborhood)
        {
            neighborhoodCount++;
            desiredHeading += boidDef.heading;
        }
        desiredHeading = desiredHeading/neighborhoodCount;
        float deltaAngle = desiredHeading - boidDefinition.heading; 
        return deltaAngle;
    }

    private float separation(List<BoidDefinition> neighborhood)
    {
        float x_influence = 0; 
        float y_influence = 0;
        foreach(BoidDefinition boidDef in neighborhood)
        {
            if(Vector2.Distance(boidDef.position, this.boidDefinition.position) < separationRadius)
            {
                //normalize vectors so that all boids within radius are weighted equally
                float x_diff = boidDef.position.x - this.boidDefinition.position.x;
                float y_diff = boidDef.position.y - this.boidDefinition.position.y;
                float mag = Mathf.Sqrt(x_diff*x_diff + y_diff*y_diff);
                // Debug.Log(mag);
                Debug.Log(boidDef.position.x + " " + this.boidDefinition.position.x);

                //multiply by inverse of direction difference to weigh closer things more heavily
                x_influence += x_diff/mag * Mathf.Clamp(1.0f/x_diff * 1.0f/x_diff, 0, 100);
                y_influence += y_diff/mag * Mathf.Clamp(1.0f/y_diff * 1.0f/y_diff, 0, 100); 

            }
        }
        // x_influence /= neighborhoodCount;
        // y_influence /= neighborhoodCount;
        if(x_influence == 0 && y_influence == 0)
            return 0;
        Vector2 inverseVectorFromThisToHeading = new Vector2(-1*x_influence, -1*y_influence);
        // Debug.Log(gameObject.name + inverseVectorFromThisToHeading);
        
        float desiredAngle = Mathf.Atan2(inverseVectorFromThisToHeading.y, inverseVectorFromThisToHeading.x) * Mathf.Rad2Deg;
        // Debug.Log(gameObject.name + desiredAngle);        

        if(desiredAngle - boidDefinition.heading >= -180 && desiredAngle - boidDefinition.heading <= 180)
            return desiredAngle - boidDefinition.heading;
        else if(desiredAngle - boidDefinition.heading < -180)
            return 360 + desiredAngle - boidDefinition.heading;
        else
            return desiredAngle - boidDefinition.heading - 360;
    }

    private float cohesion(List<BoidDefinition> neighborhood)
    {
        float centerOfMass_x = boidDefinition.position.x;
        float centerOfMass_y = boidDefinition.position.y;
        
        int neighborhoodCount = 1;
        foreach(BoidDefinition boidDef in neighborhood)
        {
            centerOfMass_x += boidDef.position.x;
            centerOfMass_y += boidDef.position.y;
        }
        centerOfMass_x /= neighborhoodCount;
        centerOfMass_y /= neighborhoodCount;
        

        
        float deltaAngle = (Vector2.Angle(this.boidDefinition.position, new Vector2(centerOfMass_x, centerOfMass_y))) - boidDefinition.heading; //desired heading - current heading

        
        return deltaAngle;
    }

    private List<BoidDefinition> getNeighborhood(List<BoidDefinition> boidDefinitions)
    {
        List<BoidDefinition> neighborhood = new List<BoidDefinition>();
        foreach(BoidDefinition boidDef in boidDefinitions)
        {     
            //skip yourself
            if(this.boidDefinition.id == boidDef.id)
                continue;  
            if(Vector2.Distance(boidDef.position, this.boidDefinition.position) < neighborhoodRadius)
            {
                neighborhood.Add(boidDef);
            }
        }
        return neighborhood;
    }

    private float accelSeparation(List<BoidDefinition> neighborhood)
    {
        //calculate relative velocities between all neighbors. 

        return 0.0f;
    }

    public void UpdatePosition(List<BoidDefinition> bds) {
        float heading = transform.eulerAngles.z * Mathf.Deg2Rad;
        float total_movement = movementSpeed * Time.deltaTime;

        transform.Translate(total_movement * Vector3.right);
        
        List<BoidDefinition> neighborhood = getNeighborhood(bds);
        float deltaAngle = separationWeight * separation(neighborhood) + alignmentWeight * alignment(neighborhood);
        // float deltaAngle = 0.0f;
        float deltaX = 0.0f;
        float deltaY = 0.0f;
        // float deltaAngle = separationWeight * separation(neighborhood);
        
        // float deltaAngle = alignmentWeight * alignment(neighborhood) + separationWeight * separation(neighborhood) + cohesionWeight * cohesion(neighborhood);
        float totalRotation = rotationSpeed * Time.deltaTime;

        deltaAngle = Mathf.Clamp(deltaAngle, -1*totalRotation, totalRotation);

        // Debug.Log("seapration = " + separation(neighborhood));
        // Debug.Log(deltaAngle);
        transform.Rotate(0,0,deltaAngle, Space.World);
        

        this.boidDefinition.position = new Vector2(transform.position.x, transform.position.y);
        this.boidDefinition.heading = this.boidDefinition.heading + deltaAngle;
        this.boidDefinition.velocity = new Vector2(this.boidDefinition.velocity.x + deltaX, this.boidDefinition.velocity.y + deltaY);
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

    void OnTriggerEnter(Collider collider)
    {   
        Debug.Log("Collision!");
    }

}
