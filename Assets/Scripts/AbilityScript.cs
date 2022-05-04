using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{
    public string skillName; // Nazwa zdolno�ci
    public string skillDesc; //Opis zdolno�ci, �eby gracz wiedzia�, co dana zdolno�� robi
    public Sprite skillIcon;
    public string type; // Przyjmuje warto�ci "PHYSICAL", "LIGHT" lub "DARK" i okre�la, kt�r� odporno�ci� zmniejszane s� obra�enia otrzymane
    public int damage; //Ilo�� obra�e� zadanych przez zdolno��
    public int times; // Ile razy zdolno�� zadaje obra�enia. Zwi�ksza to obra�enia, ale te� skuteczno�� odporno�ci przeciwko temu atakowi.
    public int cost; // Koszt w punktach energii. Ka�da posta� ma 100 energii maksymalnej, regeneruje 20 energii na tur�, gdy jest "w polu" oraz 5 energii, gdy jest "na �awce"
    public int speed; // Zdolno�ci z wi�ksz� szybko�ci� s� aktywowane przed umiej�tno�ciami z ni�sz� szybko�ci�
    public string banes; //Efekty nak�adane na wroga
    public string boons; //Efekty nak�adane na rzucaj�cego
}
