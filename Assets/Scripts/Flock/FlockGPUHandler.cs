using System;
using UnityEngine;
using UnityEngine.Profiling;
using Utility;

public class FlockGPUHandler : FlockHandler
{
    [Header("Compute Shader Reference")]
    public ComputeShader flockComputeShader;

    private ComputeBuffer _velocitiesBuffer;
    private int 
        id_keepPositionKernel,
        id_updatePositionKernel;

    private static readonly int
        id_deltaTime = Shader.PropertyToID("delta_time"),

        id_numAgents = Shader.PropertyToID("num_agents"),
        id_agentPositions = Shader.PropertyToID("agent_positions"),
        id_agentVelocities = Shader.PropertyToID("agent_velocities"),

        id_agentSizeRadius = Shader.PropertyToID("agent_size_radius"),
        id_agentViewRange = Shader.PropertyToID("agent_view_range"),

        id_flockCenter = Shader.PropertyToID("flock_center"),
        id_driveFactor = Shader.PropertyToID("drive_factor"),

        id_avoidanceParams = Shader.PropertyToID("avoidance_params"),
        id_cohesionParams = Shader.PropertyToID("cohesion_params"),
        id_alignmentParams = Shader.PropertyToID("alignment_params"),
        id_boundingSphereParams = Shader.PropertyToID("bounding_sphere_params");

    private void Awake()
    {
        id_keepPositionKernel = flockComputeShader.FindKernel("k_KeepPosition");
        id_updatePositionKernel = flockComputeShader.FindKernel("k_UpdatePosition");
    }

    protected override void InitializeData()
    {
        // Initialize starting positions with random values inside the range of the flock
        positionsBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
        positionsBuffer.SetData(MyMath.RandomPointsInSphere(flock.numAgents, center, flock.flockRadius));
        _velocitiesBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
    }

    protected override void DisposeData()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
        
        _velocitiesBuffer.Release();
        _velocitiesBuffer = null;
    }

    protected override void UpdatePositionsAndVelocities()
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
    }
    
    private void SetComputeShaderVariables(int kernelId)
    {
        // Global memory variables
        flockComputeShader.SetFloats(id_flockCenter, center.x, center.y, center.z);
        flockComputeShader.SetFloat(id_driveFactor, flock.driveFactor);
        flockComputeShader.SetFloat(id_deltaTime, Time.deltaTime);
        flockComputeShader.SetFloat(id_agentSizeRadius, flock.AgentRadius);
        flockComputeShader.SetFloat(id_agentViewRange, flock.EffectiveViewRange);
        
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
        flockComputeShader.SetBuffer(kernelId, id_agentPositions, positionsBuffer);
        flockComputeShader.SetBuffer(kernelId, id_agentVelocities, _velocitiesBuffer);
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
}