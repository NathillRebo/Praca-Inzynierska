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

    public Button attack01;
    public Button attack02;
    public Button attack03;
    public Button attack04;
    public Button switch01;
    public Button switch02;

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

    //zmienne wykorzystywane przez SI
    private int bardzo_mala_wartosc = -214748;
    private int bardzo_duza_wartosc = 214748;
    public int AI_KillBonus = 100;
    public float AI_ManaMultiplier = 7.2f;
    public bool AI_ChooseMax = false;
    public bool AI_ChooseMin = false;
    public float p_c = 80; //Zalecany przedzia³ to 70% do 90% - jest to dyskryminator krzy¿owania
    public float p_m = 35; //Zalecany przedzia³ to 20% do 50% - jest to dyskryminator mutowania
    public int AI_pop = 3;
    public int AI_n = 2; // Zalecana wartoœæ nie wiêksza ni¿ 9, bo mo¿e wysadziæ komputer - to i tak ju¿ bêdzie wymagaæ pewnie z trzy gigabajty, 8 bêdzie zajmowaæ ok. 400MB, 7 oko³o 50MB, 6 ok. 6MB
    public int AI_gen_max = 64;

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

        //Aktualizacja ikon

        attack01.image.sprite = playerUnit.ability[0].skillIcon;
        attack02.image.sprite = playerUnit.ability[1].skillIcon;
        attack03.image.sprite = playerUnit.ability[2].skillIcon;
        attack04.image.sprite = playerUnit.ability[3].skillIcon;
        switch01.image.sprite = playerUnitSpare1.unitPortrait;
        switch02.image.sprite = playerUnitSpare2.unitPortrait;

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

            SetEnemyActionNumber(AI_Run());
            StartCoroutine(ActionExecute());
        }
		else if((playerActionNumber == 5 || playerActionNumber == 8) && playerAlive > 1)
        {
			playerUnit.switches = true;
            playerSpeedAmt = 20;

            SetEnemyActionNumber(AI_Run());
            StartCoroutine(ActionExecute());
        }
		else if(playerActionNumber == 6 && enemyAlive > 1)
        {
			playerUnit.intercepts = true;
            playerSpeedAmt = 20;

            SetEnemyActionNumber(AI_Run());
            StartCoroutine(ActionExecute());
        }
        else if(playerActionNumber <= 3 && playerUnit.ability[playerActionNumber].cost <= playerUnit.curMana)
        {
            playerUnit.curMana -= playerUnit.ability[playerActionNumber].cost;
            playerSpeedAmt = playerUnit.ability[playerActionNumber].speed + Convert.ToInt32(playerUnit.haste) * playerUnit.GetHasteAmt() + Convert.ToInt32(playerUnit.slow) * playerUnit.GetSlowAmt();
            //Okreœlimy tutaj ruch przeciwnika
            SetEnemyActionNumber(AI_Run());
            //Nastêpnie przejdziemy do fazy wykonania ataków
            StartCoroutine(ActionExecute());
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
		else if (enemyActionNumber == 5 || enemyActionNumber == 8)
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
        if (playerActionNumber < 4) Debug.Log("Akcja gracza: " + playerUnit.ability[playerActionNumber].skillName);
        else if (playerActionNumber == 4) Debug.Log("Akcja gracza: Przerwa");
        else if (playerActionNumber == 5) Debug.Log("Akcja gracza: Zamiana z: " + playerUnitSpare1.name);
        else if (playerActionNumber == 6) Debug.Log("Akcja gracza: Przechwycenie!");
        else if (playerActionNumber == 8) Debug.Log("Akcja gracza: Zamiana z: " + playerUnitSpare2.name);

        if (enemyActionNumber < 4) Debug.Log("Akcja przeciwnika: " + enemyUnit.ability[enemyActionNumber].skillName);
        else if (enemyActionNumber == 4) Debug.Log("Akcja przeciwnika: Przerwa");
        else if (enemyActionNumber == 5) Debug.Log("Akcja przeciwnika: Zamiana z: " + enemyUnitSpare1.name);
        else if (enemyActionNumber == 6) Debug.Log("Akcja przeciwnika: Przechwycenie!");
        else if (enemyActionNumber == 8) Debug.Log("Akcja przeciwnika: Zamiana z: " + enemyUnitSpare2.name);

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
                    if (enemyAlive > 1)
                    {
                        enemyAlive--;
                        if (enemyUnitSpare1.CheckDead()) SwitchCharacter(true, false); else SwitchCharacter(true, true);
                        
                    }
                    else
                    {
                        //Gracz wygrywa!
                    }
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
                            if (playerAlive > 1)
                            {
                                playerAlive--;
                                if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                            }
                            else
                            {
                                //Gracz przegrywa!
                            }
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
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
                            if (enemyAlive > 1)
                            {
                                enemyAlive--;
                                if (enemyUnitSpare1.CheckDead()) SwitchCharacter(true, false); else SwitchCharacter(true, true);

                            }
                            else
                            {
                                //Gracz wygrywa!
                            }
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
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
                    if (enemyAlive > 1)
                    {
                        enemyAlive--;
                        if (enemyUnitSpare1.CheckDead()) SwitchCharacter(true, false); else SwitchCharacter(true, true);

                    }
                    else
                    {
                        //Gracz wygrywa!
                    }
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
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
            else if (enemyActionNumber == 5 || enemyActionNumber == 8)
            {
                if (enemyActionNumber == 5) SwitchCharacter(true, true); else SwitchCharacter(true, false);
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
                }
                else
                {
                    //playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
                }
                playerUnit.curHP = Math.Min(playerUnit.maxHP, playerUnit.curHP + enemyUnit.restHeal);
                playerUnit.curMana = Math.Min(playerUnit.maxMana, playerUnit.curMana + enemyUnit.restMana);
                playerHUD.SetHP(playerUnit.curHP);
                playerHUD.SetMP(playerUnit.curMana);
            }
        }
        else if(playerActionNumber == 5 || playerActionNumber == 8)
        {
            if (enemyActionNumber <= 3)
            {
                if (playerActionNumber == 5) SwitchCharacter(false, true); else SwitchCharacter(false, false);
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
                if (playerActionNumber == 5) SwitchCharacter(false, true); else SwitchCharacter(false, false);
                //Wróg siê leczy
                enemyUnit.curHP = Math.Min(enemyUnit.maxHP, enemyUnit.curHP + enemyUnit.restHeal);
                enemyUnit.curMana = Math.Min(enemyUnit.maxMana, enemyUnit.curMana + enemyUnit.restMana);
                enemyHUD.SetHP(enemyUnit.curHP);
                enemyHUD.SetMP(enemyUnit.curMana);
            }
            else if (enemyActionNumber == 5 || enemyActionNumber == 8)
            {
                if (playerActionNumber == 5) SwitchCharacter(false, true); else SwitchCharacter(false, false);
                if (enemyActionNumber == 5) SwitchCharacter(true, true); else SwitchCharacter(true, false);
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
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
                    if (playerAlive > 1)
                    {
                        playerAlive--;
                        if (playerUnitSpare1.CheckDead()) SwitchCharacter(false, false); else SwitchCharacter(false, true);
                    }
                    else
                    {
                        //Gracz przegrywa!
                    }
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
            else if (enemyActionNumber == 5 || enemyActionNumber == 8)
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
                    //Zmuœ wroga, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, gracz wygrywa
                    if (enemyAlive > 1)
                    {
                        enemyAlive--;
                        if (enemyUnitSpare1.CheckDead()) SwitchCharacter(true, false); else SwitchCharacter(true, true);

                    }
                    else
                    {
                        //Gracz wygrywa!
                    }
                }
            }
            else if (enemyActionNumber == 6)
            {
                //Postacie próbuj¹ siê przechwyciæ nawzajem - nic siê nie dzieje
            }
        }
        Upkeep();
    }

    GameObject prefabTemporary;
    UnitScript unitScriptTemporary;

    void SwitchCharacter (bool isEnemy, bool isFirstOption)
    {
        if (isEnemy)
        {
            if (isFirstOption)
            {
                prefabTemporary = enemyPrefab;
                enemyPrefab = enemyPrefabSpare1;
                enemyPrefabSpare1 = prefabTemporary;
                unitScriptTemporary = enemyUnit;
                enemyUnit = enemyUnitSpare1;
                enemyUnitSpare1 = unitScriptTemporary;
            }
            else
            {
                prefabTemporary = enemyPrefab;
                enemyPrefab = enemyPrefabSpare2;
                enemyPrefabSpare2 = prefabTemporary;
                unitScriptTemporary = enemyUnit;
                enemyUnit = enemyUnitSpare2;
                enemyUnitSpare2 = unitScriptTemporary;
            }
        }
        else
        {
            if (isFirstOption)
            {
                prefabTemporary = playerPrefab;
                playerPrefab = playerPrefabSpare1;
                playerPrefabSpare1 = prefabTemporary;
                unitScriptTemporary = playerUnit;
                playerUnit = playerUnitSpare1;
                playerUnitSpare1 = unitScriptTemporary;
            }
            else
            {
                prefabTemporary = playerPrefab;
                playerPrefab = playerPrefabSpare2;
                playerPrefabSpare2 = prefabTemporary;
                unitScriptTemporary = playerUnit;
                playerUnit = playerUnitSpare2;
                playerUnitSpare2 = unitScriptTemporary;
            }
        }
    }

    ////////////////////////////
    // Funkcje powi¹zane z SI //
    ////////////////////////////
    
    //AI_EvaluateSingle zwraca wartoœæ punktow¹ jednego ruchu. Wywo³ywane przez AI_Evaluate.
    //Algorytm zagl¹da ³¹cznie w dziewiêæ ruchów, bo na wiêcej mo¿e nie starczyæ pamiêci.
    float AI_EvaluateSingle(int[] actionAIqueue, int activeL, int activeR, int L1HP, int L1MP, string L1STAT, int L2HP, int L2MP, string L2STAT, int L3HP, int L3MP, string L3STAT, int R1HP, int R1MP, string R1STAT, int R2HP, int R2MP, string R2STAT, int R3HP, int R3MP, string R3STAT, int pozostale_iteracje)
    {
        int actionAI = actionAIqueue[AI_n - pozostale_iteracje];
        //0-3 to zdolnoœci, 4 to opuszczenie tury, 5 to zmiana pierwsza, 6 to przechwycenie, 8 to zmiana druga
        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
        int[] AI_damages = new int[6]; //indeksy 0, 1, 2, 3 oznaczaj¹ obra¿enia, które zada atak gracza ludzkiego wymierzony w gracza komputerowego, 4 to przechwycenie wykonane przez gracza ludzkiego, 5 to atak wykonany przez gracza komputerowego
        int[] AI_MPLoses = new int[5]; //indeksy 0, 1, 2, 3 oznaczaj¹ akcje gracza ludzkiego, 4 oznacza jedn¹ akcjê gracza komputerowego
        int[] AI_Speeds = new int[5]; //analogiczne do linijki wy¿ej
        float AI_amt = 0;

        //Tutaj wyliczamy obra¿enia, jakie poszczególne ataki postaci gracza ludzkiego zadadz¹
        if (activeL == 0)
        {
            if (activeR == 0)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnit.ability[AI_damages_i].damage;
                    if (R1STAT[0] == '1') AI_amt += 4;
                    if (L1STAT[1] == '1') AI_amt += 4;
                    if (R1STAT[2] == '1') AI_amt *= 1.4f;
                    if (L1STAT[3] == '1') AI_amt *= 1.4f;
                    if (L1STAT[4] == '1') AI_amt *= 0.7f;
                    if (R1STAT[5] == '1') AI_amt *= 0.7f;
                    if (L1STAT[6] == '1') AI_amt -= 4;
                    if (R1STAT[7] == '1') AI_amt -= 4;
                    if (playerUnit.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                    if (playerUnit.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnit.lgtArmor);
                    if (playerUnit.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnit.drkArmor);
                    AI_amt *= playerUnit.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R1STAT[0] == '1') AI_amt += 4;
                if (L1STAT[1] == '1') AI_amt += 4;
                if (R1STAT[2] == '1') AI_amt *= 1.4f;
                if (L1STAT[3] == '1') AI_amt *= 1.4f;
                if (L1STAT[4] == '1') AI_amt *= 0.7f;
                if (R1STAT[5] == '1') AI_amt *= 0.7f;
                if (L1STAT[6] == '1') AI_amt -= 4;
                if (R1STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if(actionAI < 4)
                {
                    AI_amt = enemyUnit.ability[actionAI].damage;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnit.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    if (enemyUnit.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnit.lgtArmor);
                    if (enemyUnit.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnit.drkArmor);
                    AI_amt *= enemyUnit.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 1)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnit.ability[AI_damages_i].damage;
                    if (R2STAT[0] == '1') AI_amt += 4;
                    if (L1STAT[1] == '1') AI_amt += 4;
                    if (R2STAT[2] == '1') AI_amt *= 1.4f;
                    if (L1STAT[3] == '1') AI_amt *= 1.4f;
                    if (L1STAT[4] == '1') AI_amt *= 0.7f;
                    if (R2STAT[5] == '1') AI_amt *= 0.7f;
                    if (L1STAT[6] == '1') AI_amt -= 4;
                    if (R2STAT[7] == '1') AI_amt -= 4;
                    if (playerUnit.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.phsArmor);
                    if (playerUnit.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.lgtArmor);
                    if (playerUnit.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.drkArmor);
                    AI_amt *= playerUnit.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R2STAT[0] == '1') AI_amt += 4;
                if (L1STAT[1] == '1') AI_amt += 4;
                if (R2STAT[2] == '1') AI_amt *= 1.4f;
                if (L1STAT[3] == '1') AI_amt *= 1.4f;
                if (L1STAT[4] == '1') AI_amt *= 0.7f;
                if (R2STAT[5] == '1') AI_amt *= 0.7f;
                if (L1STAT[6] == '1') AI_amt -= 4;
                if (R2STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare1.ability[actionAI].damage;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare1.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnit.lgtArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnit.drkArmor);
                    AI_amt *= enemyUnitSpare1.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 2)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnit.ability[0].damage;
                    if (R3STAT[0] == '1') AI_amt += 4;
                    if (L1STAT[1] == '1') AI_amt += 4;
                    if (R3STAT[2] == '1') AI_amt *= 1.4f;
                    if (L1STAT[3] == '1') AI_amt *= 1.4f;
                    if (L1STAT[4] == '1') AI_amt *= 0.7f;
                    if (R3STAT[5] == '1') AI_amt *= 0.7f;
                    if (L1STAT[6] == '1') AI_amt -= 4;
                    if (R3STAT[7] == '1') AI_amt -= 4;
                    if (playerUnit.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.phsArmor);
                    if (playerUnit.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.lgtArmor);
                    if (playerUnit.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.drkArmor);
                    AI_amt *= playerUnit.ability[0].times;
                    AI_damages[0] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R3STAT[0] == '1') AI_amt += 4;
                if (L1STAT[1] == '1') AI_amt += 4;
                if (R3STAT[2] == '1') AI_amt *= 1.4f;
                if (L1STAT[3] == '1') AI_amt *= 1.4f;
                if (L1STAT[4] == '1') AI_amt *= 0.7f;
                if (R3STAT[5] == '1') AI_amt *= 0.7f;
                if (L1STAT[6] == '1') AI_amt -= 4;
                if (R3STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);


                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare2.ability[actionAI].damage;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare2.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnit.lgtArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnit.drkArmor);
                    AI_amt *= enemyUnitSpare2.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L1STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L1STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L1STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L1STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnit.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
        }
        else if (activeL == 1)
        {
            if (activeR == 0)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare1.ability[AI_damages_i].damage;
                    if (R1STAT[0] == '1') AI_amt += 4;
                    if (L2STAT[1] == '1') AI_amt += 4;
                    if (R1STAT[2] == '1') AI_amt *= 1.4f;
                    if (L2STAT[3] == '1') AI_amt *= 1.4f;
                    if (L2STAT[4] == '1') AI_amt *= 0.7f;
                    if (R1STAT[5] == '1') AI_amt *= 0.7f;
                    if (L2STAT[6] == '1') AI_amt -= 4;
                    if (R1STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare1.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnit.lgtArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnit.drkArmor);
                    AI_amt *= playerUnitSpare1.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R1STAT[0] == '1') AI_amt += 4;
                if (L2STAT[1] == '1') AI_amt += 4;
                if (R1STAT[2] == '1') AI_amt *= 1.4f;
                if (L2STAT[3] == '1') AI_amt *= 1.4f;
                if (L2STAT[4] == '1') AI_amt *= 0.7f;
                if (R1STAT[5] == '1') AI_amt *= 0.7f;
                if (L2STAT[6] == '1') AI_amt -= 4;
                if (R1STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if (actionAI < 4)
                {
                    AI_amt = enemyUnit.ability[actionAI].damage;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnit.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    if (enemyUnit.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.lgtArmor);
                    if (enemyUnit.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.drkArmor);
                    AI_amt *= enemyUnit.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 1)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare1.ability[AI_damages_i].damage;
                    if (R2STAT[0] == '1') AI_amt += 4;
                    if (L2STAT[1] == '1') AI_amt += 4;
                    if (R2STAT[2] == '1') AI_amt *= 1.4f;
                    if (L2STAT[3] == '1') AI_amt *= 1.4f;
                    if (L2STAT[4] == '1') AI_amt *= 0.7f;
                    if (R2STAT[5] == '1') AI_amt *= 0.7f;
                    if (L2STAT[6] == '1') AI_amt -= 4;
                    if (R2STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare1.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.phsArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.lgtArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.drkArmor);
                    AI_amt *= playerUnitSpare1.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R2STAT[0] == '1') AI_amt += 4;
                if (L2STAT[1] == '1') AI_amt += 4;
                if (R2STAT[2] == '1') AI_amt *= 1.4f;
                if (L2STAT[3] == '1') AI_amt *= 1.4f;
                if (L2STAT[4] == '1') AI_amt *= 0.7f;
                if (R2STAT[5] == '1') AI_amt *= 0.7f;
                if (L2STAT[6] == '1') AI_amt -= 4;
                if (R2STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare1.ability[actionAI].damage;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare1.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.lgtArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.drkArmor);
                    AI_amt *= enemyUnitSpare1.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 2)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare1.ability[0].damage;
                    if (R3STAT[0] == '1') AI_amt += 4;
                    if (L2STAT[1] == '1') AI_amt += 4;
                    if (R3STAT[2] == '1') AI_amt *= 1.4f;
                    if (L2STAT[3] == '1') AI_amt *= 1.4f;
                    if (L2STAT[4] == '1') AI_amt *= 0.7f;
                    if (R3STAT[5] == '1') AI_amt *= 0.7f;
                    if (L2STAT[6] == '1') AI_amt -= 4;
                    if (R3STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare1.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.phsArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.lgtArmor);
                    if (playerUnitSpare1.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.drkArmor);
                    AI_amt *= playerUnitSpare1.ability[0].times;
                    AI_damages[0] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R3STAT[0] == '1') AI_amt += 4;
                if (L2STAT[1] == '1') AI_amt += 4;
                if (R3STAT[2] == '1') AI_amt *= 1.4f;
                if (L2STAT[3] == '1') AI_amt *= 1.4f;
                if (L2STAT[4] == '1') AI_amt *= 0.7f;
                if (R3STAT[5] == '1') AI_amt *= 0.7f;
                if (L2STAT[6] == '1') AI_amt -= 4;
                if (R3STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);
                
                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare2.ability[actionAI].damage;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare2.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.lgtArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.drkArmor);
                    AI_amt *= enemyUnitSpare2.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L2STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L2STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L2STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L2STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare1.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
        }
        else if (activeL == 2)
        {
            if (activeR == 0)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare2.ability[AI_damages_i].damage;
                    if (R1STAT[0] == '1') AI_amt += 4;
                    if (L3STAT[1] == '1') AI_amt += 4;
                    if (R1STAT[2] == '1') AI_amt *= 1.4f;
                    if (L3STAT[3] == '1') AI_amt *= 1.4f;
                    if (L3STAT[4] == '1') AI_amt *= 0.7f;
                    if (R1STAT[5] == '1') AI_amt *= 0.7f;
                    if (L3STAT[6] == '1') AI_amt -= 4;
                    if (R1STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare2.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnit.lgtArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnit.drkArmor);
                    AI_amt *= playerUnitSpare2.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R1STAT[0] == '1') AI_amt += 4;
                if (L3STAT[1] == '1') AI_amt += 4;
                if (R1STAT[2] == '1') AI_amt *= 1.4f;
                if (L3STAT[3] == '1') AI_amt *= 1.4f;
                if (L3STAT[4] == '1') AI_amt *= 0.7f;
                if (R1STAT[5] == '1') AI_amt *= 0.7f;
                if (L3STAT[6] == '1') AI_amt -= 4;
                if (R1STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);
                
                if (actionAI < 4)
                {
                    AI_amt = enemyUnit.ability[actionAI].damage;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnit.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    if (enemyUnit.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.lgtArmor);
                    if (enemyUnit.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.drkArmor);
                    AI_amt *= enemyUnit.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R1STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R1STAT[3] == '1') AI_amt *= 1.4f;
                    if (R1STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R1STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 1)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare2.ability[AI_damages_i].damage;
                    if (R2STAT[0] == '1') AI_amt += 4;
                    if (L3STAT[1] == '1') AI_amt += 4;
                    if (R2STAT[2] == '1') AI_amt *= 1.4f;
                    if (L3STAT[3] == '1') AI_amt *= 1.4f;
                    if (L3STAT[4] == '1') AI_amt *= 0.7f;
                    if (R2STAT[5] == '1') AI_amt *= 0.7f;
                    if (L3STAT[6] == '1') AI_amt -= 4;
                    if (R2STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare2.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.phsArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.lgtArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare1.drkArmor);
                    AI_amt *= playerUnitSpare2.ability[AI_damages_i].times;
                    AI_damages[AI_damages_i] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R2STAT[0] == '1') AI_amt += 4;
                if (L3STAT[1] == '1') AI_amt += 4;
                if (R2STAT[2] == '1') AI_amt *= 1.4f;
                if (L3STAT[3] == '1') AI_amt *= 1.4f;
                if (L3STAT[4] == '1') AI_amt *= 0.7f;
                if (R2STAT[5] == '1') AI_amt *= 0.7f;
                if (L3STAT[6] == '1') AI_amt -= 4;
                if (R2STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare1.ability[actionAI].damage;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare1.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.lgtArmor);
                    if (enemyUnitSpare1.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.drkArmor);
                    AI_amt *= enemyUnitSpare1.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R2STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R2STAT[3] == '1') AI_amt *= 1.4f;
                    if (R2STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R2STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
            else if (activeR == 2)
            {
                for (int AI_damages_i = 0; AI_damages_i < 4; AI_damages_i++)
                {
                    AI_amt = playerUnitSpare2.ability[0].damage;
                    if (R3STAT[0] == '1') AI_amt += 4;
                    if (L3STAT[1] == '1') AI_amt += 4;
                    if (R3STAT[2] == '1') AI_amt *= 1.4f;
                    if (L3STAT[3] == '1') AI_amt *= 1.4f;
                    if (L3STAT[4] == '1') AI_amt *= 0.7f;
                    if (R3STAT[5] == '1') AI_amt *= 0.7f;
                    if (L3STAT[6] == '1') AI_amt -= 4;
                    if (R3STAT[7] == '1') AI_amt -= 4;
                    if (playerUnitSpare2.ability[AI_damages_i].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.phsArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.lgtArmor);
                    if (playerUnitSpare2.ability[AI_damages_i].type == "DARK") AI_amt = Math.Max(0, AI_amt - enemyUnitSpare2.drkArmor);
                    AI_amt *= playerUnitSpare2.ability[0].times;
                    AI_damages[0] = Convert.ToInt32(AI_amt);
                }
                AI_amt = 50;
                if (R3STAT[0] == '1') AI_amt += 4;
                if (L3STAT[1] == '1') AI_amt += 4;
                if (R3STAT[2] == '1') AI_amt *= 1.4f;
                if (L3STAT[3] == '1') AI_amt *= 1.4f;
                if (L3STAT[4] == '1') AI_amt *= 0.7f;
                if (R3STAT[5] == '1') AI_amt *= 0.7f;
                if (L3STAT[6] == '1') AI_amt -= 4;
                if (R3STAT[7] == '1') AI_amt -= 4;
                AI_amt = Math.Max(0, AI_amt - enemyUnit.phsArmor);
                AI_damages[4] = Convert.ToInt32(AI_amt);

                if (actionAI < 4)
                {
                    AI_amt = enemyUnitSpare2.ability[actionAI].damage;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    if (enemyUnitSpare2.ability[actionAI].type == "PHYSICAL") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "LIGHT") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.lgtArmor);
                    if (enemyUnitSpare2.ability[actionAI].type == "DARK") AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.drkArmor);
                    AI_amt *= enemyUnitSpare2.ability[actionAI].times;
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }

                if (actionAI == 6)
                {
                    AI_amt = 50;
                    if (L3STAT[0] == '1') AI_amt += 4;
                    if (R3STAT[1] == '1') AI_amt += 4;
                    if (L3STAT[2] == '1') AI_amt *= 1.4f;
                    if (R3STAT[3] == '1') AI_amt *= 1.4f;
                    if (R3STAT[4] == '1') AI_amt *= 0.7f;
                    if (L3STAT[5] == '1') AI_amt *= 0.7f;
                    if (R3STAT[6] == '1') AI_amt -= 4;
                    if (L3STAT[7] == '1') AI_amt -= 4;
                    AI_amt = Math.Max(0, AI_amt - playerUnitSpare2.phsArmor);
                    AI_damages[5] = Convert.ToInt32(AI_amt);
                }
            }
        }

        //Tutaj wyliczamy koszty energii i szybkoœci ataków wykonywanych przez obu graczy
        if (activeL == 0)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                AI_MPLoses[AI_iterator] = playerUnit.ability[AI_iterator].cost;

                AI_Speeds[AI_iterator] = playerUnit.ability[AI_iterator].speed;
                if (L1STAT[8] == '1') AI_Speeds[AI_iterator] += 2;
                if (L1STAT[9] == '1') AI_Speeds[AI_iterator] -= 2;
            }
        }
        else if (activeL == 1)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                AI_MPLoses[AI_iterator] = playerUnitSpare1.ability[AI_iterator].cost;

                AI_Speeds[AI_iterator] = playerUnitSpare1.ability[AI_iterator].speed;
                if (L2STAT[8] == '1') AI_Speeds[AI_iterator] += 2;
                if (L2STAT[9] == '1') AI_Speeds[AI_iterator] -= 2;
            }
        }
        else if (activeL == 2)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                AI_MPLoses[AI_iterator] = playerUnitSpare2.ability[AI_iterator].cost;

                AI_Speeds[AI_iterator] = playerUnitSpare2.ability[AI_iterator].speed;
                if (L3STAT[8] == '1') AI_Speeds[AI_iterator] += 2;
                if (L3STAT[9] == '1') AI_Speeds[AI_iterator] -= 2;
            }
        }
        if (activeR == 0)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                if (actionAI < 4) AI_MPLoses[4] = enemyUnit.ability[actionAI].cost; else AI_MPLoses[4] = 0;

                if (actionAI < 4) AI_Speeds[4] = playerUnit.ability[actionAI].speed; else AI_Speeds[4] = 0;
                if (R1STAT[8] == '1') AI_Speeds[actionAI] += 2;
                if (R1STAT[9] == '1') AI_Speeds[actionAI] -= 2;
            }
        }
        else if (activeR == 1)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                if (actionAI < 4) AI_MPLoses[4] = enemyUnitSpare1.ability[actionAI].cost; else AI_MPLoses[4] = 0;

                if (actionAI < 4) AI_Speeds[4] = playerUnitSpare1.ability[actionAI].speed; else AI_Speeds[4] = 0;
                if (R2STAT[8] == '1') AI_Speeds[actionAI] += 2;
                if (R2STAT[9] == '1') AI_Speeds[actionAI] -= 2;
            }
        }
        else if (activeR == 2)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                if (actionAI < 4) AI_MPLoses[4] = enemyUnitSpare2.ability[actionAI].cost; else AI_MPLoses[4] = 0;

                if (actionAI < 4) AI_Speeds[4] = playerUnitSpare2.ability[actionAI].speed; else AI_Speeds[4] = 0;
                if (R3STAT[8] == '1') AI_Speeds[actionAI] += 2;
                if (R3STAT[9] == '1') AI_Speeds[actionAI] -= 2;
            }
        }

        //Gracz komputerowy wylicza wartoœci poszczególnych ruchów i celuje w wartoœæ najwy¿sz¹. 
        float[] AI_points = new float[8];
        int HPL = L1HP + L2HP + L3HP;
        int HPR = R1HP + R2HP + R3HP;

        int[] LHP = new int[3];
        LHP[0] = L1HP;
        LHP[1] = L2HP;
        LHP[2] = L3HP;
        int[] RHP = new int[3];
        RHP[0] = R1HP;
        RHP[1] = R2HP;
        RHP[2] = R3HP;
        int[] LMP = new int[3];
        LMP[0] = L1MP;
        LMP[1] = L2MP;
        LMP[2] = L3MP;
        int[] RMP = new int[3];
        RMP[0] = R1MP;
        RMP[1] = R2MP;
        RMP[2] = R3MP;
        string[] LSTAT = new string[3];
        LSTAT[0] = L1STAT;
        LSTAT[1] = L2STAT;
        LSTAT[2] = L3STAT;
        string[] RSTAT = new string[3];
        RSTAT[0] = R1STAT;
        RSTAT[1] = R2STAT;
        RSTAT[2] = R3STAT;
        UnitScript[] AI_playerUnit = new UnitScript[3];
        AI_playerUnit[0] = playerUnit;
        AI_playerUnit[1] = playerUnitSpare1;
        AI_playerUnit[2] = playerUnitSpare2;
        UnitScript[] AI_enemyUnit = new UnitScript[3];
        AI_enemyUnit[0] = enemyUnit;
        AI_enemyUnit[1] = enemyUnitSpare1;
        AI_enemyUnit[2] = enemyUnitSpare2;

        if (actionAI < 4)
        {
            if (AI_MPLoses[4] > RMP[activeR])
            {
                //¯eby gracz komputerowy nie próbowa³ u¿yæ akcji, na któr¹ brakuje mu energii.
                return bardzo_mala_wartosc;
            }
            else
            {
                for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
                {
                    if (AI_Speeds[4] > AI_Speeds[AI_iterator] && AI_damages[5] > LHP[activeL])
                    {
                        AI_points[AI_iterator] = HPR - HPL + LHP[activeL] + AI_KillBonus - (AI_MPLoses[4] / AI_ManaMultiplier);
                        if (LHP[(activeL + 1) % 3] > 0 && pozostale_iteracje > 1)
                        {
                            char[] RSTAT1new = new char[11];
                            for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                            string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                                if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                                if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                                if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                                if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                                if (feature == "BOLSTER") RSTAT1new[7] = '1';
                                if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                                if (feature == "HASTE") RSTAT1new[8] = '1';
                                if (feature == "SLOW") RSTAT1new[9] = '1';
                                if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                                if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                            }
                            string RSTATnew = RSTAT1new.ToString();
                            AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, 1, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                        else if (LHP[(activeL + 2) % 3] > 0 && pozostale_iteracje > 1)
                        {
                            char[] RSTAT1new = new char[11];
                            for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                            string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                                if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                                if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                                if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                                if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                                if (feature == "BOLSTER") RSTAT1new[7] = '1';
                                if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                                if (feature == "HASTE") RSTAT1new[8] = '1';
                                if (feature == "SLOW") RSTAT1new[9] = '1';
                                if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                                if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                            }
                            string RSTATnew = RSTAT1new.ToString();
                            AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, 2, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                        else
                        {
                            return bardzo_duza_wartosc; //To oznacza, ¿e wyeliminowaliœmy ostatniego gracza, czyli osi¹gamy zwyciêstwo.
                        }
                    }
                    else if (AI_Speeds[4] < AI_Speeds[AI_iterator] && AI_damages[AI_iterator] > RHP[0])
                    {
                        AI_points[AI_iterator] = HPR - HPL - RHP[activeR] - AI_KillBonus;
                        if (pozostale_iteracje > 1)
                        {
                            if (RHP[(activeR + 1) % 3] > 0)
                            {
                                char[] LSTAT1new = new char[11];
                                for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                                string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                                //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                                foreach (string feature in featureList)
                                {
                                    if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; else LSTAT1new[10] = '0';
                                    if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                                    if (feature == "STRENGTH") LSTAT1new[3] = '1'; else LSTAT1new[3] = '0';
                                    if (feature == "IMPAIR") LSTAT1new[4] = '1'; else LSTAT1new[4] = '0';
                                    if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                                    if (feature == "BOLSTER") LSTAT1new[7] = '1';
                                    if (feature == "FRAILTY") LSTAT1new[0] = '1'; else LSTAT1new[0] = '0';
                                    if (feature == "HASTE") LSTAT1new[8] = '1';
                                    if (feature == "SLOW") LSTAT1new[9] = '1';
                                    if (feature == "POWER") LSTAT1new[1] = '1'; else LSTAT1new[1] = '0';
                                    if (feature == "WEAKNESS") LSTAT1new[6] = '1'; else LSTAT1new[6] = '0';
                                }
                                string LSTATnew = LSTAT1new.ToString();
                                AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTATnew, LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                            }
                            else if (LHP[(activeL + 2) % 3] > 0)
                            {
                                char[] LSTAT1new = new char[11];
                                for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                                string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                                //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                                foreach (string feature in featureList)
                                {
                                    if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; else LSTAT1new[10] = '0';
                                    if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                                    if (feature == "STRENGTH") LSTAT1new[3] = '1'; else LSTAT1new[3] = '0';
                                    if (feature == "IMPAIR") LSTAT1new[4] = '1'; else LSTAT1new[4] = '0';
                                    if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                                    if (feature == "BOLSTER") LSTAT1new[7] = '1';
                                    if (feature == "FRAILTY") LSTAT1new[0] = '1'; else LSTAT1new[0] = '0';
                                    if (feature == "HASTE") LSTAT1new[8] = '1';
                                    if (feature == "SLOW") LSTAT1new[9] = '1';
                                    if (feature == "POWER") LSTAT1new[1] = '1'; else LSTAT1new[1] = '0';
                                    if (feature == "WEAKNESS") LSTAT1new[6] = '1'; else LSTAT1new[6] = '0';
                                }
                                string LSTATnew = LSTAT1new.ToString();
                                AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTATnew, LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                            }
                            else
                            {
                                return bardzo_duza_wartosc; //To oznacza, ¿e wyeliminowaliœmy ostatniego gracza, czyli osi¹gamy zwyciêstwo.
                            }
                        }
                    }
                    else
                    {
                        AI_points[AI_iterator] = HPR - HPL + AI_damages[5] - AI_damages[AI_iterator] + (AI_MPLoses[AI_iterator] - AI_MPLoses[4]) / AI_ManaMultiplier;
                        if (pozostale_iteracje > 1)
                        {
                            char[] LSTAT1new = new char[11];
                            char[] RSTAT1new = new char[11];
                            for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = '0';
                            for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = '0';
                            string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; ;
                                if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                                if (feature == "STRENGTH") RSTAT1new[3] = '1';
                                if (feature == "IMPAIR") RSTAT1new[4] = '1';
                                if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                                if (feature == "BOLSTER") RSTAT1new[7] = '1';
                                if (feature == "FRAILTY") RSTAT1new[0] = '1';
                                if (feature == "HASTE") RSTAT1new[8] = '1';
                                if (feature == "SLOW") RSTAT1new[9] = '1';
                                if (feature == "POWER") RSTAT1new[1] = '1';
                                if (feature == "WEAKNESS") RSTAT1new[6] = '1';
                            }
                            featureList = AI_playerUnit[activeL].ability[AI_iterator].banes.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; ;
                                if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                                if (feature == "STRENGTH") RSTAT1new[3] = '1';
                                if (feature == "IMPAIR") RSTAT1new[4] = '1';
                                if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                                if (feature == "BOLSTER") RSTAT1new[7] = '1';
                                if (feature == "FRAILTY") RSTAT1new[0] = '1';
                                if (feature == "HASTE") RSTAT1new[8] = '1';
                                if (feature == "SLOW") RSTAT1new[9] = '1';
                                if (feature == "POWER") RSTAT1new[1] = '1';
                                if (feature == "WEAKNESS") RSTAT1new[6] = '1';
                            }
                            featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; ;
                                if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                                if (feature == "STRENGTH") LSTAT1new[3] = '1';
                                if (feature == "IMPAIR") LSTAT1new[4] = '1';
                                if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                                if (feature == "BOLSTER") LSTAT1new[7] = '1';
                                if (feature == "FRAILTY") LSTAT1new[0] = '1';
                                if (feature == "HASTE") LSTAT1new[8] = '1';
                                if (feature == "SLOW") LSTAT1new[9] = '1';
                                if (feature == "POWER") LSTAT1new[1] = '1';
                                if (feature == "WEAKNESS") LSTAT1new[6] = '1';
                            }
                            featureList = AI_enemyUnit[activeR].ability[actionAI].banes.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; ;
                                if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                                if (feature == "STRENGTH") LSTAT1new[3] = '1';
                                if (feature == "IMPAIR") LSTAT1new[4] = '1';
                                if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                                if (feature == "BOLSTER") LSTAT1new[7] = '1';
                                if (feature == "FRAILTY") LSTAT1new[0] = '1';
                                if (feature == "HASTE") LSTAT1new[8] = '1';
                                if (feature == "SLOW") LSTAT1new[9] = '1';
                                if (feature == "POWER") LSTAT1new[1] = '1';
                                if (feature == "WEAKNESS") LSTAT1new[6] = '1';
                            }
                            string LSTATnew = LSTAT1new.ToString();
                            string RSTATnew = RSTAT1new.ToString();
                            AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] - AI_damages[5], LMP[0] - AI_MPLoses[AI_iterator], LSTATnew, LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] - AI_damages[AI_iterator], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                    }
                }

                if (AI_damages[5] > LHP[activeL])
                {
                    AI_points[4] = HPR - HPL + LHP[activeL] + AI_KillBonus - (AI_MPLoses[4] / AI_ManaMultiplier);
                    if (LHP[(activeL + 1) % 3] > 0 && pozostale_iteracje > 1)
                    {
                        char[] RSTAT1new = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                        string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                            if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                            if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                            if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                            if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                            if (feature == "BOLSTER") RSTAT1new[7] = '1';
                            if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                            if (feature == "HASTE") RSTAT1new[8] = '1';
                            if (feature == "SLOW") RSTAT1new[9] = '1';
                            if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                            if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                        }
                        string RSTATnew = RSTAT1new.ToString();
                        AI_points[5] += AI_EvaluateSingle(actionAIqueue, 1, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                    else if (LHP[(activeL + 2) % 3] > 0 && pozostale_iteracje > 1)
                    {
                        char[] RSTAT1new = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                        string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                            if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                            if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                            if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                            if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                            if (feature == "BOLSTER") RSTAT1new[7] = '1';
                            if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                            if (feature == "HASTE") RSTAT1new[8] = '1';
                            if (feature == "SLOW") RSTAT1new[9] = '1';
                            if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                            if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                        }
                        string RSTATnew = RSTAT1new.ToString();
                        AI_points[5] += AI_EvaluateSingle(actionAIqueue, 2, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                    else
                    {
                        return bardzo_duza_wartosc; //To oznacza, ¿e wyeliminowaliœmy ostatniego gracza, czyli osi¹gamy zwyciêstwo.
                    }
                }
                else
                {
                    AI_points[4] = HPR - HPL + AI_damages[5] - (2 + 5 / AI_ManaMultiplier) - AI_MPLoses[4] / AI_ManaMultiplier;
                    if (pozostale_iteracje > 1)
                    {
                        char[] LSTAT1new = new char[11];
                        char[] RSTAT1new = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = '0';
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = '0';
                        string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; ;
                            if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                            if (feature == "STRENGTH") RSTAT1new[3] = '1';
                            if (feature == "IMPAIR") RSTAT1new[4] = '1';
                            if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                            if (feature == "BOLSTER") RSTAT1new[7] = '1';
                            if (feature == "FRAILTY") RSTAT1new[0] = '1';
                            if (feature == "HASTE") RSTAT1new[8] = '1';
                            if (feature == "SLOW") RSTAT1new[9] = '1';
                            if (feature == "POWER") RSTAT1new[1] = '1';
                            if (feature == "WEAKNESS") RSTAT1new[6] = '1';
                        }
                        featureList = AI_enemyUnit[activeR].ability[actionAI].banes.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; ;
                            if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                            if (feature == "STRENGTH") LSTAT1new[3] = '1';
                            if (feature == "IMPAIR") LSTAT1new[4] = '1';
                            if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                            if (feature == "BOLSTER") LSTAT1new[7] = '1';
                            if (feature == "FRAILTY") LSTAT1new[0] = '1';
                            if (feature == "HASTE") LSTAT1new[8] = '1';
                            if (feature == "SLOW") LSTAT1new[9] = '1';
                            if (feature == "POWER") LSTAT1new[1] = '1';
                            if (feature == "WEAKNESS") LSTAT1new[6] = '1';
                        }
                        string LSTATnew = LSTAT1new.ToString();
                        string RSTATnew = RSTAT1new.ToString();
                        AI_points[4] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] - AI_damages[5] + 2, LMP[0] + 5, LSTATnew, LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }

                AI_points[5] = HPR - HPL - AI_MPLoses[4] / AI_ManaMultiplier;
                if (pozostale_iteracje > 1)
                {
                    char[] RSTAT1new = new char[11];
                    for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                    string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                    //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                    foreach (string feature in featureList)
                    {
                        if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                        if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                        if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                        if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                        if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                        if (feature == "BOLSTER") RSTAT1new[7] = '1';
                        if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                        if (feature == "HASTE") RSTAT1new[8] = '1';
                        if (feature == "SLOW") RSTAT1new[9] = '1';
                        if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                        if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                    }
                    string RSTATnew = RSTAT1new.ToString();
                    AI_points[5] += AI_EvaluateSingle(actionAIqueue, (activeL + 1) % 3, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[7] = HPR - HPL - AI_MPLoses[4] / AI_ManaMultiplier;
                if (pozostale_iteracje > 1)
                {
                    char[] RSTAT1new = new char[11];
                    for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                    string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                    //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                    foreach (string feature in featureList)
                    {
                        if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                        if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                        if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                        if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                        if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                        if (feature == "BOLSTER") RSTAT1new[7] = '1';
                        if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                        if (feature == "HASTE") RSTAT1new[8] = '1';
                        if (feature == "SLOW") RSTAT1new[9] = '1';
                        if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                        if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                    }
                    string RSTATnew = RSTAT1new.ToString();
                    AI_points[7] += AI_EvaluateSingle(actionAIqueue, (activeL + 2) % 3, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTATnew, RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }

                if (AI_damages[5] >= LHP[activeL])
                {
                    AI_points[6] = HPR - HPL + LHP[activeL] + AI_KillBonus - (AI_MPLoses[4] / AI_ManaMultiplier);
                    char[] RSTAT1new = RSTAT[activeR].ToCharArray();
                    RSTAT1new[10] = '0';
                    RSTAT1new[3] = '0';
                    RSTAT1new[4] = '0';
                    RSTAT1new[1] = '0';
                    RSTAT1new[6] = '0';
                    RSTAT[activeR] = RSTAT1new.ToString();
                    if (LHP[(activeL + 1) % 3] > 0 && pozostale_iteracje > 1) AI_points[6] += AI_EvaluateSingle(actionAIqueue, (activeL + 1) % 3, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[1] - AI_MPLoses[4], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    else if (LHP[(activeL + 2) % 3] > 0 && pozostale_iteracje > 1) AI_points[6] += AI_EvaluateSingle(actionAIqueue, (activeL + 2) % 3, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[1] - AI_MPLoses[4], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    else AI_points[6] = bardzo_duza_wartosc;
                }
                else
                {
                    char[] RSTAT1new = new char[11];
                    for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                    string[] featureList = AI_enemyUnit[activeR].ability[actionAI].boons.Split('|');
                    //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                    foreach (string feature in featureList)
                    {
                        if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                        if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                        if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                        if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                        if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                        if (feature == "BOLSTER") RSTAT1new[7] = '1';
                        if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                        if (feature == "HASTE") RSTAT1new[8] = '1';
                        if (feature == "SLOW") RSTAT1new[9] = '1';
                        if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                        if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                    }
                    RSTAT[activeR] = RSTAT1new.ToString();

                    for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) RSTAT1new[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                    featureList = AI_enemyUnit[activeR].ability[actionAI].banes.Split('|');
                    //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                    foreach (string feature in featureList)
                    {
                        if (feature == "INTERRUPTED") RSTAT1new[10] = '1'; else RSTAT1new[10] = '0';
                        if (feature == "VULNERABILITY") RSTAT1new[2] = '1';
                        if (feature == "STRENGTH") RSTAT1new[3] = '1'; else RSTAT1new[3] = '0';
                        if (feature == "IMPAIR") RSTAT1new[4] = '1'; else RSTAT1new[4] = '0';
                        if (feature == "RESISTANCE") RSTAT1new[5] = '1';
                        if (feature == "BOLSTER") RSTAT1new[7] = '1';
                        if (feature == "FRAILTY") RSTAT1new[0] = '1'; else RSTAT1new[0] = '0';
                        if (feature == "HASTE") RSTAT1new[8] = '1';
                        if (feature == "SLOW") RSTAT1new[9] = '1';
                        if (feature == "POWER") RSTAT1new[1] = '1'; else RSTAT1new[1] = '0';
                        if (feature == "WEAKNESS") RSTAT1new[6] = '1'; else RSTAT1new[6] = '0';
                    }
                    LSTAT[activeL] = RSTAT1new.ToString();

                    AI_points[6] = HPR - HPL + AI_damages[5] - (2 + 5 / AI_ManaMultiplier) - AI_MPLoses[4] / AI_ManaMultiplier;
                    if (pozostale_iteracje > 1) AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] - AI_damages[5], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0] - AI_MPLoses[4], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
            }
        }
        if (actionAI == 4)
        {
            for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
            {
                if (AI_damages[AI_iterator] > RHP[activeR])
                {
                    AI_points[AI_iterator] = HPR - HPL - RHP[activeR] - AI_KillBonus + AI_MPLoses[AI_iterator];

                    if (RHP[(activeR + 1) % 3] > 0 && pozostale_iteracje > 1)
                    {
                        char[] LSTAT1new = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                        string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; else LSTAT1new[10] = '0';
                            if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                            if (feature == "STRENGTH") LSTAT1new[3] = '1'; else LSTAT1new[3] = '0';
                            if (feature == "IMPAIR") LSTAT1new[4] = '1'; else LSTAT1new[4] = '0';
                            if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                            if (feature == "BOLSTER") LSTAT1new[7] = '1';
                            if (feature == "FRAILTY") LSTAT1new[0] = '1'; else LSTAT1new[0] = '0';
                            if (feature == "HASTE") LSTAT1new[8] = '1';
                            if (feature == "SLOW") LSTAT1new[9] = '1';
                            if (feature == "POWER") LSTAT1new[1] = '1'; else LSTAT1new[1] = '0';
                            if (feature == "WEAKNESS") LSTAT1new[6] = '1'; else LSTAT1new[6] = '0';
                        }
                        LSTAT[activeL] = LSTAT1new.ToString();
                        AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                    else if (RHP[(activeR + 2) % 3] > 0 && pozostale_iteracje > 1)
                    {
                        char[] LSTAT1new = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) LSTAT1new[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                        string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") LSTAT1new[10] = '1'; else LSTAT1new[10] = '0';
                            if (feature == "VULNERABILITY") LSTAT1new[2] = '1';
                            if (feature == "STRENGTH") LSTAT1new[3] = '1'; else LSTAT1new[3] = '0';
                            if (feature == "IMPAIR") LSTAT1new[4] = '1'; else LSTAT1new[4] = '0';
                            if (feature == "RESISTANCE") LSTAT1new[5] = '1';
                            if (feature == "BOLSTER") LSTAT1new[7] = '1';
                            if (feature == "FRAILTY") LSTAT1new[0] = '1'; else LSTAT1new[0] = '0';
                            if (feature == "HASTE") LSTAT1new[8] = '1';
                            if (feature == "SLOW") LSTAT1new[9] = '1';
                            if (feature == "POWER") LSTAT1new[1] = '1'; else LSTAT1new[1] = '0';
                            if (feature == "WEAKNESS") LSTAT1new[6] = '1'; else LSTAT1new[6] = '0';
                        }
                        LSTAT[activeL] = LSTAT1new.ToString();
                        AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                    else
                    {
                        AI_points[AI_iterator] = -65535;
                    }
                }
                else
                {
                    AI_points[AI_iterator] = HPR - HPL - AI_damages[AI_iterator] + AI_MPLoses[AI_iterator] + (2 + 5 / AI_ManaMultiplier);
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) STAT[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                        string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                            if (feature == "VULNERABILITY") STAT[2] = '1';
                            if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                            if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                            if (feature == "RESISTANCE") STAT[5] = '1';
                            if (feature == "BOLSTER") STAT[7] = '1';
                            if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                            if (feature == "HASTE") STAT[8] = '1';
                            if (feature == "SLOW") STAT[9] = '1';
                            if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                            if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                        }
                        LSTAT[activeL] = STAT.ToString();

                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) STAT[AI_iterator_str] = RSTAT[activeR][AI_iterator_str];
                        featureList = AI_playerUnit[activeL].ability[AI_iterator].banes.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                            if (feature == "VULNERABILITY") STAT[2] = '1';
                            if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                            if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                            if (feature == "RESISTANCE") STAT[5] = '1';
                            if (feature == "BOLSTER") STAT[7] = '1';
                            if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                            if (feature == "HASTE") STAT[8] = '1';
                            if (feature == "SLOW") STAT[9] = '1';
                            if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                            if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                        }
                        RSTAT[activeR] = STAT.ToString();
                        AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] - AI_damages[AI_iterator] + 2, RMP[0] + 5, RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
            }
            AI_points[4] = HPR - HPL + (RMP[activeR] - LMP[activeL]) / AI_ManaMultiplier;
            if (pozostale_iteracje > 1)
            {
                AI_points[4] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] + 2, LMP[0] + 5, LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] + 2, RMP[0] + 5, RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
            }
            AI_points[5] = HPR - HPL + (2 + 5 / AI_ManaMultiplier);
            if (pozostale_iteracje > 1)
            {
                AI_points[5] += AI_EvaluateSingle(actionAIqueue, 1, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] + 2, RMP[0] + 5, RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
            }
            AI_points[6] = HPR - HPL + (2 + 5 / AI_ManaMultiplier);
            if (pozostale_iteracje > 1)
            {
                AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] + 2, RMP[0] + 5, RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
            }
            AI_points[7] = HPR - HPL + (2 + 5 / AI_ManaMultiplier);
            if (pozostale_iteracje > 1)
            {
                AI_points[7] += AI_EvaluateSingle(actionAIqueue, 2, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] + 2, RMP[0] + 5, RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
            }
        }
        else if (actionAI == 5)
        {
            if (RHP[(activeR + 1) % 3] <= 0)
            {
                //Niska wartoœæ, ¿eby przypadkiem gracz komputerowy nie chcia³ wykonaæ akcji, której nie mo¿e.
                return bardzo_mala_wartosc;
            }
            else
            {
                for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
                {
                    AI_points[AI_iterator] = HPR - HPL + AI_MPLoses[AI_iterator] / AI_ManaMultiplier;
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) STAT[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                        string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                            if (feature == "VULNERABILITY") STAT[2] = '1';
                            if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                            if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                            if (feature == "RESISTANCE") STAT[5] = '1';
                            if (feature == "BOLSTER") STAT[7] = '1';
                            if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                            if (feature == "HASTE") STAT[8] = '1';
                            if (feature == "SLOW") STAT[9] = '1';
                            if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                            if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                        }
                        LSTAT[activeL] = STAT.ToString();
                        AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
                AI_points[4] = HPR - HPL - 2 - 5 / AI_ManaMultiplier;
                if (pozostale_iteracje > 1)
                {
                    AI_points[4] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0] + 2, LMP[0] + 5, LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[5] = HPR - HPL + RMP[0] + RMP[1] + RMP[2] - LMP[0] - LMP[1] - LMP[2];
                if (pozostale_iteracje > 1)
                {
                    AI_points[5] += AI_EvaluateSingle(actionAIqueue, 1, 1, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[7] = HPR - HPL + RMP[0] + RMP[1] + RMP[2] - LMP[0] - LMP[1] - LMP[2];
                if (pozostale_iteracje > 1)
                {
                    AI_points[7] += AI_EvaluateSingle(actionAIqueue, 2, 1, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                if (RHP[activeR] <= AI_damages[4])
                {
                    AI_points[6] = HPR - HPL - LHP[activeL] - AI_KillBonus;
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = LSTAT[activeL].ToCharArray();
                        STAT[3] = '0';
                        STAT[4] = '0';
                        STAT[8] = '0';
                        STAT[9] = '0';
                        STAT[1] = '0';
                        STAT[6] = '0';
                        LSTAT[activeL] = STAT.ToString();
                        AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
                else
                {
                    AI_points[6] = HPR - HPL - AI_damages[4];
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = LSTAT[activeL].ToCharArray();
                        STAT[3] = '0';
                        STAT[4] = '0';
                        STAT[8] = '0';
                        STAT[9] = '0';
                        STAT[1] = '0';
                        STAT[6] = '0';
                        LSTAT[activeL] = STAT.ToString();
                        STAT = RSTAT[activeR].ToCharArray();
                        STAT[2] = '0';
                        STAT[5] = '0';
                        STAT[7] = '0';
                        STAT[0] = '0';
                        RSTAT[activeR] = STAT.ToString();
                        AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] - AI_damages[4], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
            }
        }
        else if (actionAI == 6)
        {
            if (LHP[(activeL + 2) % 3] <= 0 || LHP[(activeL + 1) % 3] <= 0)
            {
                //Przechwytywanie, gdy wróg nie mo¿e siê zmieniaæ, nie ma absolutnie ¿adnego sensu, dlatego gracz komputerowy nigdy nie powinien tego robiæ.
                return bardzo_mala_wartosc;
            }
            else
            {
                for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
                {
                    if (AI_damages[AI_iterator] > RHP[activeR])
                    {
                        AI_points[AI_iterator] = HPR - HPL - RHP[activeR] - AI_KillBonus + (AI_MPLoses[AI_iterator] / AI_ManaMultiplier);
                        if (RHP[(activeR + 1) % 3] == 0 && RHP[(activeR + 2) % 3] == 0)
                        {
                            AI_points[AI_iterator] = -65535;
                        }
                        if (pozostale_iteracje > 1)
                        {
                            if (RHP[(activeR + 1) % 3] > 0)
                            {
                                char[] STAT = LSTAT[activeL].ToCharArray();
                                string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                                //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                                foreach (string feature in featureList)
                                {
                                    if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                                    if (feature == "VULNERABILITY") STAT[2] = '1';
                                    if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                                    if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                                    if (feature == "RESISTANCE") STAT[5] = '1';
                                    if (feature == "BOLSTER") STAT[7] = '1';
                                    if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                                    if (feature == "HASTE") STAT[8] = '1';
                                    if (feature == "SLOW") STAT[9] = '1';
                                    if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                                    if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                                }
                                LSTAT[activeL] = STAT.ToString();
                                AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                            }
                            else
                            {
                                char[] STAT = LSTAT[activeL].ToCharArray();
                                string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                                //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                                foreach (string feature in featureList)
                                {
                                    if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                                    if (feature == "VULNERABILITY") STAT[2] = '1';
                                    if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                                    if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                                    if (feature == "RESISTANCE") STAT[5] = '1';
                                    if (feature == "BOLSTER") STAT[7] = '1';
                                    if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                                    if (feature == "HASTE") STAT[8] = '1';
                                    if (feature == "SLOW") STAT[9] = '1';
                                    if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                                    if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                                }
                                LSTAT[activeL] = STAT.ToString();
                                AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                            }
                        }
                    }
                    else
                    {
                        AI_points[AI_iterator] = HPR - HPL - AI_damages[AI_iterator] + (AI_MPLoses[AI_iterator] / AI_ManaMultiplier);
                        if (pozostale_iteracje > 1)
                        {
                            char[] STAT = LSTAT[activeL].ToCharArray();
                            string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                                if (feature == "VULNERABILITY") STAT[2] = '1';
                                if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                                if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                                if (feature == "RESISTANCE") STAT[5] = '1';
                                if (feature == "BOLSTER") STAT[7] = '1';
                                if (feature == "FRAILTY") STAT[0] = '1';
                                if (feature == "HASTE") STAT[8] = '1';
                                if (feature == "SLOW") STAT[9] = '1';
                                if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                                if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                            }
                            LSTAT[activeL] = STAT.ToString();

                            STAT = RSTAT[activeR].ToCharArray();
                            featureList = AI_playerUnit[activeL].ability[AI_iterator].banes.Split('|');
                            foreach (string feature in featureList)
                            {
                                if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                                if (feature == "VULNERABILITY") STAT[2] = '1'; else STAT[2] = '0';
                                if (feature == "STRENGTH") STAT[3] = '1';
                                if (feature == "IMPAIR") STAT[4] = '1';
                                if (feature == "RESISTANCE") STAT[5] = '1'; else STAT[5] = '0';
                                if (feature == "BOLSTER") STAT[7] = '1'; else STAT[7] = '0';
                                if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                                if (feature == "HASTE") STAT[8] = '1';
                                if (feature == "SLOW") STAT[9] = '1';
                                if (feature == "POWER") STAT[1] = '1';
                                if (feature == "WEAKNESS") STAT[6] = '1';
                            }
                            RSTAT[activeR] = STAT.ToString();

                            AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] - AI_damages[AI_iterator], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                        
                    }
                }
                AI_points[4] = HPR - HPL - 2 - 5 / AI_ManaMultiplier;
                if (pozostale_iteracje > 1)
                {
                    AI_points[4] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] + 2, LMP[0] + 5, LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[6] = HPR - HPL;
                if (pozostale_iteracje > 1)
                {
                    AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[0], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                if (AI_damages[5] >= LHP[activeL])
                {
                    AI_points[5] = HPR - HPL + LHP[activeL] + AI_KillBonus;
                    if (LHP[(activeL + 1) % 3] == 0 && LHP[(activeL + 2) % 3] == 0)
                    {
                        return bardzo_duza_wartosc;
                    }
                    else if (pozostale_iteracje > 1)
                    {
                        if (LHP[(activeL + 1) % 3] > 0)
                        {
                            char[] STAT = RSTAT[activeR].ToCharArray();
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            STAT[1] = '0';
                            STAT[3] = '0';
                            STAT[4] = '0';
                            STAT[6] = '0';
                            RSTAT[activeR] = STAT.ToString();
                            AI_points[5] += AI_EvaluateSingle(actionAIqueue, 1, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                        else
                        {
                            char[] STAT = RSTAT[activeR].ToCharArray();
                            //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                            STAT[1] = '0';
                            STAT[3] = '0';
                            STAT[4] = '0';
                            STAT[6] = '0';
                            RSTAT[activeR] = STAT.ToString();
                            AI_points[5] += AI_EvaluateSingle(actionAIqueue, 2, activeR, 0, 0, "00000000000", LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                    }
                }
                else
                {
                    AI_points[5] = HPR - HPL + AI_damages[5];
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = RSTAT[activeR].ToCharArray();
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        STAT[1] = '0';
                        STAT[3] = '0';
                        STAT[4] = '0';
                        STAT[6] = '0';
                        RSTAT[activeR] = STAT.ToString();

                        STAT = LSTAT[activeL].ToCharArray();
                        STAT[0] = '0';
                        STAT[2] = '0';
                        STAT[5] = '0';
                        STAT[7] = '0';
                        LSTAT[activeL] = STAT.ToString();
                        AI_points[5] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0] - AI_damages[5], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
                AI_points[7] = AI_points[5];
            }

        }
        else if (actionAI == 8)
        {
            if (RHP[(activeR + 2) % 3] <= 0)
            {
                //Niska wartoœæ, ¿eby przypadkiem gracz komputerowy nie chcia³ wykonaæ akcji, której nie mo¿e.
                return bardzo_mala_wartosc;
            }
            else
            {
                for (int AI_iterator = 0; AI_iterator < 4; AI_iterator++)
                {
                    AI_points[AI_iterator] = HPR - HPL + AI_MPLoses[AI_iterator] / AI_ManaMultiplier;
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = new char[11];
                        for (int AI_iterator_str = 0; AI_iterator_str < 11; AI_iterator_str++) STAT[AI_iterator_str] = LSTAT[activeL][AI_iterator_str];
                        string[] featureList = AI_playerUnit[activeL].ability[AI_iterator].boons.Split('|');
                        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
                        foreach (string feature in featureList)
                        {
                            if (feature == "INTERRUPTED") STAT[10] = '1'; else STAT[10] = '0';
                            if (feature == "VULNERABILITY") STAT[2] = '1';
                            if (feature == "STRENGTH") STAT[3] = '1'; else STAT[3] = '0';
                            if (feature == "IMPAIR") STAT[4] = '1'; else STAT[4] = '0';
                            if (feature == "RESISTANCE") STAT[5] = '1';
                            if (feature == "BOLSTER") STAT[7] = '1';
                            if (feature == "FRAILTY") STAT[0] = '1'; else STAT[0] = '0';
                            if (feature == "HASTE") STAT[8] = '1';
                            if (feature == "SLOW") STAT[9] = '1';
                            if (feature == "POWER") STAT[1] = '1'; else STAT[1] = '0';
                            if (feature == "WEAKNESS") STAT[6] = '1'; else STAT[6] = '0';
                        }
                        LSTAT[activeL] = STAT.ToString();
                        AI_points[AI_iterator] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0], LMP[0] - AI_MPLoses[AI_iterator], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
                AI_points[4] = HPR - HPL - 2 - 5 / AI_ManaMultiplier;
                if (pozostale_iteracje > 1)
                {
                    AI_points[4] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0] + 2, LMP[0] + 5, LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[5] = HPR - HPL + RMP[0] + RMP[1] + RMP[2] - LMP[0] - LMP[1] - LMP[2];
                if (pozostale_iteracje > 1)
                {
                    AI_points[5] += AI_EvaluateSingle(actionAIqueue, 1, 2, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                AI_points[7] = HPR - HPL + RMP[0] + RMP[1] + RMP[2] - LMP[0] - LMP[1] - LMP[2];
                if (pozostale_iteracje > 1)
                {
                    AI_points[7] += AI_EvaluateSingle(actionAIqueue, 2, 2, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                }
                if (RHP[activeR] <= AI_damages[4])
                {
                    AI_points[6] = HPR - HPL - LHP[0] - AI_KillBonus;
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = LSTAT[activeL].ToCharArray();
                        STAT[3] = '0';
                        STAT[4] = '0';
                        STAT[8] = '0';
                        STAT[9] = '0';
                        STAT[1] = '0';
                        STAT[6] = '0';
                        LSTAT[0] = STAT.ToString();
                        if (RHP[(activeR + 1) % 3] > 0)
                        {
                            AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, 1, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }
                        else
                        {
                            AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, 2, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], 0, 0, "00000000000", RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                        }

                    }
                }
                else
                {
                    AI_points[6] = HPR - HPL - AI_damages[4];
                    if (pozostale_iteracje > 1)
                    {
                        char[] STAT = LSTAT[activeL].ToCharArray();
                        STAT[3] = '0';
                        STAT[4] = '0';
                        STAT[8] = '0';
                        STAT[9] = '0';
                        STAT[1] = '0';
                        STAT[6] = '0';
                        LSTAT[activeL] = STAT.ToString();
                        STAT = RSTAT[activeR].ToCharArray();
                        STAT[2] = '0';
                        STAT[5] = '0';
                        STAT[7] = '0';
                        STAT[0] = '0';
                        RSTAT[activeR] = STAT.ToString();
                        AI_points[6] += AI_EvaluateSingle(actionAIqueue, activeL, activeR, LHP[0], LMP[0], LSTAT[0], LHP[1], LMP[1], LSTAT[1], LHP[2], LMP[2], LSTAT[2], RHP[0] - AI_damages[4], RMP[0], RSTAT[0], RHP[1], RMP[1], RSTAT[1], RHP[2], RMP[2], RSTAT[2], pozostale_iteracje - 1);
                    }
                }
            }
        }


        if (AI_ChooseMax == true) {
            int kontrolka = 0;
            for (int AI_iterator = 0; AI_iterator < 8; AI_iterator++)
            {
                if (AI_points[kontrolka] < AI_points[AI_iterator]) kontrolka = AI_iterator;
            }
            return AI_points[kontrolka];
        }
        else if (AI_ChooseMin == true)
        {
            int kontrolka = 0;
            for (int AI_iterator = 0; AI_iterator < 8; AI_iterator++)
            {
                if (AI_points[kontrolka] > AI_points[AI_iterator]) kontrolka = AI_iterator;
            }
            return AI_points[kontrolka];
        }
        else
        {
            int kontrolka = 0;
            float amt = 0;
            for (int AI_iterator = 0; AI_iterator < 8; AI_iterator++)
            {
                if (AI_points[AI_iterator] > -65536)
                {
                    kontrolka += 1;
                    amt = AI_points[AI_iterator];
                }
            }
            return amt / kontrolka;
        }
    }
    
    //AI_Evaluate zwraca indeks najlepszego ruchu, dokonuje wyceny wszystkich zestawów ruchów w acierzy, mnastêpnie wybiera najlepsz¹ kolejkê z nich
    int AI_Evaluate(int[][] evaluateMatrix)
    {
        float[] evaluated = new float[AI_pop];
        
        for (int Eval_iterator = 0; Eval_iterator < AI_pop; Eval_iterator++)
        {
            evaluated[Eval_iterator] = AI_EvaluateSingle(evaluateMatrix[Eval_iterator], 0, 0, playerUnit.curHP, playerUnit.curMana, AI_prepareSTAT(playerUnit), playerUnitSpare1.curHP, playerUnitSpare1.curMana, AI_prepareSTAT(playerUnitSpare1), playerUnitSpare2.curHP, playerUnitSpare2.curMana, AI_prepareSTAT(playerUnitSpare2), enemyUnit.curHP, enemyUnit.curMana, AI_prepareSTAT(enemyUnit), enemyUnitSpare1.curHP, enemyUnitSpare1.curMana, AI_prepareSTAT(enemyUnitSpare1), enemyUnitSpare2.curHP, enemyUnitSpare2.curMana, AI_prepareSTAT(enemyUnitSpare2), AI_n);
        }
        //Wybieramy najwy¿sz¹ wartoœæ, bo bêdzie to teoretycznie najlepszy ruch do wybrania
        int kontrolka_eval = 0;
        for (int Eval_iterator = 0; Eval_iterator < AI_pop; Eval_iterator++)
        {
            if (evaluated[Eval_iterator] > evaluated[kontrolka_eval]) kontrolka_eval = Eval_iterator;
        }
        return kontrolka_eval;
    }

    //Przemienia aktywne efekty statusu z postaci na ci¹g, który jest póŸniej wykorzystany przy wycenianiu punktowym AI_Evaluate i AI_EvaluateSingle
    string AI_prepareSTAT(UnitScript unit_p)
    {
        //Statusy rozpatrywane wed³ug indeksów: [0] = frailty, [1] = power, [2] = vulnerability, [3] = strength, [4] = impair, [5] = resistance, [6] = weak, [7] = bolster, [8] = haste, [9] = slow, [10] = interrupt
        char[] prepared = new char[11];
        if (unit_p.frailty == true) prepared[0] = '1'; else prepared[0] = '0';
        if (unit_p.power == true) prepared[1] = '1'; else prepared[1] = '0';
        if (unit_p.vulnerability == true) prepared[2] = '1'; else prepared[2] = '0';
        if (unit_p.strength == true) prepared[3] = '1'; else prepared[3] = '0';
        if (unit_p.impair == true) prepared[4] = '1'; else prepared[4] = '0';
        if (unit_p.resistance == true) prepared[5] = '1'; else prepared[5] = '0';
        if (unit_p.weak == true) prepared[6] = '1'; else prepared[6] = '0';
        if (unit_p.bolster == true) prepared[7] = '1'; else prepared[7] = '0';
        if (unit_p.haste == true) prepared[8] = '1'; else prepared[8] = '0';
        if (unit_p.slow == true) prepared[9] = '1'; else prepared[9] = '0';
        if (unit_p.interrupted == true) prepared[10] = '1'; else prepared[10] = '0';
        string prepared_s = new string(prepared);
        return prepared_s;
    }

    int[][] AI_Selection(int[][] AI_X)
    {
        int Selection_i = 0;
        while(Selection_i < AI_pop)
        {
            int[] Selection_i1 = AI_X[Random.Range(0, AI_pop - 1)];
            int[] Selection_i2 = AI_X[Random.Range(0, AI_pop - 1)];
            if (Selection_i1 != Selection_i2)
            {
                if (AI_EvaluateSingle(Selection_i1, 0, 0, playerUnit.curHP, playerUnit.curMana, AI_prepareSTAT(playerUnit), playerUnitSpare1.curHP, playerUnitSpare1.curMana, AI_prepareSTAT(playerUnitSpare1), playerUnitSpare2.curHP, playerUnitSpare2.curMana, AI_prepareSTAT(playerUnitSpare2), enemyUnit.curHP, enemyUnit.curMana, AI_prepareSTAT(enemyUnit), enemyUnitSpare1.curHP, enemyUnitSpare1.curMana, AI_prepareSTAT(enemyUnitSpare1), enemyUnitSpare2.curHP, enemyUnitSpare2.curMana, AI_prepareSTAT(enemyUnitSpare2), AI_n) <= AI_EvaluateSingle(Selection_i2, 0, 0, playerUnit.curHP, playerUnit.curMana, AI_prepareSTAT(playerUnit), playerUnitSpare1.curHP, playerUnitSpare1.curMana, AI_prepareSTAT(playerUnitSpare1), playerUnitSpare2.curHP, playerUnitSpare2.curMana, AI_prepareSTAT(playerUnitSpare2), enemyUnit.curHP, enemyUnit.curMana, AI_prepareSTAT(enemyUnit), enemyUnitSpare1.curHP, enemyUnitSpare1.curMana, AI_prepareSTAT(enemyUnitSpare1), enemyUnitSpare2.curHP, enemyUnitSpare2.curMana, AI_prepareSTAT(enemyUnitSpare2), AI_n))
                {
                    AI_X[Selection_i] = Selection_i1;
                }
                else
                {
                    AI_X[Selection_i] = Selection_i2;
                }
            }
            Selection_i += 1;
        }
        return AI_X;
    }

    int[][] AI_Crossover(int[][] AI_X)
    {
        int Crossover_i = 0;
        while(Crossover_i < AI_pop - 2)
        {
            if (Random.Range(0, 99) < p_c)
            {
                //AI_Cross(AI_X[Crossover_i], AI_X[Crossover_i + 1]);
                int rd1 = Random.Range(0, AI_n - 1);
                int rd2 = Random.Range(0, AI_n - 1);
                if (rd1 == rd2)
                {
                    rd2 = (rd1 + 1) % AI_n;
                    if (rd2 == AI_n) rd2 = Convert.ToInt32(AI_n / 2);
                }
                if (rd2 == AI_n - 1) rd2 = Convert.ToInt32(AI_n / 2);
                if (rd1 > rd2)
                {
                    int rd_temp = rd1;
                    rd1 = rd2;
                    rd2 = rd_temp;
                }
                int Cross_len = rd2 - rd1;
                int[] Cross_MappingList0 = new int[rd2 - rd1];
                int[] Cross_MappingList1 = new int[rd2 - rd1];
                for (int Cross_i = rd1; Cross_i < rd2; Cross_i++)
                {
                    Cross_MappingList0[Cross_i - rd1] = AI_X[Crossover_i][Cross_i];
                    Cross_MappingList1[Cross_i - rd1] = AI_X[Crossover_i + 1][Cross_i];
                    int cross_temp = AI_X[Crossover_i][Cross_i];
                    AI_X[Crossover_i][Cross_i] = AI_X[Crossover_i + 1][Cross_i];
                    AI_X[Crossover_i + 1][Cross_i] = cross_temp;
                }
                for (int Cross_it = 0; Cross_it < Cross_len; Cross_it++)
                {
                    int Cross_i = 0;
                    while(Cross_i < AI_n)
                    {
                        if (Cross_i == rd1) Cross_i = rd2;
                        int Cross_j = 0;
                        while(Cross_j < Cross_len)
                        {
                            if (AI_X[Crossover_i][Cross_i] == Cross_MappingList1[Cross_j]) AI_X[Crossover_i][Cross_i] = Cross_MappingList0[Cross_j];
                            Cross_j += 1;
                        }
                        Cross_i += 1;
                    }
                }
                for (int Cross_it = 0; Cross_it < Cross_len; Cross_it++)
                {
                    int Cross_i = 0;
                    while (Cross_i < AI_n)
                    {
                        if (Cross_i == rd1) Cross_i = rd2;
                        int Cross_j = 0;
                        while (Cross_j < Cross_len)
                        {
                            if (AI_X[Crossover_i + 1][Cross_i] == Cross_MappingList0[Cross_j]) AI_X[Crossover_i + 1][Cross_i] = Cross_MappingList1[Cross_j];
                            Cross_j += 1;
                        }
                        Cross_i += 1;
                    }
                }
                //return X1, X2
            }
            Crossover_i += 2;
        }
        return AI_X;
    }
    

    int[][] AI_Mutation(int[][] AI_X)
    {
        int Mutation_i = 0;
        while (Mutation_i < AI_pop)
        {
            if (Random.Range(0, 99) < p_m) AI_Mutate(AI_X[Mutation_i]);
            Mutation_i += 1;
        }
        return AI_X;
    }

    int[] AI_Mutate(int[] AI_X)
    {
        int rd1 = Random.Range(0, AI_n - 1);
        int rd2 = Random.Range(0, AI_n - 1);
        int temp = 0;
        if (rd1 == rd2) rd2 = (rd1 + 1) % AI_n;
        temp = AI_X[rd1];
        AI_X[rd1] = AI_X[rd2];
        AI_X[rd2] = temp;
        return AI_X;
    }
    //Jest to funkcja, która dokona ca³kowitego obliczenia ruchu poprzez algorytm genetyczny. Tutaj bêdzie inicjowane i przetwarzane wartoœci
    int AI_Run()
    {
        int[][] AI_P = new int[AI_pop][] ;
        int[][] AI_Pn = new int[AI_pop][];
        for (int AI_i = 0; AI_i < AI_pop; AI_i++)
        {
            AI_P[AI_i] = new int[AI_n];
            for (int AI_j = 0; AI_j < AI_n; AI_j++)
            {
                AI_P[AI_i][AI_j] = Random.Range(0, 7);
                if (AI_P[AI_i][AI_j] == 7) AI_P[AI_i][AI_j] = 8;
            }
        }
        int AI_gen = 0;
        int best = AI_Evaluate(AI_P);
        while((AI_gen < AI_gen_max) && (AI_EvaluateSingle(AI_P[best], 0, 0, playerUnit.curHP, playerUnit.curMana, AI_prepareSTAT(playerUnit), playerUnitSpare1.curHP, playerUnitSpare1.curMana, AI_prepareSTAT(playerUnitSpare1), playerUnitSpare2.curHP, playerUnitSpare2.curMana, AI_prepareSTAT(playerUnitSpare2), enemyUnit.curHP, enemyUnit.curMana, AI_prepareSTAT(enemyUnit), enemyUnitSpare1.curHP, enemyUnitSpare1.curMana, AI_prepareSTAT(enemyUnitSpare1), enemyUnitSpare2.curHP, enemyUnitSpare2.curMana, AI_prepareSTAT(enemyUnitSpare2), AI_n) < 65535))
        {
            AI_Pn = AI_Selection(AI_P);
            AI_Pn = AI_Crossover(AI_Pn);
            AI_Pn = AI_Mutation(AI_Pn);
            best = AI_Evaluate(AI_Pn);
            AI_P = AI_Pn;
            AI_gen += 1;
        }
        Debug.Log("AI_gen: " + AI_gen);
        return AI_P[best][0];
    }
}