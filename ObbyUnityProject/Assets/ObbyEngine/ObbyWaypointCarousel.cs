using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WaypointLoopType {
    PingPong,
    Loop
}

public enum WaypointSpacingType {
    Max,
    Custom
}

public class ObbyWaypointCarousel : MonoBehaviour
{
    public List<Transform> waypoints;
    [HideInInspector] public List<Transform> waypointsInternal = new();
    public List<Transform> platforms;
    public float speed;
    public WaypointLoopType loopType;
    public bool pauseAtEnds;
    public float pauseTime;
    public WaypointSpacingType spacingType;
    public float customSpacing;
    [HideInInspector] public float maxSpacing;
    private List<float> _platformPositions = new();
    private List<int> _platformWaypointIndices = new();
    private List<float> _waypointDistSumarray = new();
    private float _maxPosition;
    private float _pauseTimer;

    private void Start() {
        OnValidate();
    }
    
    private void Update() {
        // if the pause timer is active, don't do anything
        if (_pauseTimer > 0) {
            _pauseTimer -= Time.deltaTime;
            return;
        }
        
        float increment = Time.deltaTime * speed;

        // if pingpong and reached end, flip direction
        if (loopType == WaypointLoopType.PingPong && (_platformPositions.Last() + increment > _maxPosition || _platformPositions.First() + increment < 0)) {
            speed *= -1;
            increment *= -1;

            // if pause at ends is on then we start the pause now
            if (pauseAtEnds) {
                _pauseTimer = pauseTime;
                return;
            }
        }

        for (int i = 0; i < _platformPositions.Count; i++) {
            _platformPositions[i] += increment;

            // take modulo to keep in range if loop and reset to first waypoint
            if (loopType == WaypointLoopType.Loop && _platformPositions[i] > _maxPosition) {
                _platformPositions[i] %= _maxPosition;
                _platformWaypointIndices[i] = 0;
            }
        }

        SyncPosition();
    }
    
    public void SyncPosition() {
        for (int i = 0; i < platforms.Count; i++) {
            // update waypoint index if position is past dist of waypoint
            if (_platformPositions[i] > _waypointDistSumarray[_platformWaypointIndices[i] + 1]) {
                _platformWaypointIndices[i]++;
            }
            else if (_platformPositions[i] < _waypointDistSumarray[_platformWaypointIndices[i]]) {
                _platformWaypointIndices[i]--;
            }

            int waypointIdx = _platformWaypointIndices[i];
            float dist = _waypointDistSumarray[waypointIdx + 1] - _waypointDistSumarray[waypointIdx];
            float localPos = _platformPositions[i] - _waypointDistSumarray[waypointIdx];
            Vector3 posA = waypointsInternal[waypointIdx].position;
            Vector3 posB = waypointsInternal[waypointIdx + 1].position;
            platforms[i].position = Vector3.Lerp(posA, posB, localPos / dist);
        }
    }

    //FIXME: probably abusing OnValidate here
    private void OnValidate() {
        if (waypoints.Count < 2) {
            Debug.LogError("WaypointCarousel requires more than one waypoint!");
            return;
        }
        
        // Add the first waypoint to the end of list if loop type is set to loop
        if (waypointsInternal.Count < waypoints.Count)
            waypointsInternal.AddRange(waypoints);

        if (loopType == WaypointLoopType.Loop && waypointsInternal.Count == waypoints.Count)
            waypointsInternal.Add(waypointsInternal.First());
        else if (loopType == WaypointLoopType.PingPong && waypointsInternal.Count > waypoints.Count) 
            waypointsInternal.RemoveAt(waypointsInternal.Count - 1);
        
        // populate distance sum array and maxposition
        _waypointDistSumarray = new List<float> { 0 };
        for (int i = 1; i < waypointsInternal.Count; i++) {
            float dist = Vector3.Distance(waypointsInternal[i - 1].position, waypointsInternal[i].position);
            _waypointDistSumarray.Add(dist + _waypointDistSumarray[i - 1]);
        }
        _maxPosition = _waypointDistSumarray.Last();

        maxSpacing = _maxPosition / platforms.Count;
        customSpacing = Mathf.Clamp(customSpacing, 0, maxSpacing);

        float spacePerPlatform = spacingType switch
        {
            WaypointSpacingType.Max => maxSpacing,
            WaypointSpacingType.Custom => customSpacing,
            _ => throw new System.NotImplementedException()
        };

        // Initialize positions based on spacing
        _platformPositions.Clear();
        _platformWaypointIndices.Clear();
        for (int i = 0; i < platforms.Count; i++) {
            _platformPositions.Add(spacePerPlatform * i);
            
            // position[i] is between sum of waypoint dists at waypoints j and j+1
            for (int j = 0; j < _waypointDistSumarray.Count; j++) {
                if (_platformPositions[i] < _waypointDistSumarray[j]) {
                    _platformWaypointIndices.Add(j - 1);
                    break;
                }
            }
        }

        // only allow pause at ends for pingpong
        if (loopType != WaypointLoopType.PingPong) {
            pauseAtEnds = false;
        }

        speed = Mathf.Clamp(speed, 0, 100);
        pauseTime = Mathf.Abs(pauseTime);

        // Sync position platforms while in editor
        SyncPosition();
    }
}
