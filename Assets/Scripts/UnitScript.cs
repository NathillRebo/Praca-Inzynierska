using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public string unitName ;
    public Sprite unitPortrait;
	public int maxHP ;
	public int curHP ;
	public int maxMana ;
	public int curMana ;
	public int phsArmor ;
	public int lgtArmor ;
	public int drkArmor ;

	public AbilityScript[] ability;

    //stany

    readonly float vulnerabilityMult = 1.4f;
    readonly float strengthMult = 1.4f;
    readonly float impairMult = 0.7f;
    readonly float resistanceMult = 0.7f;
    readonly int hasteAmt = 2;
    readonly int slowAmt = -2;
    readonly int bolsterAmt = 4;
    readonly int frailtyAmt = 4;
    readonly int powerAmt = 4;
    readonly int weakAmt = 4;

    readonly public int interceptDamage = 50;
    readonly public int restHeal = 2;
    readonly public int restMana = 5;

	public bool switches = false; //Unika wszystkich ataków, oprócz przechwycenia
	public bool intercepts = false; //Próbuje przechwyciæ wroga zmieniaj¹cego siê
	public bool rests = false; //Próbuje odpocz¹æ, przywracaj¹c sobie punkty ¿ywonoœci i energii

	public bool interrupted = false; //Nie wykonuje swojej akcji, ale wci¹¿ zu¿ywa na ni¹ energiê
	public bool vulnerability = false; //Otrzymuje obra¿enia zwiêkszone o 40%
	public bool strength = false; // Zadaje obra¿enia zwiêkszone o 40%
	public bool impair = false; // Zadaje obra¿enia zmniejszone o 30%
	public bool resistance = false; // Otrzymuje obra¿enia zmniejszone o 30%
	public bool haste = false; //Akcja ma szybkoœæ zwiêkszon¹ o 2
	public bool slow = false; //Akcja ma szybkoœæ zmniejszon¹ o 2
	public bool bolster = false; //Zmniejsza obra¿enia otrzymane o 4
	public bool frailty = false; //Zwiêksza obra¿enia otrzymane o 4
	public bool power = false; //Zwiêksza obra¿enia zadane o 4
	public bool weak = false; //Zmniejsza obra¿enia zadane o 4

    public int GetHasteAmt()
    {
        return hasteAmt;
    }

    public int GetSlowAmt()
    {
        return slowAmt;
    }

    public void TakeDamage(int amnt, int mult, string dmgType, bool impairSrc, bool strengthSrc, bool weakSrc, bool powerSrc)
    {
		int redu = 0;
		float amt = amnt;
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
        //Debug.Log("amt: " + amt);
        amt = Convert.ToInt32(Math.Round(amt, MidpointRounding.AwayFromZero));
        //Debug.Log("amt: " + amt);
        int dmg = Convert.ToInt32(Math.Max(0, (amt - redu) * mult));
        //Debug.Log("dmg: " + dmg);
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
