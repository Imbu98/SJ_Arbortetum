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
            "Location information is used for map features.");

        // 카메라 권한
        root.SetString("NSCameraUsageDescription",
            "Camera functionality is required.");

        // 마이크(녹음) 권한
        root.SetString("NSMicrophoneUsageDescription",
            "A microphone is required for voice input functionality.");

        // 저장된 plist 다시 쓰기
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
