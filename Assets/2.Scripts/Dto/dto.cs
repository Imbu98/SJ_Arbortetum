using System;
using System.Collections.Generic;

namespace Data
{
    // AI가 생성한 전체 미션 목록을 받아오는 dto
    public class AICreatedMissions
    {
        public List<MissionContainer> MissionContainers;
    }

    // 하나의 미션이 가지고 있는 여러 개의 세부 미션
    public class MissionContainer
    {
        public List<MissionDto> MissionDtos;
    }

    // 세부 미션이 가지고 있는 정보
    public class MissionDto
    {
        public GeoCoordinate DestinationCoordinate; // 현재 세부 미션의 도착지 좌표 정보

        public string DestinationName; // 도착지 이름

        public string MissonDescription; // 미션 설명
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


    public class LocationData
    {
        public string KoreanName;
        public string EnglishName;
        public double Latitude;
        public double Longitude;

        public LocationData(string ko, string en, double lat, double lon)
        {
            KoreanName = ko;
            EnglishName = en;
            Latitude = lat;
            Longitude = lon;
        }
        public string GetLocalizedName()
        {
            string lang = csSingleton.Instance.languageCode;
            return lang == "ko" ? KoreanName : EnglishName;
        }
    }
}