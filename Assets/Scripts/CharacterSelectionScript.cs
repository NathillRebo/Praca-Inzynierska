using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement ;

public class CharacterSelectionScript : MonoBehaviour
{
    private GameObject[] characterList ;
    private GameObject[] characterListLeft;
    private GameObject[] characterListRight;
    private int[] index = new int[3] ;

    private void Start(){
        index[0] = 0;
        index[1] = 0;
        index[2] = 0;
		characterList = new GameObject[transform.childCount];
        characterListLeft = new GameObject[transform.childCount];
        characterListRight = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++){
			characterList[i] = transform.GetChild(i).gameObject;
            characterListLeft[i] = transform.GetChild(i).gameObject;
            characterListRight[i] = transform.GetChild(i).gameObject;
        }
		foreach(GameObject go in characterList){
			go.SetActive(false) ;
		}
        foreach (GameObject go in characterListLeft)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in characterListRight)
        {
            go.SetActive(false);
        }
        //if (characterList[0]){
			characterList[0].SetActive(true) ;
		//}
        //if (characterListLeft[0])
        //{
            characterListLeft[0].SetActive(true);
        //}
        //if (characterListRight[0])
        //{
            characterListRight[0].SetActive(true);
        //}
    }
    
    public void ToggleLeft(){
		characterList[index[1]].SetActive(false) ;
		
		index[1]-- ;
		if (index[1] < 0){
			index[1] = characterList.Length - 1 ;
		}
		
		characterList[index[1]].SetActive(true) ;
    }
    
    public void ToggleRight(){
		characterList[index[1]].SetActive(false) ;
		
		index[1] = (index[1] + 1) % characterList.Length ;
		
		characterList[index[1]].SetActive(true) ;
    }

    public void ToggleLeftL()
    {
        characterListLeft[index[0]].SetActive(false);

        index[0]--;
        if (index[0] < 0)
        {
            index[0] = characterListLeft.Length - 1;
        }

        characterListLeft[index[0]].SetActive(true);
    }

    public void ToggleRightL()
    {
        characterListLeft[index[0]].SetActive(false);

        index[0] = (index[0] + 1) % characterListLeft.Length;

        characterListLeft[index[0]].SetActive(true);
    }

    public void ToggleLeftR()
    {
        characterListRight[index[2]].SetActive(false);

        index[2]--;
        if (index[2] < 0)
        {
            index[2] = characterListRight.Length - 1;
        }

        characterListRight[index[2]].SetActive(true);
    }

    public void ToggleRightR()
    {
        characterListRight[index[2]].SetActive(false);

        index[2] = (index[2] + 1) % characterListRight.Length;

        characterListRight[index[2]].SetActive(true);
    }

    public void ConfirmButton(){
		PlayerPrefs.SetInt("CharacterSelectedFirst", index[1]) ;
		PlayerPrefs.SetInt("CharacterSelectedSecond", index[0]) ;
		PlayerPrefs.SetInt("CharacterSelectedThird", index[2]) ;
		SceneManager.LoadScene("BattleScene") ;
    }
}
