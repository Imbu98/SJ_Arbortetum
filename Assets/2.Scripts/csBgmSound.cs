using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class csBgmSound : MonoBehaviour
{
    private new AudioSource audio;
    public AudioClip[] bgmClip = new AudioClip[6];//0:MainIntro,1:Rogi,2:Raunie,3:Hattie,4:Coco

    public AudioMixer masterMixer;

    // Start is called before the first frame update
    private void Awake()
    {


    }
    void Start()
    {
        //SoundBgm(0);

        float bgm = Mathf.Lerp(-30f, 10f, csSingleton.Instance.fBgm);
        float effect = Mathf.Lerp(-30f, 10f, csSingleton.Instance.fSoundEffect);

        if (bgm <= -30) bgm = -80;
        if (effect <= -30) effect = -80;

        masterMixer.SetFloat("Bgm", bgm);
        masterMixer.SetFloat("Effect", effect);

        if (csSingleton.Instance.bBgmMute)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }

    }


    public void SoundBgm(int num)
    {
        //csSingleton.Instance.fSound = 0;
        if (audio == null)
            audio = gameObject.GetComponent<AudioSource>();
        switch (num)
        {
            case 0:
                audio.clip = bgmClip[0];
                audio.Play();
                break;
            case 1:
                audio.clip = bgmClip[1];
                audio.Play();
                break;
            case 2:
                audio.clip = bgmClip[2];
                audio.Play();
                break;
            case 3:
                audio.clip = bgmClip[3];
                audio.Play();
                break;
            case 4:
                audio.clip = bgmClip[4];
                audio.Play();
                break;
            case 5:
                audio.clip = bgmClip[5];
                audio.Play();
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
