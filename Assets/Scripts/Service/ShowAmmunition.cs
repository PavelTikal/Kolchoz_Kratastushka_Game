using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_WeaponController;
using TMPro;
public class ShowAmmunition : MonoBehaviour
{
    public TMP_Text Ammo;
    public scr_WeaponController currentWeapon;
    void Start()
    {
        Ammo = GetComponent<TextMeshPro>();
        UpdateTextMesh();
    }

    void Update()
    {
        UpdateTextMesh();   
    }

    private void UpdateTextMesh() 
    {
        if (MissionEnvScript.lastKillPlayed)
        {
            Ammo.text = scr_WeaponController.GetAmmoAndFireMode() + "\n" + "Vyhrál jsi";
        }
        else if (MissionEnvScript.playerHitPlayed)
        {
            Ammo.text = scr_WeaponController.GetAmmoAndFireMode() + "\n" + "Prohrál jsi.....";
        }
        else if (MissionAudio.radioWarnPlayed)
        {
            Ammo.text = scr_WeaponController.GetAmmoAndFireMode() + "\n" + "Zastøelených nepøátel: " + VerbiszkyAI.deadAI + "/" + VerbiszkyAI.totalAI;
        }
        else 
        {
            Ammo.text = scr_WeaponController.GetAmmoAndFireMode() ;
        }
    }
}
