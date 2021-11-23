using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI ;

public enum BattleState { START, UPKEEP, PLAYERMOVE, ENEMYMOVE, PRECOMBAT, PLAYERACTION, ENEMYACTION, CLEANUP, PLAYERSELECT, ENEMYSELECT, WIN, LOSE }
public enum BattleDistance { CLOSE, FAR }

public class BattleSystem : MonoBehaviour
{
	//Inicjalizacja zmiennych gry

	public BattleState state;
	public BattleDistance distance;

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
	
	public GameObject moveActionContainer;
	public GameObject attackActionContainer;

	public int playerActionNumber = 7;
	public int enemyActionNumber = 7;

	UnitScript playerUnit ;
	UnitScript playerUnitSpare1 ;
	UnitScript playerUnitSpare2 ;
	UnitScript enemyUnit ;
	UnitScript enemyUnitSpare1 ;
	UnitScript enemyUnitSpare2 ;

	Vector3 velocity = new Vector3(1.0f, 1.0f, 1.0f);
	Vector3 closeDistancePlayer = new Vector3(-1.65f, -4.7f, 0.0f);
	Vector3 closeDistanceEnemy = new Vector3(1.65f, -4.7f, 0.0f);
	Vector3 farDistancePlayer = new Vector3(-3.3f, -4.7f, 0.0f);
	Vector3 farDistanceEnemy = new Vector3(3.3f, -4.7f, 0.0f);

	float vulnerabilityMult = 1.4f;
	float strengthMult = 1.4f;
	float impairMult = 0.7f;
	float resistanceMult = 0.7f;
	int hasteAmt = 2;
	int slowAmt = -2;
	int bolsterAmt = -4;
	int frailtyAmt = 4;

	int moveCost = 50;

	int interceptDamage = 50;
	int restHeal = 2;
	int restMana = 5;

	void Start()
	{
		moveActionContainer.SetActive(false);
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

		state = BattleState.PLAYERMOVE;
		moveActionContainer.SetActive(true);
	}

	void Precombat()
	{
		//Ustawienie dystansów
		if (distance == BattleDistance.CLOSE)
        {
			playerBattleStation.position = Vector3.SmoothDamp(playerBattleStation.position, closeDistancePlayer, ref velocity, 0.5f);
			enemyBattleStation.position = Vector3.SmoothDamp(enemyBattleStation.position, closeDistanceEnemy, ref velocity, 0.5f);
		}
		else if(distance == BattleDistance.FAR)
		{
			playerBattleStation.position = Vector3.SmoothDamp(playerBattleStation.position, farDistancePlayer, ref velocity, 0.5f);
			enemyBattleStation.position = Vector3.SmoothDamp(enemyBattleStation.position, farDistanceEnemy, ref velocity, 0.5f);
		}
		//Aktualizacja pasków
		playerHUD.SetMP(playerUnit.curMana);
		enemyHUD.SetMP(enemyUnit.curMana);

		attackActionContainer.SetActive(true);
	}

	///////////////////////////////
	// Ruch gracza i przeciwnika //
	///////////////////////////////

	public void PlayerMoveClose()
	{
		if (playerUnit.curMana >= moveCost)
        {
			playerUnit.curMana -= moveCost;
			moveActionContainer.SetActive(false);
			distance = BattleDistance.CLOSE;
			state = BattleState.ENEMYMOVE;
			EnemyMakeMove();
        }
	}
	public void PlayerMoveFar()
	{
		if (playerUnit.curMana >= moveCost)
		{
			playerUnit.curMana -= moveCost;
			moveActionContainer.SetActive(false);
			distance = BattleDistance.FAR;
			state = BattleState.ENEMYMOVE;
			EnemyMakeMove();
		}
	}
	public void PlayerMoveStay()
	{
		moveActionContainer.SetActive(false);
		state = BattleState.ENEMYMOVE;
		EnemyMakeMove();
	}

	void EnemyMoveClose()
	{
		if (enemyUnit.curMana >= moveCost)
		{
			enemyUnit.curMana -= moveCost;
			distance = BattleDistance.CLOSE;
			state = BattleState.PRECOMBAT;
			Precombat();
		}
	}
	void EnemyMoveFar()
	{
		if (enemyUnit.curMana >= moveCost)
		{
			enemyUnit.curMana -= moveCost;
			distance = BattleDistance.FAR;
			state = BattleState.PRECOMBAT;
			Precombat();
		}
	}
	void EnemyMoveStay()
	{
		state = BattleState.PRECOMBAT;
	}

	////////////////////////////////
	// Akcje gracza i przeciwnika //
	////////////////////////////////
	
	public void SetPlayerActionNumber(int numer)
    {
		playerActionNumber = numer;
		if (playerActionNumber == 4)
        {
			playerUnit.switches = true;
        }
		else if(playerActionNumber == 5)
        {
			playerUnit.intercepts = true;
        }
		else if(playerActionNumber == 6)
        {
			playerUnit.rests = true;
        }
	}

	public void SetEnemyActionNumber(int numer)
	{
		enemyActionNumber = numer;
		if (enemyActionNumber == 4)
		{
			enemyUnit.switches = true;
		}
		else if (enemyActionNumber == 5)
		{
			enemyUnit.intercepts = true;
		}
		else if (enemyActionNumber == 6)
		{
			enemyUnit.rests = true;
		}
	}
	
	public void ActionExecute()
    {
		//Gdy postacie u¿yj¹ zwyk³ych akcji, czyli tych unikalnych dla ka¿dej postaci
		if (playerActionNumber <= 3 && enemyActionNumber <= 3)
        {
			if (playerUnit.ability[playerActionNumber].speed+(hasteAmt*Convert.ToInt32(playerUnit.haste)) > enemyUnit.ability[enemyActionNumber].speed + (hasteAmt * Convert.ToInt32(enemyUnit.haste)))
			{
				//Atak gracza jest szybszy od ataku wroga

				//Najpierw gracz wywo³uje wszystko zwi¹zane z atakiem
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
					//nastêpnie wróg ma prawo swoje rzeczy zrobiæ, chyba ¿e nast¹pi³o przerwanie
					if (enemyUnit.interrupted == false)
					{
						//playerUnit.TakeDamage(enemyUnit.ability[enemyActionNumber].damage, enemyUnit.ability[enemyActionNumber].times, enemyUnit.ability[enemyActionNumber].type);
						enemyUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].boons);
						if (playerUnit.CheckDead())
						{
							//Zmuœ gracza, ¿eby umieœci³ now¹ postaæ. Jeœli nie mo¿e, przeciwnik wygrywa
						}
						else
						{
							playerUnit.InflictFeatures(enemyUnit.ability[enemyActionNumber].banes);
						}
					}
					else enemyUnit.interrupted = false;
				}
			}
		}
    }

	////////////////////////////
	// Funkcje powi¹zane z SI //
	////////////////////////////

	void EnemyMakeMove()
	{
		EnemyMoveStay();
	}
}