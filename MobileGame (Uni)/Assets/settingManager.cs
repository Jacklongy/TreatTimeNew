using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class settingManager : MonoBehaviour
{
    GameOverseer overseer;
    public TextMeshProUGUI VibOnOrOff;

    private void Start()
    {
        overseer = FindAnyObjectByType<GameOverseer>();
        VibOnOrOff.text = "Vibrations Off";
    }

    public void VibrationToggle()
    {
        overseer.VibrationsToggle();

        // text here 
        if (overseer.Vibrations != true)
        {
            VibOnOrOff.text = "Vibrations Off";
        }
        else
        {
            
            VibOnOrOff.text = "Vibrations On";
        }

       

    }

    public void CloudDataLoad()
    {
        overseer.UseCloud();
    }
}
