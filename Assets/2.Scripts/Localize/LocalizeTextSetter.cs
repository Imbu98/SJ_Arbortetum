using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// TMP_Text 컴포넌트에 붙어 로컬라이즈된 문자열들을 작성해주는 컴포넌트
/// </summary>
public class LocalizeTextSetter : MonoBehaviour
{
    private TMP_Text _text;
    private LocalizedString _localizedString;

    [SerializeField] private LocalizedString defaultString;

    #region Mono Methods

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

        if (defaultString != null)
            Init(defaultString);
        else
            _localizedString = new();
    }

    private void OnEnable()
    {

        _localizedString.StringChanged += OnLocalizedStringChanged;
    }

    private void OnDisable()
    {
        _localizedString.StringChanged -= OnLocalizedStringChanged;
    }

    private void OnDestroy()
    {
        _text = null;
    }

    #endregion


    /// <summary>
    /// 로컬라이즈 테이블과 키를 설정하고 자동으로 텍스트를 갱신해주는 함수
    /// </summary>
    public void Init(string tableName, string tableEntryKey)
    {
        if (tableName == null || tableEntryKey == null)
            return;

        if (_localizedString == null)
            _localizedString = new();

        _localizedString.TableReference = tableName;
        _localizedString.TableEntryReference = tableEntryKey;

        _localizedString.RefreshString();
    }

    /// <summary>
    /// 로컬라이즈 테이블과 키를 설정하고 자동으로 텍스트를 갱신해주는 함수
    /// </summary>
    public void Init(LocalizedString newString)
    {
        _localizedString = newString;
        _localizedString.RefreshString();
    }


    private void OnLocalizedStringChanged(string value)
    {
        if(_text)
        {
            _text.text = value;
        }
    }

    /// <summary>
    /// 현재 LocalizedString에 새로운 인자값들을 적용하고 문자열을 새로 고칩니다.
    /// </summary>
    public void SetArguments(params object[] args)
    {
        if (_localizedString == null)
            return;

        if (args != null && args.Length > 0)
        {
            _localizedString.Arguments = new List<object>(args);
        }
        else
        {
            _localizedString.Arguments = null;
        }

        _localizedString.RefreshString();
    }
}