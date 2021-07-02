using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct BoidDefinition{
    public Vector2 position;
    public float heading;
    public Vector2 velocity;
    public int id;

    public BoidDefinition(float x, float y, float heading, float speed)
    {
        position = new Vector2(x,y);
        this.heading = heading;
        this.velocity = new Vector2(speed * Mathf.Cos(Mathf.Deg2Rad * heading), speed * Mathf.Sin(Mathf.Deg2Rad * heading));
        id = -1; //leave unassigned (must be assigned when registered into manager!)
    }
    //I don't like this -> potentially having an unassigned agent id
    public BoidDefinition(BoidDefinition bp)
    {
        this.position = new Vector2(bp.position.x, bp.position.y);
        this.heading = bp.heading;
        this.velocity = bp.velocity;
        this.id = bp.id;
    }
    public BoidDefinition(BoidDefinition bp, int id)
    {

        this.position = new Vector2(bp.position.x, bp.position.y);
        this.heading = bp.heading;
        this.velocity = bp.velocity;
        this.id = id;
    }
}


public class AgentManager : MonoBehaviour
{
    private static List<Agent> agents;    
    private static List<BoidDefinition> currentStates;

    public int NumBoids;
    
    //used to give a unique id to each agent in the sim
    private static int agentCount = 0;

    public GameObject BoidPrefab;

    private void Awake() {
        agents = new List<Agent>();
        currentStates = new List<BoidDefinition>();
    }

    private void Start() {
        Bounds bounds = CameraUtility.GetCameraBounds(Camera.main);
        float min_separation = BoidPrefab.GetComponent<Agent>().separationRadius * 2;
        int max_attempts = 40;
        int boids_spawned = 0;
        List<Vector2> placedSoFar = new List<Vector2>();
        for(int i = 0; i < NumBoids; i++)
        {   
            int current_attempts = 0;

            bool foundSpot = false;
            float initial_x = 0;
            float initial_y = 0;
            while(!foundSpot && current_attempts < max_attempts)
            {
                initial_x = Random.Range(bounds.min.x, bounds.max.x);
                initial_y = Random.Range(bounds.min.y, bounds.max.y);
                Vector2 current = new Vector2(initial_x, initial_y);
                foundSpot = true;
                foreach(Vector2 toCheck in placedSoFar)
                {
                    if(Vector2.Distance(toCheck, current) < min_separation)
                    {
                        foundSpot = false;
                        break;
                    }
                }
                current_attempts++;
            }
            if(foundSpot)
            {
                Quaternion intial_rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
                GameObject go = Instantiate(BoidPrefab, new Vector3(initial_x, initial_y, 0), intial_rotation);
                placedSoFar.Add(new Vector2(initial_x, initial_y));
                boids_spawned++;
            }
        }
        Debug.Log("Boids spawned " + boids_spawned);
    }

    public static void RegisterAgent(Agent agent)
    {
        agent.boidDefinition = new BoidDefinition(agent.boidDefinition, agentCount);//assign id here
        agents.Add(agent);
        currentStates.Add(new BoidDefinition(agent.boidDefinition));
        // Debug.Log(currentStates[currentStates.Count-1].id);
        agentCount++;
    }

    private void Update() {
        foreach(Agent agent in agents)
        {
            agent.UpdatePosition(currentStates);
        }
        currentStates = new List<BoidDefinition>();
        foreach(Agent agent in agents)
        {
            currentStates.Add(new BoidDefinition(agent.boidDefinition));
        }
    }   
}