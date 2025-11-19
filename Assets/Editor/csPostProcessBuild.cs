using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class csPostProcessBuild
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict root = plist.root;

        // 위치 권한
        root.SetString("NSLocationWhenInUseUsageDescription",
            "지도 기능을 위해 위치 정보를 사용합니다.");

        // 카메라 권한
        root.SetString("NSCameraUsageDescription",
            "카메라 기능이 필요합니다.");

        // 마이크(녹음) 권한
        root.SetString("NSMicrophoneUsageDescription",
            "음성 입력 기능을 위해 마이크가 필요합니다.");

        // 저장된 plist 다시 쓰기
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
