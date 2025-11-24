using System.Collections.Generic;
using UnityEngine;

public class csAnimationManager : MonoBehaviour
{
    public static csAnimationManager Instance { get; private set; }

    [System.Serializable]
    public class BoolAnimData
    {
        public string key;                 // Play할 때 사용할 키
        public Animator animator;          // UI Animator
        public string boolParameterName;   // Animator Bool Parameter 이름
    }

    [Header("Bool 애니메이션 리스트")]
    public List<BoolAnimData> boolAnimList = new List<BoolAnimData>();

    private Dictionary<string, BoolAnimData> _boolDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _boolDict = new Dictionary<string, BoolAnimData>();

        foreach (var data in boolAnimList)
        {
            if (string.IsNullOrEmpty(data.key) || data.animator == null) continue;

            if (!_boolDict.ContainsKey(data.key))
            {
                _boolDict.Add(data.key, data);
            }
            else
            {
                Debug.LogWarning($"중복된 키 존재! key = {data.key}");
            }
        }
    }

    /// <summary>
    /// 키로 Bool Parameter 값을 설정
    /// </summary>
    public void SetTrue(string key)
    {
        SetBool(key, true);
    }

    public void SetFalse(string key)
    {
        SetBool(key, false);
    }

    public void SetBool(string key, bool value)
    {
        if (!_boolDict.TryGetValue(key, out var data))
        {
            Debug.LogWarning($"키를 찾을 수 없음! key = {key}");
            return;
        }

        data.animator.SetBool(data.boolParameterName, value);
    }
}
