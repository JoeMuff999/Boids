using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct BoidDefinition{
    public Vector2 position;
    public float heading;

    public BoidDefinition(float x, float y, float heading)
    {
        position = new Vector2(x,y);
        this.heading = heading;
    }

    public BoidDefinition(BoidDefinition bp)
    {
        this.position = new Vector2(bp.position.x, bp.position.y);
        this.heading = bp.heading;
    }
}


public class AgentManager : MonoBehaviour
{
    private static List<Agent> agents;    
    private static List<BoidDefinition> currentStates;

    public int NumBoids;

    public GameObject BoidPrefab;

    private void Awake() {
        agents = new List<Agent>();
        currentStates = new List<BoidDefinition>();
    }

    private void Start() {
        Bounds bounds = CameraUtility.GetCameraBounds(Camera.main);
        for(int i = 0; i < NumBoids; i++)
        {
            float initial_x = Random.Range(bounds.min.x, bounds.max.x);
            float initial_y = Random.Range(bounds.min.y, bounds.max.y);
            Quaternion intial_rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
            Instantiate(BoidPrefab, new Vector3(initial_x, initial_y, 0), intial_rotation);
        }
    }

    public static void RegisterAgent(Agent agent)
    {
        agents.Add(agent);
        currentStates.Add(new BoidDefinition(agent.boidDefinition));
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