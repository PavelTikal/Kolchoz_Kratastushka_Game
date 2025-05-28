
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static scr_Models;



public class scr_WeaponController : MonoBehaviour
{
    private scr_CharacterController characterController;

    [Header("References")]
    public Animator weaponAnimator;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    [Header("Settings")]
    public WeaponSettingsModel settings;

    bool isInitialized;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    [Header("Weapon Breathing")]
    public Transform weaponSwayObject;
    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;

    public float swayTime;
    public Vector3 swayPosition;

    public bool isAimingIn;

    [Header("Sights")]
    public Transform sightTarget;
    public float sightOffSet;
    public float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;

    [Header("Shooting")]
    public float rateOfFire;
    private float currentFireRate;
    public List<WeaponFireType> allowedFireTypes;
    public static WeaponFireType currentFireType;
    public bool isShooting;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 10f;
    public static float bulletsInMagazine = 30;
    private bool isFullAutoShooting = false;
     
    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;

        currentFireType = allowedFireTypes.First();
        currentFireRate = 0.000000000000000000000000001f / rateOfFire;
    }

    public void Initialize(scr_CharacterController CharacterController) 
    {
        characterController = CharacterController;
        isInitialized = true;
    }
    private void Update()
    {
        if (!isInitialized) 
        {
            return;
        }
        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        CalculateShooting();
        
    }
    private void CalculateShooting() 
    {
        if (isShooting) 
        {
            Shoot();

            if (currentFireType.Equals(WeaponFireType.SemiAuto))
            {
                isShooting = false;
            }
            else if (currentFireType.Equals(WeaponFireType.FullAuto)) 
            {
        
                StartCoroutine(FullAutoShooting());
            }
        }
    }
    private IEnumerator FullAutoShooting() 
    {
        isFullAutoShooting = true;
        while (isShooting && bulletsInMagazine > 0)
        {
            
            yield return new WaitForSeconds(currentFireRate);
            Shoot();
        }
        isFullAutoShooting = false;
    }
   
    private void Shoot() 
    {
        
        if (bulletsInMagazine > 0 )
        {
            if (currentFireType.Equals(WeaponFireType.Save))
            {
                return;
            }
           
            var bullet = Instantiate(bulletPrefab,bulletSpawn.position,Quaternion.identity); // misto rotation/Quaterion bulletSpawn.rotation

            bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse) ;

            StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime + 5));
            bulletsInMagazine -= 1;
        }
    }
    private IEnumerator DestroyBulletAfterTime(GameObject bullet,float delay ) 
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    private void CalculateAimingIn() 
    {
        var targetPosition = transform.position;
        if (isAimingIn) 
        {
            targetPosition = characterController.cameraHolder.transform.position + 
                (weaponSwayObject.transform.position - sightTarget.position) + (characterController.cameraHolder.transform.forward * sightOffSet);

        }
        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition,ref weaponSwayPositionVelocity, aimingInTime);

        weaponSwayObject.transform.position = weaponSwayPosition;
    }

    private void CalculateWeaponRotation() 
    {
        weaponAnimator.speed = characterController.weaponAnimationSpeed;
        //settings.SwayAmount
        targetWeaponRotation.y += (isAimingIn ? settings.SwayAmount/2 : settings.SwayAmount) * (settings.SwayXInverted ? - characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;        
        targetWeaponRotation.x += (isAimingIn ? settings.SwayAmount/2 : settings.SwayAmount) * (settings.SwayYInverted ? characterController.input_View.y : - characterController.input_View.y) * Time.deltaTime;
        
        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        //targetWeaponRotation.y
        targetWeaponRotation.z = isAimingIn ? 0: targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation,Vector3.zero,ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation,targetWeaponRotation,ref newWeaponRotationVelocity, settings.SwaySmoothing);

        targetWeaponMovementRotation.z = settings.MovementSwayX * (settings.MovementSwayXInverted ? - characterController.input_Movement.x: characterController.input_Movement.x);
        targetWeaponMovementRotation.x = settings.MovementSwayY * (settings.MovementSwayYInverted ? - characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.SwayResetSmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.SwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations() 
    {
        weaponAnimator.SetBool("IsSprinting", characterController.isSprinting);
    }

    private void CalculateWeaponSway() 
    {
        if (swayScale < 1)
        {
            swayScale = 1;
        }
        var targetPosition = LissajousCurve(swayTime,swayAmountA,swayAmountB) /(isAimingIn ? swayScale * 3 : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime* swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f) 
        {
            swayTime = 0;
        }
    }

    private Vector3 LissajousCurve(float Time, float A, float B) 
    {
        return new Vector3(Mathf.Sin(Time),A * Mathf.Sin(B * Time + Mathf.PI));
    }
    public static string GetAmmoAndFireMode()
    {
        if (bulletsInMagazine == 0)
            return "Stiskni R pro reload";
        return "Munice: " + bulletsInMagazine.ToString() + "\n" +"Fire Mode: " + currentFireType;
    }
}
