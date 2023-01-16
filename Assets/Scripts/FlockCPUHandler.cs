using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class FlockCPUHandler : MonoBehaviour
{
    public Flock flock;

    private ComputeBuffer _positionsBuffer;
    private Vector3 _center;

    private Vector3[] _agentPositions;
    private Vector3[] _agentVelocities;
    
    private void OnValidate()
    {
        // NOTE: Cohesion and Alignment radius are a "fake" parameter: they're bound to the Agents view range.
        flock.cohesionParameters.radius = flock.agentViewRange;
        flock.alignmentParameters.radius = flock.agentViewRange;
        //NormalizeBehaviourWeights();
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

    private void OnEnable()
    {
        _center = flock.Origin.position;
        _positionsBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
        _agentPositions = Utility.MyMath.RandomPointsInSphere(flock.numAgents, _center, flock.flockRadius);
        _agentVelocities = new Vector3[flock.numAgents];
    }

    public Vector3[] MyMath { get; set; }

    private void Update()
    {
        Profiler.BeginSample("FlockCPUHandler: Computation");
        _center = flock.Origin.position;
        UpdatePositionCPU();
        Profiler.EndSample();
        Profiler.BeginSample("FlockCPUHandler: SetBuffer and Draw");
        _positionsBuffer.SetData(_agentPositions);
        DrawAgents();
        Profiler.EndSample();
    }
    
    private void OnDisable()
    {
        _positionsBuffer.Release();
        _positionsBuffer = null;
    }

    private void DrawAgents()
    {
        flock.agentMaterial.SetBuffer(Shader.PropertyToID("_Positions"), _positionsBuffer);
        flock.agentMaterial.SetFloat(Shader.PropertyToID("_Scale"), flock.agentScale);
        var bounds = new Bounds(_center, Vector3.one * 100);
        Graphics.DrawMeshInstancedProcedural(
            flock.agentMesh, 0, flock.agentMaterial, bounds, _positionsBuffer.count);
    }
    
    private Vector3 c_currentPosition;
    
    private Vector3 c_distanceVector;
    private float c_distanceMagnitude;
    private Vector3 c_distanceDirection;

    private int c_nAvoid;
    private Vector3 c_avoidanceMove;

    private int c_nCohesion;
    private Vector3 c_cohesionMove;
    
    private int c_nAlignment;
    private Vector3 c_alignmentMove;
    private float c_neighbourVelocityMagnitude;

    private Vector3 c_boundingSphereMove;

    private Vector3 c_totMove;
    
    private void UpdatePositionCPU()
    {
        for (int i = 0; i < flock.numAgents; i++)
        {
            c_currentPosition = _agentPositions[i];

            c_nAvoid = 0;
            c_avoidanceMove = Vector3.zero;

            c_nCohesion = 0;
            c_cohesionMove = Vector3.zero;

            c_nAlignment = 0;
            c_alignmentMove = Vector3.zero;

            for (int j = 0; j < flock.numAgents; j++)
            {
                if (j == i)
                    continue;
                
                c_distanceVector = _agentPositions[j] - c_currentPosition;
                c_distanceMagnitude = c_distanceVector.magnitude;
                c_distanceDirection = c_distanceVector.normalized;

                c_distanceMagnitude -= 2 * flock.AgentRadius;
                if (c_distanceMagnitude <= 0.01f) c_distanceMagnitude = 0.01f;

                if (c_distanceMagnitude < flock.EffectiveViewRange)
                {
                    // Calculate Avoidance move
                    if (flock.avoidanceParameters.active
                        && c_distanceMagnitude < flock.avoidanceParameters.radius)
                    {
                        c_nAvoid++;
                        c_avoidanceMove += -10 * (flock.avoidanceParameters.radius - c_distanceMagnitude) *
                                           c_distanceDirection;
                    }
                    
                    // Calculate Cohesion move
                    if (flock.cohesionParameters.active
                        && c_distanceMagnitude > 0.01f)
                    {
                        c_nCohesion++;
                        c_cohesionMove += c_distanceMagnitude * c_distanceDirection;
                    }
                    
                    // Calculate Alignment move
                    if (flock.alignmentParameters.active)
                    {
                        c_neighbourVelocityMagnitude = _agentVelocities[j].magnitude;
                        if (c_neighbourVelocityMagnitude > 0f)
                        {
                            c_nAlignment++;
                            c_alignmentMove += _agentVelocities[j] / c_neighbourVelocityMagnitude * flock.driveFactor;
                        }
                    }
                }
            }
            
            // Calculate Bounding Sphere move
            c_boundingSphereMove = Vector3.zero;
            c_distanceVector = c_currentPosition - _center;
            c_distanceMagnitude = c_distanceVector.magnitude;
            if (flock.boundingSphereParameters.active
                && c_distanceMagnitude > flock.boundingSphereParameters.radius)
            {
                c_boundingSphereMove = 10 * (flock.boundingSphereParameters.radius - c_distanceMagnitude) *
                                       c_distanceVector.normalized;
            }
            
            // Normalize moves
            if (c_nAlignment > 0)
                c_alignmentMove /= c_nAlignment;
            if (c_nCohesion > 0)
                c_cohesionMove /= c_nCohesion;
            
            // Sum all behaviours weights
            float weightSum = flock.avoidanceParameters.weight +
                              flock.cohesionParameters.weight +
                              flock.alignmentParameters.weight +
                              flock.boundingSphereParameters.weight;
            
            // Calculate weighted sum of all movement vectors
            c_totMove = (
                c_alignmentMove * flock.alignmentParameters.radius +
                c_avoidanceMove * flock.avoidanceParameters.weight +
                c_cohesionMove * flock.cohesionParameters.weight +
                c_boundingSphereMove * flock.boundingSphereParameters.weight
            ) / weightSum;
            
            // Update position and velocity
            _agentVelocities[i] = c_totMove;
            _agentPositions[i] = c_currentPosition + c_totMove * Time.unscaledDeltaTime;
        }
    }
}
