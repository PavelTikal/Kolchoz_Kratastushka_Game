
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    private CharacterController characterController;
    private DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 input_View;
    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYmin = -70;
    public float viewClampYmax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    public float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    public LayerMask playerMask;
    public bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public scr_WeaponController currentWeapon;

    public float weaponAnimationSpeed;

    [Header("Leaning")]
    public Transform leanPivot;
    private float currentLean;
    private float targetLean;
    public float leanAngle;
    public float leanSmoothnig;
    private float leanVelocity;

    private bool isLeaningLeft;
    private bool isLeaningRight;

    [Header("Aiming In")]
    public bool isAimingIn;
    private float fTypeCount;    

    private void Awake()
    {
        defaultInput = new DefaultInput();
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();

        defaultInput.Character.Sprint.performed += e => ToogleSprint();
        defaultInput.Character.SprintReleased.performed += e => StopSprint();

        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleaseded();

        defaultInput.Character.LeanLeftPressed.performed += e => isLeaningLeft = true;
        defaultInput.Character.LeanLeftReleased.performed += e => isLeaningLeft = false;

        defaultInput.Character.LeanRightPressed.performed += e => isLeaningRight = true;
        defaultInput.Character.LeanRightReleased.performed += e => isLeaningRight = false;

        defaultInput.Weapon.Fire1Pressed.performed += e => ShootingPressed();
        defaultInput.Weapon.Fire1Released.performed += e => ShootingReleased();

        defaultInput.Weapon.Reload.performed += e => ReloadWeapon();
        
        defaultInput.Weapon.SwitchFireType.performed += e =>
        {
            SwitchFireType();
        };
        
        defaultInput.Enable();
        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;
        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;
        if (currentWeapon)
        {
            currentWeapon.Initialize(this);
        }
    }
    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
        CalculateLeaning();
        CalculateAimingIn();
    }
    private void Start()
    {
        if (currentWeapon)
        {
            currentWeapon.Initialize(this);
        }
    }
    private void SwitchFireType()
    {
        fTypeCount = (fTypeCount + 1) % 3;

        if (fTypeCount == 0)
        {
            scr_WeaponController.currentFireType = WeaponFireType.SemiAuto;
        }
        else if (fTypeCount == 1)
        {
            scr_WeaponController.currentFireType = WeaponFireType.FullAuto;
        }
        else if (fTypeCount == 2)
        {
            scr_WeaponController.currentFireType = WeaponFireType.Save;
        }
    }
    private void ReloadWeapon()
    {
        StartCoroutine(ReloadWithDelay(1.5f));
    }
    private IEnumerator ReloadWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        scr_WeaponController.bulletsInMagazine = 30;
    }

    private void ShootingPressed()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = true;
        }
    }
    private void ShootingReleased()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = false;
        }
    }

    private void CalculateLeaning()
    {
        if (isLeaningLeft)
        {
            targetLean = leanAngle;
        }
        else if (isLeaningRight)
        {
            targetLean = -leanAngle;
        }
        else
        {
            targetLean = 0;
        }
        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothnig);

        leanPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
    }
    private void LeanLeft()
    {
        targetLean = leanAngle;
    }
    private void LeanRight()
    {
        targetLean = -leanAngle;
    }
    private void CalculateView()
    {
        //Horizontal -- netreba omezeni pres Math.Clamp
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        //Vertical
        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYmin, viewClampYmax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }
    private void CalculateMovement()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }
        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        weaponAnimationSpeed = characterController.velocity.magnitude / playerSettings.WalkingForwardSpeed;
        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }


        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime)
            , ref newMovementSpeedVelocity, playerSettings.MovementSmoothnig);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        if (playerGravity < -0.1f && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;
        characterController.Move(movementSpeed);
    }
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }
    private void CalculateStance()
    {
        var currentStance = playerStandStance;
        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }
        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceColider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceColider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void Jump()
    {
        if (!characterController.isGrounded || playerStance == PlayerStance.Prone)
        {
            return;
        }
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceColider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;
        }

        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
    }
    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceColider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.StanceColider.height))
        {
            return;
        }
        playerStance = PlayerStance.Crouch;
    }
    private void Prone()
    {
        playerStance = PlayerStance.Prone;
    }
    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    private void ToogleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
            return;
        }
        isSprinting = !isSprinting;
    }
    private void StopSprint()
    {
        if (playerSettings.SprintingHold)
        {
            isSprinting = false;
        }
    }
    private void AimingInPressed()
    {
        isAimingIn = true;
    }
    private void AimingInReleaseded()
    {
        isAimingIn = false;
    }
    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }
        currentWeapon.isAimingIn = isAimingIn;
    }
}
