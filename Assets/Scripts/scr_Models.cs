
using UnityEngine;
using System;

public static class scr_Models 
{
    public enum PlayerStance 
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("Viev Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed ;
        public float WalkingBackwardSpeed ;
        public float WalkingStrafeSpeed ;

        [Header("Movement - Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Settings")]
        public bool SprintingHold;
        public float MovementSmoothnig;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;        
    }

    [Serializable]
    public class CharacterStance 
    {
        public float CameraHeight;
        public CapsuleCollider StanceColider;
    }

    public enum WeaponFireType 
    {
        SemiAuto,
        FullAuto,
        Save       
    }
    
    [Serializable]
    public class WeaponSettingsModel 
    {
        [Header("Weapon Sway")]
        public float SwayAmount;
        public bool SwayYInverted;
        public bool SwayXInverted;
        public float SwaySmoothing;
        public float SwayResetSmoothing;
        public float SwayClampX;
        public float SwayClampY;

        [Header("Weapon Movement")]
        public float MovementSwayX;
        public float MovementSwayY;
        public bool MovementSwayXInverted;
        public bool MovementSwayYInverted;
        public float MovementSwaySmoothing;
    }
}
