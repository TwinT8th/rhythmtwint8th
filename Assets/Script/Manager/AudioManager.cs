using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}


public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [SerializeField] Sound[] sfx = null;
    [SerializeField] Sound[] bgm = null;

    [SerializeField] AudioSource bgmPlayer = null;
    [SerializeField] AudioSource[] sfxPlayer = null;


    void Awake()
    {
        
        instance = this;   
    }


    /*
    public void PlayBGM(string p_bgmName)
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            if(p_bgmName == bgm[i].name)
            {
                bgmPlayer.clip = bgm[i].clip;
                bgmPlayer.Play();
            }
        }
    }
    */

    // ������� ���
    public void PlayBGM(string name, double delaySec = 0d)
    {

        Sound target = null;

        for (int i = 0; i < bgm.Length; i++)
        {
            if (bgm[i].name == name)
            {
                target = bgm[i];
                break; // ã������ �ݺ��� ����
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[AudioManager] BGM '{name}'�� ã�� �� �����ϴ�.");
            return;
        }

        bgmPlayer.clip = target.clip;

        if (delaySec > 0)
        {
            double startDSP = AudioSettings.dspTime + delaySec;
            bgmPlayer.PlayScheduled(startDSP);
            Debug.Log($"[AudioManager] '{name}' scheduled to start at DSP={startDSP:F3}");
        }
        else
        {
            bgmPlayer.Play();
            //Debug.Log($"[AudioManager] '{name}' ��� ���");
        }
    }

    public void StopBGM() 
    {
        bgmPlayer.Stop();
    }


    public void PlaySFX(string p_sfxName)
    {
        for (int i = 0; i < sfx.Length; i++)
        {
            if (p_sfxName == sfx[i].name)
            {
                //AudioSource ������ŭ �ݺ�
                for(int x=0; x<sfxPlayer.Length;x++)
                {
                    //��������� ���� �÷��̾ ã����
                    if (!sfxPlayer[x].isPlaying)
                    {
                        sfxPlayer[x].clip = sfx[x].clip;
                        sfxPlayer[x].Play();
                        return;
                    }
                }
                Debug.Log("��� �÷��̾ ������Դϴ�.");
                return;
            }
        }

        Debug.Log(p_sfxName + "�̸��� ȿ������ �����ϴ�.");

    }

    // ���̵�ƿ� ���
    public IEnumerator FadeOutBGM(float fadeDuration = 2f)
    {
        if (bgmPlayer == null || !bgmPlayer.isPlaying)
            yield break;

        float startVolume = bgmPlayer.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        bgmPlayer.Stop();
        bgmPlayer.volume = startVolume; // �ʱ�ȭ
    }

    //���� ����ð� �ʿ��� ���
    public float GetMusicTime()
    {
        return bgmPlayer != null ? bgmPlayer.time : 0f;
    }

}
