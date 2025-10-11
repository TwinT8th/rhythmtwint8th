using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject[] goGameUI = null;



    public static GameManager instance;

    public bool isStartGame = false;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {

        for(int i = 0; i< goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(true);
        }
        isStartGame = true;
        Debug.Log("[GameManager] ���� ���۵�!");


        if (NoteManager.instance != null)
            NoteManager.instance.ResetForReplay();

        // ���⼭ NoteManager���� ���� ���� ��û
        if (NoteManager.instance != null)
            NoteManager.instance.StartMusic();
    }

    public void ExitGame()
    {

        for (int i = 0; i < goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(false);
        }
        isStartGame = false;
        Debug.Log("[GameManager] ���� ��");

    }

}
