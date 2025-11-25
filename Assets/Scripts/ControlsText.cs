using UnityEngine;
using TMPro;

public class ControlsText : MonoBehaviour
{
    public TMP_Text controlsText;

    public void onWASD()
    {
        controlsText.text = "Move with WASD";
    }

    public void onJump()
    {
        controlsText.text = "Jump with Space";
    }

    public void onRun()
    {
        controlsText.text = "Run with Shift";
    }

    public void onCrouch()
    {
        controlsText.text = "Crouch with Left Control";
    }

    public void onClicking()
    {
        controlsText.text = "Pickup Items with E";
    }

    public void onGunBeam()
    {
        controlsText.text = "Fire Beam with Left Mouse";
    }

    public void onGunFoam()
    {
        controlsText.text = "Toggle between Firing Modes with C<br>or quick switch with Right Mouse, then Left Mouse";
    }

    public void onCanister()
    {
        controlsText.text = "Use Resin-due Refills with R<br>and Health Refills with H";
    }

    public void onLanternFirst()
    {
        controlsText.text = "Activate babyvision(rename this good god) with Q";
    }
}
