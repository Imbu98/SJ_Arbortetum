using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizeDropdownSetter : MonoBehaviour
{
    private TMP_Dropdown _dropdown;
    private List<LocalizedString> _localizedStrings = new();

    [SerializeField]
    private List<LocalizedString> defaultStrings;

    #region Mono Methods

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        if (defaultStrings != null && defaultStrings.Count > 0)
        {
            Init(defaultStrings);
        }

        RegisterCallbacks();
        RefreshAll();
    }


    private void OnDisable()
    {
        UnregisterCallbacks();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    #endregion

    /// <summary>
    /// 드롭다운의 옵션을 새로운 LocalizedString 리스트로 설정
    /// </summary>
    public void Init(List<LocalizedString> newStrings)
    {
        UnregisterCallbacks(); // 기존 콜백 해제
        _localizedStrings = newStrings;
        RegisterCallbacks();  // 새 콜백 등록

        RefreshAll();
    }

    private void RegisterCallbacks()
    {
        foreach (var localizedString in _localizedStrings)
        {
            localizedString.StringChanged += OnLocalizedStringChanged;
        }
    }

    private void UnregisterCallbacks()
    {
        foreach (var localizedString in _localizedStrings)
        {
            localizedString.StringChanged -= OnLocalizedStringChanged;
        }
    }

    private void OnLocalizedStringChanged(string _)
    {
        // 단일 항목 변경만 감지하기 어렵기 때문에 전체 갱신
        RefreshAll();
    }

    private void OnLocaleChanged(Locale _)
    {
        RefreshAll();
    }

    private async void RefreshAll()
    {
        List<TMP_Dropdown.OptionData> options = new();

        foreach (var localizedString in _localizedStrings)
        {
            var localized = await localizedString.GetLocalizedStringAsync();
            options.Add(new TMP_Dropdown.OptionData(localized));
        }

        _dropdown.ClearOptions();
        _dropdown.AddOptions(options);
    }
}
