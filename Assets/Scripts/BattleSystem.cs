using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI ;

using Random = UnityEngine.Random;

public enum BattleState { START, UPKEEP, PRECOMBAT, PLAYERACTION, ENEMYACTION, CLEANUP, PLAYERSELECT, ENEMYSELECT, WIN, LOSE }

public class BattleSystem : MonoBehaviour
{
    //Inicjalizacja zmiennych gry

	public BattleState state;

	public GameObject playerPrefab ;
	public GameObject playerPrefabSpare1 ;
	public GameObject playerPrefabSpare2 ;
	public GameObject enemyPrefab ;
	public GameObject enemyPrefabSpare1 ;
	public GameObject enemyPrefabSpare2 ;
	
	public Transform playerBattleStation ;
	public Transform playerBattleStationSpare1 ;
	public Transform playerBattleStationSpare2 ;
	public Transform enemyBattleStation ;
	public Transform enemyBattleStationSpare1 ;
	public Transform enemyBattleStationSpare2 ;
	
	public BattleHUDScript playerHUD ;
	public BattleHUDScript enemyHUD ;
	
	public GameObject attackActionContainer;
    public GameObject switchActionContainer;

    public int playerActionNumber = 7;
	public int enemyActionNumber = 7;

    public int playerSpeedAmt = 5;
    public int enemySpeedAmt = 5;

    public int playerAlive = 3;
    public int enemyAlive = 3;

    UnitScript playerUnit ;
	UnitScript playerUnitSpare1 ;
	UnitScript playerUnitSpare2 ;
	UnitScript enemyUnit ;
	UnitScript enemyUnitSpare1 ;
	UnitScript enemyUnitSpare2 ;

    UnitScript spareUnit;

    void Start()
	{
		attackActionContainer.SetActive(false);
		state = BattleState.START ;
		StartCoroutine(SetupBattle()) ;
    }
	IEnumerator SetupBattle(){
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation) ;
		playerUnit = playerGO.GetComponent<UnitScript>() ;
		
		GameObject playerGOSpare1 = Instantiate(playerPrefabSpare1, playerBattleStationSpare1) ;
		playerUnitSpare1 = playerGOSpare1.GetComponent<UnitScript>() ;
		
		GameObject playerGOSpare2 = Instantiate(playerPrefabSpare2, playerBattleStationSpare2) ;
		playerUnitSpare2 = playerGOSpare2.GetComponent<UnitScript>() ;
		
		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation) ;
		enemyUnit = enemyGO.GetComponent<UnitScript>() ;
		
		GameObject enemyGOSpare1 = Instantiate(enemyPrefabSpare1, enemyBattleStationSpare1) ;
		enemyUnitSpare1 = enemyGOSpare1.GetComponent<UnitScript>() ;
		
		GameObject enemyGOSpare2 = Instantiate(enemyPrefabSpare2, enemyBattleStationSpare2) ;
		enemyUnitSpare2 = enemyGOSpare2.GetComponent<UnitScript>() ;
		
		playerHUD.SetHUD(playerUnit) ;
		enemyHUD.SetHUD(enemyUnit) ;
		
		yield return new WaitForSeconds(1.0f) ;
		
		state = BattleState.UPKEEP ;
		Upkeep() ;
	}
	
	void Upkeep()
	{
		//Regeneracje postaci
		playerUnit.curMana = Math.Min(playerUnit.curMana + 20, playerUnit.maxMana);

		playerUnitSpare1.curMana = Math.Min(playerUnitSpare1.curMana + 5, playerUnitSpare1.maxMana);
		playerUnitSpare1.curHP = Math.Min(playerUnitSpare1.curHP + 2, playerUnitSpare1.maxHP);

		playerUnitSpare2.curMana = Math.Min(playerUnitSpare2.curMana + 5, playerUnitSpare2.maxMana);
		playerUnitSpare2.curHP = Math.Min(playerUnitSpare2.curHP + 2, playerUnitSpare2.maxHP);

		enemyUnit.curMana = Math.Min(enemyUnit.curMana + 20, enemyUnit.maxMana);

		enemyUnitSpare1.curMana = Math.Min(enemyUnitSpare1.curMana + 5, enemyUnitSpare1.maxMana);
		enemyUnitSpare1.curHP = Math.Min(enemyUnitSpare1.curHP + 2, enemyUnitSpare1.maxHP);

		enemyUnitSpare2.curMana = Math.Min(enemyUnitSpare2.curMana + 5, enemyUnitSpare2.maxMana);
		enemyUnitSpare2.curHP = Math.Min(enemyUnitSpare2.curHP + 2, enemyUnitSpare2.maxHP);

		//Aktualizacja pasków zdrowia i energii

		playerHUD.SetMP(playerUnit.curMana);
		enemyHUD.SetMP(enemyUnit.curMana);

		//Przejœcie do dalszej fazy

		state = BattleState.PLAYERACTION;
		attackActionContainer.SetActive(true);
	}

	////////////////////////////////
	// Akcje gracza i przeciwnika //
	////////////////////////////////
	
	public void SetPlayerActionNumber(int numer)
    {
        attackActionContainer.SetActive(false);
        playerActionNumber = numer;
		if (playerActionNumber == 4)
        {
			playerUnit.rests = true;
            playerSpeedAmt = -10;

            SetEnemyActionNumber(Random.Range(1, 7));
            ActionExecute();
        }
		else if(playerActionNumber == 5 && playerAlive > 1)
        {
			playerUnit.switches = true;
            playerSpeedAmt = 20;

            SetEnemyActionNumber(Random.Range(1, 7));
            ActionExecute();
        }
		else if(playerActionNumber == 6)
        {
			playerUnit.intercepts = true;
            playerSpeedAmt = 20;

            SetEnemyActionNumber(Random.Range(1, 7));
            ActionExecute();
        }
        else if(playerActionNumber <= 3)
        {
            playerSpeedAmt = playerUnit.ability[playerActionNumber].speed + Convert.ToInt32(playerUnit.haste) * playerUnit.GetHasteAmt() + Convert.ToInt32(playerUnit.slow) * playerUnit.GetSlowAmt();
            //Okreœlimy tutaj ruch przeciwnika
            SetEnemyActionNumber(Random.Range(1, 7));
            //Nastêpnie przejdziemy do fazy wykonania ataków
            ActionExecute();
        }
        else
        {
            attackActionContainer.SetActive(true);
        }
	}

	public void SetEnemyActionNumber(int numer)
	{
		enemyActionNumber = numer;
		if (enemyActionNumber == 4)
		{
			enemyUnit.rests = true;
            enemySpeedAmt = -10;
        }
		else if (enemyActionNumber == 5)
		{
			enemyUnit.switches = true;
            enemySpeedAmt = 20;
        }
		else if (enemyActionNumber == 6)
		{
			enemyUnit.intercepts = true;
            enemySpeedAmt = 20;
        }
        else
        {
            enemySpeedAmt = enemyUnit.ability[enemyActionNumber].speed + Convert.ToInt32(enemyUnit.haste) * enemyUnit.GetHasteAmt() + Convert.ToInt32(enemyUnit.slow) * enemyUnit.GetSlowAmt();
        }
	}
	
	public IEnumerator ActionExecute()
    {
        //Gdy postacie u¿yj¹ zwyk³ych akcji, czyli tych unikalnych dla ka¿dej postaci
        if (playerActionNumber <= 3 && enemyActionNumber <= 3)
        {
            if (playerSpeedAmt > enemySpeedAmt)
            {
                //Atak gracza jest szybszy od ataku wroga
                yield return new WaitForSeconds(1.0f);

                //Najpierw gracz wywo³uje wszystko zwi¹zane z atakiem
                enemyUnit.TakeDamage(playerUnit.ability[playerActionNumber].damage, playerUnit.ability[playerActionNumber].times, playerUnit.ability[playerActionNumber].type, playerUnit.impair, playerUnit.strength, playerUnit.weak, playerUnit.power);
                playerUnit.impair = false;
                playerUnit.strength = false;
                playerUnit.weak = false;
                playerUnit.power = false;
                playerUnit.InflictFeatures(playerUnit.ability[playerActionNumber].boons);
                enemyHUD.SetHP(enemyUnit.curHP);
                playerHUD.SetMP(enemyUnit.curMana);
                playerHUD.SetConditions(playerUnit);
                if (enemyUnit.CheckDead())
                {
                    //Zmuœ wroga, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, gracz wygrywa
                }
                else
                {
                    enemyUnit.InflictFeatures(playerUnit.ability[playerActionNumber].banes);
                    enemyHUD.SetConditions(enemyUnit);
                    //nastêpnie wróg ma prawo swoje rzeczy zrobiæ, chyba ¿e nast¹pi³o przerwanie
                    yield return new WaitForSeconds(1.0f);
                    if (enemyUnit.interrupted == false)
                    {
                        playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type, enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                        enemyUnit.impair = false;
                        enemyUnit.strength = false;
                        enemyUnit.weak = false;
                        enemyUnit.power = false;
                        enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                        playerHUD.SetHP(playerUnit.curHP);
                        enemyHUD.SetMP(enemyUnit.curMana);
                        enemyHUD.SetConditions(enemyUnit);
                        if (playerUnit.CheckDead())
                        {
                            //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                        }
                        else
                        {
                            playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                            playerHUD.SetConditions(playerUnit);
                        }
                        yield return new WaitForSeconds(1.0f);
                    }
                    else enemyUnit.interrupted = false;
                    yield return new WaitForSeconds(1.0f);
                }
                playerUnit.interrupted = false;
            }
            else if (playerSpeedAmt < enemySpeedAmt)
            {
                //Atak wroga jest szybszy

                //Najpierw wróg wywo³uje wszystko zwi¹zane z atakiem
                playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type, enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                playerHUD.SetHP(playerUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
                enemyHUD.SetConditions(enemyUnit);
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
                else
                {
                    playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                    //nastêpnie gracz ma prawo swoje rzeczy zrobiæ, chyba ¿e nast¹pi³o przerwanie
                    if (playerUnit.interrupted == false)
                    {
                        enemyUnit.TakeDamage(playerUnit.ability[playerActionNumber].damage, playerUnit.ability[playerActionNumber].times, playerUnit.ability[playerActionNumber].type, playerUnit.impair, playerUnit.strength, playerUnit.weak, playerUnit.power);
                        playerUnit.impair = false;
                        playerUnit.strength = false;
                        playerUnit.weak = false;
                        playerUnit.power = false;
                        playerUnit.InflictFeatures(playerUnit.ability[playerActionNumber].boons);
                        if (enemyUnit.CheckDead())
                        {
                            //Zmuœ wroga, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, gracz wygrywa
                        }
                        else
                        {
                            enemyUnit.InflictFeatures(playerUnit.ability[playerActionNumber].banes);
                        }
                    }
                    else playerUnit.interrupted = false;
                }
                enemyUnit.interrupted = false;
            }
            else if (playerSpeedAmt == enemySpeedAmt)
            {
                //Szybkoœæ obu postaci jest równa

                //Najpierw gracz zadaje obra¿enia
                enemyUnit.TakeDamage(playerUnit.ability[playerActionNumber].damage, playerUnit.ability[playerActionNumber].times, playerUnit.ability[playerActionNumber].type, playerUnit.impair, playerUnit.strength, playerUnit.weak, playerUnit.power);
                playerUnit.impair = false;
                playerUnit.strength = false;
                playerUnit.weak = false;
                playerUnit.power = false;
                //Potem przeciwnik zadaje obra¿enia
                playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type, enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;

                playerHUD.SetHP(playerUnit.curHP);
                enemyHUD.SetHP(enemyUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
                enemyHUD.SetMP(enemyUnit.curMana);

                //Nastêpnie sprawdzamy, czy któraœ postaæ umar³a. Te, co prze¿y³y, maj¹ zdjête z siebie stare efekty i na³o¿one nowe
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                    //Œmieszne rzeczy wyjd¹, gdy przeciwnik te¿ straci swoj¹ ostatni¹ postaæ. Trzeba uwzglêdniæ, ¿e wtedy bêdzie remis.
                }
                else
                {
                    playerUnit.InflictFeatures(playerUnit.ability[playerActionNumber].boons);
                    playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);

                }
                if (enemyUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                    //Œmieszne rzeczy wyjd¹, gdy przeciwnik te¿ straci swoj¹ ostatni¹ postaæ. Trzeba uwzglêdniæ, ¿e wtedy bêdzie remis.
                }
                else
                {
                    enemyUnit.InflictFeatures(playerUnit.ability[playerActionNumber].banes);
                    enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                }
                playerHUD.SetConditions(playerUnit);
                enemyHUD.SetConditions(enemyUnit);
            }
        }
        else if (playerActionNumber == 4)
        {
            if (enemyActionNumber <= 3)
            {
                playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type, enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                playerHUD.SetHP(playerUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
                enemyHUD.SetConditions(enemyUnit);
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
                else
                {
                    playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                    playerHUD.SetConditions(playerUnit);
                }
                playerUnit.curHP = Math.Min(playerUnit.maxHP, playerUnit.curHP + playerUnit.restHeal);
                playerUnit.curMana = Math.Min(playerUnit.maxMana, playerUnit.curMana + playerUnit.restMana);
                playerHUD.SetHP(playerUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
            }
            else if (enemyActionNumber == 4)
            {
                playerUnit.curHP = Math.Min(playerUnit.maxHP, playerUnit.curHP + playerUnit.restHeal);
                playerUnit.curMana = Math.Min(playerUnit.maxMana, playerUnit.curMana + playerUnit.restMana);
                enemyUnit.curHP = Math.Min(enemyUnit.maxHP, enemyUnit.curHP + enemyUnit.restHeal);
                enemyUnit.curMana = Math.Min(enemyUnit.maxMana, enemyUnit.curMana + enemyUnit.restMana);
                playerHUD.SetHP(playerUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
                enemyHUD.SetHP(enemyUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
            }
            else if (enemyActionNumber == 5)
            {
                //switchUnit(1);
                playerUnit.curHP = Math.Min(playerUnit.maxHP, playerUnit.curHP + playerUnit.restHeal);
                playerUnit.curMana = Math.Min(playerUnit.maxMana, playerUnit.curMana + playerUnit.restMana);
                playerHUD.SetHP(playerUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
            }
            else if (enemyActionNumber == 6)
            {
                //Przechwycenie siê nie udaje, zrób rzeczy wizualne i informuj¹ce
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
                else
                {
                    playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                }
                playerUnit.curHP = Math.Min(playerUnit.maxHP, playerUnit.curHP + enemyUnit.restHeal);
                playerUnit.curMana = Math.Min(playerUnit.maxMana, playerUnit.curMana + enemyUnit.restMana);
                playerHUD.SetHP(playerUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
            }
        }
        else if(playerActionNumber == 5)
        {
            if (enemyActionNumber <= 3)
            {
                //switchUnit(0);
                //Wrogi gracz wykonuje nieudany atak - pud³uje

                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                enemyHUD.SetMP(enemyUnit.curMana);
                enemyHUD.SetConditions(enemyUnit);
            }
            else if (enemyActionNumber == 4)
            {
                //switchUnit(0);
                //Wróg siê leczy
                enemyUnit.curHP = Math.Min(enemyUnit.maxHP, enemyUnit.curHP + enemyUnit.restHeal);
                enemyUnit.curMana = Math.Min(enemyUnit.maxMana, enemyUnit.curMana + enemyUnit.restMana);
                enemyHUD.SetHP(enemyUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
            }
            else if (enemyActionNumber == 5)
            {
                //switchUnit(0);
                //switchUnit(1);
            }
            else if (enemyActionNumber == 6)
            {
                //Udane przechwycenie ze strony przeciwnika.
                playerUnit.TakeDamage(enemyUnit.interceptDamage, 1, "PHYSICAL", enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                playerHUD.SetHP(playerUnit.curHP);
                enemyHUD.SetConditions(enemyUnit);
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
            }
        }
        else if (playerActionNumber == 6)
        {
            if (enemyActionNumber <= 3)
            {
                //Przechwycenie gracza siê nie udaje. Przeciwnik normalnie wykonuje swój atak
                playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type, enemyUnit.impair, enemyUnit.strength, enemyUnit.weak, enemyUnit.power);
                enemyUnit.impair = false;
                enemyUnit.strength = false;
                enemyUnit.weak = false;
                enemyUnit.power = false;
                enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
                playerHUD.SetHP(playerUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
                enemyHUD.SetConditions(enemyUnit);
                if (playerUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
                else
                {
                    playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                    playerHUD.SetConditions(playerUnit);
                }
            }
            else if (enemyActionNumber == 4)
            {
                //Przechwycenie gracza siê nie udaje. Przeciwnik normalnie odpoczywa
                enemyUnit.curHP = Math.Min(enemyUnit.maxHP, enemyUnit.curHP + enemyUnit.restHeal);
                enemyUnit.curMana = Math.Min(enemyUnit.maxMana, enemyUnit.curMana + enemyUnit.restMana);
                enemyHUD.SetHP(enemyUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
            }
            else if (enemyActionNumber == 5)
            {
                //Przechwycenie siê udaje - przeciwnik otrzymuje obra¿enia
                enemyUnit.TakeDamage(playerUnit.interceptDamage, 1, "PHYSICAL", playerUnit.impair, playerUnit.strength, playerUnit.weak, playerUnit.power);
                playerUnit.impair = false;
                playerUnit.strength = false;
                playerUnit.weak = false;
                playerUnit.power = false;
                playerHUD.SetConditions(playerUnit);
                enemyHUD.SetConditions(enemyUnit);
                enemyHUD.SetHP(enemyUnit.curHP);
                if (enemyUnit.CheckDead())
                {
                    //Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
                }
            }
            else if (enemyActionNumber == 6)
            {
                //Postacie próbuj¹ siê przechwyciæ nawzajem - nic siê nie dzieje
            }
        }
    }

    void SwitchCharacter (bool isEnemy, bool isFirstOption)
    {
        if (isEnemy)
        {
            if (isFirstOption)
            {
            }
            else
            {
            }
        }
        else
        {
            if (isFirstOption)
            {
            }
            else
            {
            }
        }
    }

	////////////////////////////
	// Funkcje powi¹zane z SI //
	////////////////////////////
}