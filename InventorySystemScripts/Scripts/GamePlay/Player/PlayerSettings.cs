using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game Feel for player physics and movement
/// </summary>
public class PlayerSettings : MonoBehaviour
{
    [Serializable]
    public class GroundedSettings
    {
        [Header("Walk")]
        public float WalkSpeed = 4f;

        [Header("Run")]
        public float RunSpeed = 7f;
        public float TimeToStartRunning = 1f;

        [Header("RunThreshold")]
        public float RunInputThreshold = 0.7f;

        [Header("Acceleration")]
        public float Acceleration = 50f;
        public float Deacceleration = 100f;
    }

    [Serializable]
    public class InAirSettings
    {
        [Header("Jump")]
        public float JumpForce = 15f;
        public int MaxJump = 1;

        [Header("Move in the air")]
        public float AirAcceleration = 25f;
        public float MaxAirSpeed = 5f;

        [Header("Gravity")]
        public float GravityScale = 3f;                 // Basic gravity
        public float FallGravityMultiplier = 1.5f;      // Extra gravity multiplier when falling

        [Header("Game Feel")]
        public float CoyoteTime = 0.1f;
        public float JumpBuffer = 0.1f;
    }
    // TODO: InAirSetting, Dash, Jump...

    [Serializable]
    public class DetectionSettings
    {
        [Header("Grounded Detection")]
        public LayerMask GroundedLayer;
        public float GroundedCheckDistance = 0.1f;
        public float GroundedCheckWidth = 0.8f;
    }

    public GroundedSettings Grounded = new GroundedSettings();
    public InAirSettings InAir = new InAirSettings();
    public DetectionSettings Detection = new DetectionSettings();
}
