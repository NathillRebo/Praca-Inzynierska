using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public string unitName ;
	public int maxHP ;
	public int curHP ;
	public int maxMana ;
	public int curMana ;
	public int phsArmor ;
	public int lgtArmor ;
	public int drkArmor ;

	public AbilityScript[] ability;

	//stany
	
	float vulnerabilityMult = 1.4f;
	float strengthMult = 1.4f;
	float impairMult = 0.7f;
	float resistanceMult = 0.7f;
	int hasteAmt = 2;
	int slowAmt = -2;
	int bolsterAmt = 4;
	int frailtyAmt = 4;
	int powerAmt = 4;
	int weakAmt = 4;

	int interceptDamage = 50;
	int restHeal = 2;
	int restMana = 5;

	public bool switches = false; //Unika wszystkich atak�w, opr�cz przechwycenia
	public bool intercepts = false; //Pr�buje przechwyci� wroga zmieniaj�cego si�
	public bool rests = false; //Pr�buje odpocz��, przywracaj�c sobie punkty �ywono�ci i energii

	public bool interrupted = false; //Nie wykonuje swojej akcji, ale wci�� zu�ywa na ni� energi�
	public bool vulnerability = false; //Otrzymuje obra�enia zwi�kszone o 40%
	public bool strength = false; // Zadaje obra�enia zwi�kszone o 40%
	public bool impair = false; // Zadaje obra�enia zmniejszone o 30%
	public bool resistance = false; // Otrzymuje obra�enia zmniejszone o 30%
	public bool haste = false; //Akcja ma szybko�� zwi�kszon� o 2
	public bool slow = false; //Akcja ma szybko�� zmniejszon� o 2
	public bool bolster = false; //Zmniejsza obra�enia otrzymane o 4
	public bool frailty = false; //Zwi�ksza obra�enia otrzymne o 4
	public bool power = false;
	public bool weak = false;

	public void TakeDamage(int amnt, int mult, string dmgType, bool impairSrc, bool strengthSrc, bool weakSrc, bool powerSrc)
    {
		int redu = 0;
		float amt = (float)amnt;
		if (dmgType == "PHYSICAL")
        {
			redu = phsArmor;
        }
		else if(dmgType == "LIGHT")
        {
			redu = lgtArmor;
        }
		else if(dmgType == "DARK")
        {
			redu = drkArmor;
		}
		if (frailty)
		{
			amt += frailtyAmt;
			frailty = false;
		}
		if (powerSrc)
		{
			amt += powerAmt;
		}
		if (vulnerability)
		{
			amt *= vulnerabilityMult;
			vulnerability = false;
		}
		if (strengthSrc)
		{
			amt *= strengthMult;
		}
		if (impairSrc)
		{
			amt *= impairMult;
		}
		if (resistance)
		{
			amt *= resistanceMult;
			resistance = false;
		}
		if (weakSrc)
        {
			amt -= weakAmt;
        }
		if (bolster)
        {
			amt -= bolsterAmt;
			bolster = false;
        }
		amt = Math.Max(0, amt - redu);
		amt = Convert.ToInt32(Math.Round(amt, MidpointRounding.AwayFromZero));
		int dmg = Convert.ToInt32(Math.Min(0, (amt - redu) * mult));
		curHP -= dmg;
    }
	public bool CheckDead()
    {
		if (curHP <= 0)
        {
			return true;
        }
		return false;
    }
	
	public void InflictFeatures(string features)
    {
		string[] featureList = features.Split('|');
		foreach (string feature in featureList)
        {
			if (feature == "INTERRUPTED") interrupted = true;
			if (feature == "VULNERABILITY") vulnerability = true;
			if (feature == "STRENGTH") strength = true;
			if (feature == "IMPAIR") impair = true;
			if (feature == "RESISTANCE") resistance = true;
			if (feature == "BOLSTER") bolster = true;
			if (feature == "FRAILTY") frailty = true;
			if (feature == "HASTE") haste = true;
			if (feature == "SLOW") slow = true;
			if (feature == "POWER") power = true;
			if (feature == "WEAKNESS") weak = true;
		}
	}
}
