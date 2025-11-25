using System;
using System.Collections.Generic;

namespace Data
{

    #region mission

    // 미션 공통 속성 정의
    [System.Serializable]
    public abstract class BaseMission
    {
        public string Description; // 미션 설명

        public bool IsCleared;
    }

    // AI가 생성한 전체 미션 목록을 받아오는 dto
    [System.Serializable]
    public class AICreatedMissions
    {
        public List<Mission> missions = new List<Mission>();

        public int missionIndex=-1; // 몇번째 미션인지

        public int missionStepIndex=-1; // 미션의 몇번 째를 진행중인지
    }

    // 하나의 미션이 가지고 있는 여러 개의 세부 미션
    [System.Serializable]
    public class Mission: BaseMission
    {
        public string missionTitle;

        public int missonRewardPoint; // 미션 

        public List<MissionStep> missionStepDetails = new List<MissionStep>();
    }

    // 세부 미션이 가지고 있는 정보
    [System.Serializable]
    public class MissionStep : BaseMission
    {
        public GeoCoordinate destinationCoordinate; // 현재 세부 미션의 도착지 좌표 정보

        public string plantName; // 관찰하기 미션용 꽃 이름
    }

    #endregion


    #region coordinate

    // 목적지에 대한 길찾기 경로 좌표
    public class SearchPathCoordinate
    {
        public List<GeoCoordinate> pathCoordinates; // 길찾기 경로 좌표 리스트
    }

    [System.Serializable]
    public struct GeoCoordinate
    {
        public double Latitude; //위도
        public double Longitude; //경도

        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    [System.Serializable]
    public class LocationData
    {
        public string koreanName;
        public string englishName;
        public GeoCoordinate geoCoordinate;
        public int locationID;

        public LocationData(string ko = null, string en = null, double lat = 0, double lon = 0, int iD = 0)
        {
            koreanName = ko;
            englishName = en;
            geoCoordinate.Latitude = lat;
            geoCoordinate.Longitude = lon;
            locationID = iD;

        }
        public string GetLocalizedName()
        {
            string lang = csSingleton.Instance.languageCode;
            return lang == "ko" ? koreanName : englishName;
        }
    }

    #endregion

    #region observePlant



    [System.Serializable]
    public class PostPlantRequest
    {
        public string uid;
        public string image;   
        public string organs; 
    }

    [System.Serializable]
    public class GetPlantResponse
    {
        public string plantScientificName;
        public List<string> commonNames;
        public string description;
        public float score;
        public string gbif;
        public List<PlantImageInfo> plantimages;
        public List<QuizData> quizData;
    }

    [System.Serializable]
    public class PlantImageInfo
    {
      public string url; // image URL
      public string reference; // image 출처
      public string license;// image 라이센스
    }

    [System.Serializable]
    public class SavedPlantWrapper
    {
        public List<string> plantList;
    }

    #endregion


    #region aiChat

    [System.Serializable]
    public class ChatHistoryWrapper
    {
        public List<ChatMessage> chatList;
    }

    
    [System.Serializable]
    public class ChatMessage
    {
        public string user_id;   // 구글 또는 애플 로그인 시 나오는 UID 사용
        public string message;
    }

    [System.Serializable]
    public class AIChatResponse
    {
        public string response;
        public bool route_finalized = false; // 기본값 false
        public List<SimpleRoute> route;      // null 허용
    }

    [System.Serializable]
    public class SimpleRoute
    {
        public string name;
        public double latitude;
        public double longitude;
    }

    #endregion

    #region Quiz

    public class QuizData
    {
        public QuizType quizType;

        public string quizDescription; // 퀴즈 설명

        public List<string> quizChoices; // 퀴즈 객관식 보기

        public int answer; // 퀴즈 정답
    }

    public class QuizDataWrapper
    {
        

       public QuizData quizData;

       public bool IsSolvedQuestion; // 사용자가 처음 맞춘 문제인지에 대한 bool값

       public string plantScientificName; // 퀴즈와 연관된 식물 학명
    }

    public class QuizDataWrapperList
    {
        public List<QuizDataWrapper> quizDataWrapperList = new List<QuizDataWrapper>();
    }

    #endregion



    #region Enum

    [System.Serializable]
    public enum QuizType
    {
        None = 0,
        MultipleChoice = 1, // 객관식
        FindRight = 2, //  O/X 퀴즈
    }

    public enum PolicyType
    {
        Service,
        Privacy,
        Marketing
    }

    // 지도 이용 시 길찾기 중인지, 장소 검색만 하는 중인지 판별
    public enum SearchStatus
    {
        None = 0, // 아무 상태도 아님
        SearchPath = 1, // 내 위치로부터 도착지를 찾을 때 활성화
    }

    // 미션의 생성 상태 확인 
    public enum MissionStatus
    {
        None = 0,
        MissionCreating = 1,
        MissonCreated = 2,
    }

    #endregion
}