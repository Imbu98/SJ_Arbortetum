using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


public class csObserveScreen : MonoBehaviour
{
    Action _onImageLoadSuccess;


    [SerializeField] private RawImage targetDisplay;

    private void OnEnable()
    {
        PickImage(
            () =>
            {
                csImageManager.Instance.SetObserveScreen(csImageManager.Instance.observeLoadingObject);
                csImageManager.Instance.AnalyzeTexture();
            }
            );
    }

    private void OnDisable()
    {
        _onImageLoadSuccess = null;
    }
    public void PickImage(Action onImageLoadSuccess)
    {
        _onImageLoadSuccess = onImageLoadSuccess;

        TakePhotoWithCameraButton();
    }
    public void TakePhotoWithCameraButton()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        PickImageFromPC();
#elif UNITY_ANDROID && !UNITY_EDITOR
        // 예: 사진 찍기 호출
        TakePhotoWithCamera();
#elif UNITY_IOS && !UNITY_EDITOR
        // 예: 사진 찍기 호출
        TakePhotoWithCamera();
#endif
    }
    private void PickImageFromPC()
    {
        string path = OpenFilePanel("Select an Image", "", "Image Files|*.png;*.jpg;*.jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            LoadImage(path);
        }
    }
    private string OpenFilePanel(string title, string initialDirectory, string filter)
    {
        string path = null;

        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog
            {
                Title = title,
                InitialDirectory = initialDirectory,
                Filter = filter,
                Multiselect = false
            };

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = ofd.FileName;
            }
        });

        t.SetApartmentState(System.Threading.ApartmentState.STA);
        t.Start();
        t.Join();

        return path;
    }
    private void TakePhotoWithCamera()
    {
        if (!NativeCamera.IsCameraBusy())
        {
            NativeCamera.TakePicture((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    LoadImage(path);
                }
                else
                {
                    csImageManager.Instance.CloseObserveScreen();
                }
            }, 1024); // 사진의 최대 크기 (픽셀)
        }
    }

    private void LoadImage(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        // 이미지 표시
        targetDisplay.texture = texture;
        // 이미지 매니저에 현재 texture저장 
        csImageManager.Instance.capturedTexture = texture;

        //displayImage.SetNativeSize();

        string imageName = Path.GetFileName(path);

        Debug.Log("Image loaded from: " + path);
        _onImageLoadSuccess?.Invoke();
    }
}
