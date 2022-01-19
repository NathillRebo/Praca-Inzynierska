using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI ;

public class BattleHUDScript : MonoBehaviour
{
    public Text nameText ;
	public Slider hpSlider ;
	public Slider mpSlider ;
    public Image interruptedImage;
    public Image bolsterImage;
    public Image frailtyImage;
    public Image hasteImage;
    public Image slowImage;
    public Image strengthImage;
    public Image impairImage;
    public Image powerImage;
    public Image weakImage;
    public Image resistanceImage;
    public Image vulnerabilityImage;

    Color32 activeStatus = new Color32(255, 255, 255, 255);
    Color32 inactiveStatus = new Color32(128, 128, 128, 128);

    public void SetHUD(UnitScript unit){
		nameText.text = unit.unitName ;
		hpSlider.maxValue = unit.maxHP ;
		hpSlider.value = unit.curHP ;
		mpSlider.maxValue = unit.maxMana ;
		mpSlider.value = unit.curMana ;
    }
	
	public void SetHP(int hp){
		hpSlider.value = hp ;
	}
	
	public void SetMP(int mp){
		mpSlider.value = mp ;
	}

    public void SetConditions(UnitScript unit)
    {
        if (unit.interrupted) interruptedImage.color = activeStatus; else interruptedImage.color = inactiveStatus;
        if (unit.vulnerability) vulnerabilityImage.color = activeStatus; else vulnerabilityImage.color = inactiveStatus;
        if (unit.strength) strengthImage.color = activeStatus; else strengthImage.color = inactiveStatus;
        if (unit.impair) impairImage.color = activeStatus; else impairImage.color = inactiveStatus;
        if (unit.resistance) resistanceImage.color = activeStatus; else resistanceImage.color = inactiveStatus;
        if (unit.haste) hasteImage.color = activeStatus; else hasteImage.color = inactiveStatus;
        if (unit.slow) slowImage.color = activeStatus; else slowImage.color = inactiveStatus;
        if (unit.bolster) bolsterImage.color = activeStatus; else bolsterImage.color = inactiveStatus;
        if (unit.frailty) frailtyImage.color = activeStatus; else frailtyImage.color = inactiveStatus;
        if (unit.power) powerImage.color = activeStatus; else powerImage.color = inactiveStatus;
        if (unit.weak) weakImage.color = activeStatus; else weakImage.color = inactiveStatus;
    }
}
