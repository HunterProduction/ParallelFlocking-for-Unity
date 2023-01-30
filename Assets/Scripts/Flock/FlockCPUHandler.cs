using UnityEngine;
using UnityEngine.Profiling;

public class FlockCPUHandler : FlockHandler
{
    private Vector3[] _agentPositions;
    private Vector3[] _agentVelocities;

    protected override void InitializeData()
    {
        positionsBuffer = new ComputeBuffer(flock.numAgents, 3 * sizeof(float));
        _agentPositions = Utility.MyMath.RandomPointsInSphere(flock.numAgents, center, flock.flockRadius);
        _agentVelocities = new Vector3[flock.numAgents];
    }

    protected override void DisposeData()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
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
    
    protected override void UpdatePositionsAndVelocities()
    {
        Profiler.BeginSample("FlockCPUHandler: Computations");
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
            c_distanceVector = c_currentPosition - center;
            c_distanceMagnitude = c_distanceVector.magnitude;
            if (flock.boundingSphereParameters.active
                && c_distanceMagnitude > flock.boundingSphereParameters.radius)
            {
                c_boundingSphereMove = (flock.boundingSphereParameters.radius - c_distanceMagnitude) *
                                       c_distanceVector.normalized;
            }
            
            // Normalize moves
            if (c_nAlignment > 0)
                c_alignmentMove /= c_nAlignment;
            if (c_nCohesion > 0)
                c_cohesionMove /= c_nCohesion;
            
            // Sum all behaviours weights
            float weightSum = flock.avoidanceParameters.weight*flock.avoidanceParameters.IsActive +
                              flock.cohesionParameters.weight*flock.cohesionParameters.IsActive +
                              flock.alignmentParameters.weight*flock.alignmentParameters.IsActive +
                              flock.boundingSphereParameters.weight*flock.alignmentParameters.IsActive;
            
            // Calculate weighted sum of all movement vectors
            c_totMove = (
                c_alignmentMove * (flock.alignmentParameters.weight*flock.alignmentParameters.IsActive) +
                c_avoidanceMove * (flock.avoidanceParameters.weight * flock.avoidanceParameters.IsActive) +
                c_cohesionMove * (flock.cohesionParameters.weight*flock.cohesionParameters.IsActive) +
                c_boundingSphereMove * (flock.boundingSphereParameters.weight*flock.alignmentParameters.IsActive)
            ) / weightSum;
            
            // Update position and velocity
            _agentVelocities[i] = c_totMove;
            _agentPositions[i] = c_currentPosition + c_totMove * Time.unscaledDeltaTime;
        }
        
        // Set Positions Buffer
        positionsBuffer.SetData(_agentPositions);
        Profiler.EndSample();
    }
}
