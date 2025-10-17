using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{

    public int[] score;
    public string[] rank;

    void Start()
    {
        LoadScore();
        LoadRank();
    }


    public void SaveScore()
    {
        PlayerPrefs.SetInt("Score1", score[0]);
        PlayerPrefs.SetInt("Score2", score[1]);
        PlayerPrefs.SetInt("Score3", score[2]);
        //PlayerPrefs. �����͸� ��ü ��⿡ ���� 
    }

    public void LoadScore()
    {
        //Ű �ִ��� �˻�
        if(PlayerPrefs.HasKey("Score1"))
        {
            score[0] = PlayerPrefs.GetInt("Score1");
            score[1] = PlayerPrefs.GetInt("Score2");
            score[2] = PlayerPrefs.GetInt("Score3");
        }
    }

    public void SaveRank()
    {
        PlayerPrefs.SetString("Rank1", rank[0]);
        PlayerPrefs.SetString("Rank2", rank[1]);
        PlayerPrefs.SetString("Rank3", rank[2]);
    }

    public void LoadRank()
    {
        //Ű �ִ��� �˻�
        if (PlayerPrefs.HasKey("Rank1"))
        {
            rank[0] = PlayerPrefs.GetString("Rank1");
            rank[1] = PlayerPrefs.GetString("Rank2");
            rank[2] = PlayerPrefs.GetString("Rank3");
        }
    }



}
