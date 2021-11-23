using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement ;

public class CharacterSelectionScript : MonoBehaviour
{
    private GameObject[] characterList ;
    private int index ;
    private void Start(){
		characterList = new GameObject[transform.childCount] ;
		for(int i = 0; i < transform.childCount; i++){
			characterList[i] = transform.GetChild(i).gameObject ;
		}
		foreach(GameObject go in characterList){
			go.SetActive(false) ;
		}
		if (characterList[0]){
			characterList[0].SetActive(true) ;
		}
    }
    
    public void ToggleLeft(){
		characterList[index].SetActive(false) ;
		
		index-- ;
		if (index < 0){
			index = characterList.Length - 1 ;
		}
		
		characterList[index].SetActive(true) ;
    }
    
    public void ToggleRight(){
		characterList[index].SetActive(false) ;
		
		index = (index + 1) % characterList.Length ;
		
		characterList[index].SetActive(true) ;
    }
    
    public void ConfirmButton(){
		PlayerPrefs.SetInt("CharacterSelectedFirst", index) ;
		PlayerPrefs.SetInt("CharacterSelectedSecond", index) ;
		PlayerPrefs.SetInt("CharacterSelectedThird", index) ;
		SceneManager.LoadScene("BattleScene") ;
    }
}
