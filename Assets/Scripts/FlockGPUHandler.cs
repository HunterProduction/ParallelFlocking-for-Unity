using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Utility;

public class FlockGPUHandler : MonoBehaviour
{
    [Header("Compute Shader Reference")]
    public ComputeShader flockComputeShader;

    public Flock flock;

    private ComputeBuffer _positionsBuffer, _velocitiesBuffer;
    private int 
        id_keepPositionKernel,
        id_updatePositionKernel;

    private static readonly int
        id_time = Shader.PropertyToID("time"),
        id_deltaTime = Shader.PropertyToID("delta_time"),

        id_numAgents = Shader.PropertyToID("num_agents"),
        id_agentPositions = Shader.PropertyToID("agent_positions"),
        id_agentVelocities = Shader.PropertyToID("agent_velocities"),

        id_agentSizeRadius = Shader.PropertyToID("agent_size_radius"),
        id_agentViewRange = Shader.PropertyToID("agent_view_range"),
        id_agentFov = Shader.PropertyToID("agent_fov"),
        
        id_flockCenter = Shader.PropertyToID("flock_center"),
        id_flockRadius = Shader.PropertyToID("flock_radius"),
        id_driveFactor = Shader.PropertyToID("drive_factor"),
        
        id_avoidanceParams = Shader.PropertyToID("avoidance_params"),
        id_cohesionParams = Shader.PropertyToID("cohesion_params"),
        id_alignmentParams = Shader.PropertyToID("alignment_params"),
        id_boundingSphereParams = Shader.PropertyToID("bounding_sphere_params"),

        id_debugDistances = Shader.PropertyToID("debug_distances");

    private ComputeBuffer _debug_distancesBuffer;

    private Vector3 _center;

    private void OnValidate()
    {
        // NOTE: Cohesion and Alignment radius are a "fake" parameter: they're bound to the Agents view range.
        flock.cohesionParameters.radius = flock.agentViewRange;
        flock.alignmentParameters.radius = flock.agentViewRange;
        //NormalizeBehaviourWeights();
    }

    private void Awake()
    {
        //NormalizeBehaviourWeights();
        id_keepPositionKernel = flockComputeShader.FindKernel("k_KeepPosition");
        id_updatePositionKernel = flockComputeShader.FindKernel("k_UpdatePosition");
    }

    private void OnEnable()
    {
        _center = flock.Origin.position;
        
        // Initialize starting positions with random values inside the range of the flock
        
        _positionsBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
        _positionsBuffer.SetData(MyMath.RandomPointsInSphere(flock.numAgents, _center, flock.flockRadius));
        _velocitiesBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
        //_velocitiesBuffer.SetData(RandomPointsInSphere(flock.numAgents, Vector3.zero, 0.1f));

        _debug_distancesBuffer = new ComputeBuffer(flock.numAgents, sizeof(float));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.95f, 1f, 0.2f);
        _center = flock.Origin.position;
        if (_center == null) return;
        Gizmos.DrawSphere(_center, flock.flockRadius);
        Gizmos.color = new Color(0.28f, 1f, 0.13f, 0.12f);
        Gizmos.DrawSphere(_center, flock.boundingSphereParameters.radius);
    }

    private void Update()
    {
        _center = flock.Origin.position;
        //NormalizeBehaviourWeights();
        UpdateGPU();
    }

    private void UpdateGPU()
    {
        // Setup
        Profiler.BeginSample("FlockGPUHandler: Setup CS Variables");
        SetComputeShaderVariables(id_updatePositionKernel);
        Profiler.EndSample();
        
        // Dispatch
        Profiler.BeginSample("FlockGPUHandler: Kernel Dispatch");
        var groups = flock.numAgents > 64 ? (int)Math.Ceiling(flock.numAgents / 64f) : 1;
        flockComputeShader.Dispatch(id_updatePositionKernel, groups, 1, 1);
        Profiler.EndSample();
        
        // Debug
        //DebugActions();
        
        // Render
        DrawAgents();
    }
    
    private void OnDisable()
    {
        _positionsBuffer.Release();
        _positionsBuffer = null;
        
        _velocitiesBuffer.Release();
        _velocitiesBuffer = null;
        
        _debug_distancesBuffer.Release();
        _debug_distancesBuffer = null;
    }

    private void SetComputeShaderVariables(int kernelId)
    {
        // Global memory variables
        flockComputeShader.SetFloats(id_flockCenter, _center.x, _center.y, _center.z);
        flockComputeShader.SetFloat(id_flockRadius, flock.flockRadius);
        flockComputeShader.SetFloat(id_driveFactor, flock.driveFactor);
        flockComputeShader.SetFloat(id_time, Time.time);
        flockComputeShader.SetFloat(id_deltaTime, Time.deltaTime);
        flockComputeShader.SetFloat(id_agentSizeRadius, flock.AgentRadius);
        flockComputeShader.SetFloat(id_agentViewRange, flock.EffectiveViewRange);
        //flockComputeShader.SetFloat(id_agentFov, agentFOV);
        
        flockComputeShader.SetFloats(id_avoidanceParams,
            flock.avoidanceParameters.IsActive,
            flock.avoidanceParameters.weight,
            flock.avoidanceParameters.radius);
        
        flockComputeShader.SetFloats(id_cohesionParams,
            flock.cohesionParameters.IsActive,
            flock.cohesionParameters.weight);
        
        flockComputeShader.SetFloats(id_alignmentParams,
            flock.alignmentParameters.IsActive,
            flock.alignmentParameters.weight);
        
        flockComputeShader.SetFloats(id_boundingSphereParams,
            flock.boundingSphereParameters.IsActive,
            flock.boundingSphereParameters.weight,
            flock.boundingSphereParameters.radius);
        
        flockComputeShader.SetInt(id_numAgents, flock.numAgents);
        
        // Buffer
        flockComputeShader.SetBuffer(kernelId, id_agentPositions, _positionsBuffer);
        flockComputeShader.SetBuffer(kernelId, id_agentVelocities, _velocitiesBuffer);
        flockComputeShader.SetBuffer(kernelId, id_debugDistances, _debug_distancesBuffer);
    }

    private void DrawAgents()
    {
        flock.agentMaterial.SetBuffer(Shader.PropertyToID("_Positions"), _positionsBuffer);
        flock.agentMaterial.SetFloat(Shader.PropertyToID("_Scale"), flock.agentScale);
        var bounds = new Bounds(_center, Vector3.one * (flock.flockRadius * 10f));
        Graphics.DrawMeshInstancedProcedural(
            flock.agentMesh, 0, flock.agentMaterial, bounds, _positionsBuffer.count);
    }

    /*private void NormalizeBehaviourWeights()
    {
        float tot = (!avoidanceParameters.fixedWeight ? avoidanceParameters.weight : 0)
                    + (!cohesionParameters.fixedWeight ? cohesionParameters.weight : 0)
                    + (!alignmentParameters.fixedWeight ? alignmentParameters.weight : 0)
                    + (!boundingSphereParameters.fixedWeight ? boundingSphereParameters.weight : 0);
        float fixedTot = (avoidanceParameters.fixedWeight ? avoidanceParameters.weight : 0)
                         + (cohesionParameters.fixedWeight ? cohesionParameters.weight : 0)
                         + (alignmentParameters.fixedWeight ? alignmentParameters.weight : 0)
                         + (boundingSphereParameters.fixedWeight ? boundingSphereParameters.weight : 0);
        if (tot == 0) return;
        if(!avoidanceParameters.fixedWeight) avoidanceParameters.weight = avoidanceParameters.weight / tot * (100-fixedTot);
        if(!cohesionParameters.fixedWeight) cohesionParameters.weight = cohesionParameters.weight / tot * (100-fixedTot);
        if(!alignmentParameters.fixedWeight) alignmentParameters.weight = alignmentParameters.weight / tot * (100-fixedTot);
        if(!boundingSphereParameters.fixedWeight) boundingSphereParameters.weight = boundingSphereParameters.weight / tot * (100-fixedTot);
    }*/

    private void DebugActions()
    {
        int i = 0;
        float[] distances = new float[flock.numAgents];
        Vector3[] positions = new Vector3[flock.numAgents];
        _debug_distancesBuffer.GetData(distances);
        _positionsBuffer.GetData(positions);
        for (i = 0; i < flock.numAgents; i++)
        {
            Debug.Log($"Particle {i}: Position {positions[i]} - Distance {distances[i]}");
        }
        
    }
}