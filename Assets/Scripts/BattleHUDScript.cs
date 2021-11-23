using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI ;

public class BattleHUDScript : MonoBehaviour
{
    public Text nameText ;
	public Slider hpSlider ;
	public Slider mpSlider ;
	
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
}
