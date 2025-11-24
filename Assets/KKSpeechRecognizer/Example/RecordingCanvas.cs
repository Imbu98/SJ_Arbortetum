using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using KKSpeech;
using TMPro;

public class RecordingCanvas : MonoBehaviour
{

 [SerializeField] private csSpeechPanel _speechPanel;

  public Button startRecordingButton;
  // 녹음이 활성화 되있을 때 표시할 오브젝트(또는 스파인으로 두고 애니메이션 재생 예정)
  public GameObject recordingActiveObject;

  public TextMeshProUGUI resultText;



    void Start()
  {
    if (SpeechRecognizer.ExistsOnDevice())
    {
      SpeechRecognizerListener listener = GameObject.FindAnyObjectByType<SpeechRecognizerListener>();
      listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);
      listener.onAvailabilityChanged.AddListener(OnAvailabilityChange);
      listener.onErrorDuringRecording.AddListener(OnError);
      listener.onErrorOnStartRecording.AddListener(OnError);
      listener.onFinalResults.AddListener(OnFinalResult);
      listener.onPartialResults.AddListener(OnPartialResult);
      listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
      SpeechRecognizer.RequestAccess();

            Debug.Log("SpeechRecognizer Start");
        }
    else
    {
      resultText.text = "Sorry, but this device doesn't support speech recognition";
      startRecordingButton.enabled = false;
    }


  }

    private void OnEnable()
    {
        ResetResultText();
    }


    public void OnFinalResult(string result)
  {
    recordingActiveObject.SetActive(false);
    resultText.text = result;
    startRecordingButton.enabled = true;

     
     // 마지막 결과를 서버에 보냄 
    _speechPanel.SendResultToSerever(result);
    // 결과 텍스트 초기화
    ResetResultText();

    }

  public void OnPartialResult(string result)
  {
    resultText.text = result;
  }

  public void OnAvailabilityChange(bool available)
  {
    startRecordingButton.enabled = available;
    if (!available)
    {
      resultText.text = "Speech Recognition not available";
    }
    else
    {
      resultText.text = "Say something :-)";
    }
  }

  public void OnAuthorizationStatusFetched(AuthorizationStatus status)
  {
    switch (status)
    {
      case AuthorizationStatus.Authorized:
        startRecordingButton.enabled = true;
        break;
      default:
        startRecordingButton.enabled = false;
        resultText.text = "Cannot use Speech Recognition, authorization status is " + status;
        break;
    }
  }

  public void OnEndOfSpeech()
  {
        recordingActiveObject.SetActive(false );
  }

  public void OnError(string error)
  {
    Debug.LogError(error);
        recordingActiveObject.SetActive(false);
        startRecordingButton.enabled = true;
        ResetResultText();
  }

  public void OnStartRecordingPressed()
  {
        Debug.Log("OnStartRecordingPressed");

        if (SpeechRecognizer.IsRecording())
    {
#if UNITY_IOS && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			recordingActiveObject.SetActive(true);
			startRecordingButton.enabled = false;
#elif UNITY_ANDROID && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			recordingActiveObject.SetActive(false);
#endif
            ResetResultText();
        }
        else
    {
      SpeechRecognizer.StartRecording(true);
            recordingActiveObject.SetActive(true);
            resultText.text = "대화를 시작하세요";
    }
  }

    public void OnStopRecording()
    {
        if (SpeechRecognizer.IsRecording())
        {
#if UNITY_IOS && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			recordingActiveObject.SetActive(true);
			startRecordingButton.enabled = false;
#elif UNITY_ANDROID && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			recordingActiveObject.SetActive(false);
#endif
        }
        ResetResultText();
        _speechPanel.CloseSpeechToTextScreen();
    }

    private void ResetResultText()
    {
        resultText.text = "버튼을 누르고 말하기를 시작하세요"; // 나중에 로컬라제이션 적용
    }
}
