using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{
    public string skillName; // Nazwa zdolnoœci
    public string skillDesc; //Opis zdolnoœci, ¿eby gracz wiedzia³, co dana zdolnoœæ robi
    public Sprite skillIcon;
    public string type; // Przyjmuje wartoœci "PHYSICAL", "LIGHT" lub "DARK" i okreœla, któr¹ odpornoœci¹ zmniejszane s¹ obra¿enia otrzymane
    public int damage; //Iloœæ obra¿eñ zadanych przez zdolnoœæ
    public int times; // Ile razy zdolnoœæ zadaje obra¿enia. Zwiêksza to obra¿enia, ale te¿ skutecznoœæ odpornoœci przeciwko temu atakowi.
    public int cost; // Koszt w punktach energii. Ka¿da postaæ ma 100 energii maksymalnej, regeneruje 20 energii na turê, gdy jest "w polu" oraz 5 energii, gdy jest "na ³awce"
    public int speed; // Zdolnoœci z wiêksz¹ szybkoœci¹ s¹ aktywowane przed umiejêtnoœciami z ni¿sz¹ szybkoœci¹
    public string banes; //Efekty nak³adane na wroga
    public string boons; //Efekty nak³adane na rzucaj¹cego
}
