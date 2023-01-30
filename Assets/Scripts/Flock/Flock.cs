using System;
using UnityEngine;

[Serializable]
public struct BehaviourParameters
{
    public bool active;
    public float IsActive => active ? 1f : 0f;

    [Range(0,100)]
    public float weight;
    //public bool fixedWeight;
    public float radius;
    
}

[Serializable]
public class Flock
{
    [Header("Flock Parameters")] 
    public float flockRadius = 10f; 
    [SerializeField] private Transform origin;
    public int numAgents = 50;
    public float driveFactor = 1f;

    public float agentViewRange = 1f;
    //public int agentFOV = 270;

    public BehaviourParameters avoidanceParameters;
    public BehaviourParameters cohesionParameters;
    public BehaviourParameters alignmentParameters;
    public BehaviourParameters boundingSphereParameters;

    [Header("Agent Parameters")] 
    public Mesh agentMesh;
    public Material agentMaterial;
    public float agentScale = 1f;

    public Transform Origin => origin;
    public float AgentRadius => agentMesh!=null ? MaxBoundingSphereRadius(agentMesh) : 0;
    public float EffectiveViewRange => agentViewRange + AgentRadius;

    public void SetOrigin(Transform newOriginReference)
    {
        origin = newOriginReference;
    }
    
    private float MaxBoundingSphereRadius(Mesh mesh)
    {
        Vector3 extents = mesh.bounds.extents;
        return Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
    }
}