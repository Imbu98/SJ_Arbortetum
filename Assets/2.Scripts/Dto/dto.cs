using System;
using System.Collections.Generic;

namespace Data
{
    // 공통 속성 정의
    public abstract class BaseMission
    {
        public string Description;
        public bool IsCleared;
        public int missionDistance;
    }

    // AI가 생성한 전체 미션 목록을 받아오는 dto
    public class AICreatedMissions
    {
        public List<Mission> missions;
    }

    // 하나의 미션이 가지고 있는 여러 개의 세부 미션
    public class Mission: BaseMission
    {
        public string missionTitle;

        public int missonTimeTaken; // 미션 소요시간

        public List<MissionStep> missionStepDetails;
    }

    // 세부 미션이 가지고 있는 정보
    public class MissionStep : BaseMission
    {
        public GeoCoordinate destinationCoordinate; // 현재 세부 미션의 도착지 좌표 정보

        public string destinationName; // 도착지 이름

        public string plantName; // 관찰하기 미션용 꽃 이름

    }


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

    public class PlantData
    {
        public string name;

        public string description;
    }


    public class LocationData
    {
        public string koreanName;
        public string englishName;
        public GeoCoordinate geoCoordinate;
        public int locationID;

        public LocationData(string ko=null, string en=null, double lat=0, double lon=0,int iD=0)
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

    // 지도 이용 시 길찾기 중인지, 장소 검색만 하는 중인지 판별
    public enum SearchStatus
    {
        None=0, // 아무 상태도 아님
        SearchPath=1, // 내 위치로부터 도착지를 찾을 때 활성화
    }

    // 미션의 생성 상태 확인 
    public enum MissionStatus
    {
        None=0,
        MissionCreating=1,
        MissonCreated=2,
    }
}