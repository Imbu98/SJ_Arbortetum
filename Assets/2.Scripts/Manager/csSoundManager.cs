using System;
using System.Collections.Generic;
using UnityEngine;

public class csSoundManager : MonoBehaviour
{
    public static csSoundManager Instance { get { return _Instance; } }
    private static csSoundManager _Instance;

    [SerializeField] private AudioSource audioSources; //실제 코드에서 사용되는 오디오 소스 파일

    [SerializeField] private AudioClip[] EffectSoundAudioClip;
    private List<AudioSource> EffectAudioSources = new List<AudioSource>();
    private int effectSourcePoolSize = 10;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        audioSources = gameObject.GetComponent<AudioSource>();

        GameObject poolParent = new GameObject("EffectAudioSourcePool");
        poolParent.transform.SetParent(this.transform);

        for (int i = 0; i < effectSourcePoolSize; i++)
        {
            AudioSource newSource = poolParent.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            EffectAudioSources.Add(newSource);
        }
    }


    // 효과음이 다른 효과음에 의해 안들리지 않도록 따로 추가
    public float HashPlayEffectSound(string _s)
    {
        float retFloat = 0.0f;
        if (_s == "")
            return 0.0f;

        //AudioClip[] tmpAudioClip = null;
        //tmpAudioClip = EffectSoundAudioClip;

        AudioClip[] tmpAudioClip = EffectSoundAudioClip;

        int findIndex = Array.FindIndex(tmpAudioClip, i => i.name == _s);
        if (findIndex != -1)
        {
            HashPlayFxSound(tmpAudioClip[findIndex]);
            retFloat = tmpAudioClip[findIndex].length;
        }
        return retFloat;
    }
    public void HashPlayFxSound(AudioClip _clip)
    {
        //if (audioSources == null)
        //  audioSources = gameObject.AddComponent<AudioSource>();

        if (_clip == null) return;

        foreach (var source in EffectAudioSources)
        {
            if (!source.isPlaying)
            {
                source.clip = _clip;
                source.clip.name = _clip.name;
                source.volume = csSingleton.Instance.bSoundEffectMute? 0f:csSingleton.Instance.fSoundEffect;
                source.loop = false;
                source.Play();
                return; // Sound played, exit the method.                                          │
            }
        }


        //if (!(_clip.name.Length >= 8 && _clip.name.Substring(0, 8) == "ROGI_Num"))
        //{
        //    temp = _clip;
        //}

        //audioSources.PlayOneShot(_clip, 1.0f);
    }

    public void StopEffectSound(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        foreach (var source in EffectAudioSources)
        {
            if (source.isPlaying && source.clip != null && source.clip.name == clipName)
            {
                source.Stop();
            }
        }
    }

}
