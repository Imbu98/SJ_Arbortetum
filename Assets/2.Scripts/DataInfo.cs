using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Datainfo
{

    //플레이어 데이터
    [System.Serializable]
    public class GameData
    {
        public string strPlayerNickName = "";
    }
   

    [System.Serializable]
    public class SetData
    {
        public bool bTermsofUse;//이용약관 동의
        public bool bGoogleLogin;//구글 로그인인지
        public bool bGuestLogin;//커스텀 로그인인지
        public float fBgm;
        public float fSoundEffect;
        public bool bBgmMute;// 배경음 on/off
        public bool bSoundEffectMute;//효과음 on/off

        public float fRecommendTimer;

        public bool bAutoLogin = false;//자동 로그인

        public int nSavedLoginType = 0;//로그인 타입 1: 구글, 2: 애플

        public int nLanguage = 0;//언어설정

        public string UID;

        public List<ChatMessage> strSavedChatHistory;
    }
}