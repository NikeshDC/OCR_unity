using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeviceCameraHandler : MonoBehaviour
{
    private bool cameraAvaialable;
    private WebCamTexture cameraOutput;
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private Texture defaultTexture;
    [SerializeField] private bool frontCamera;

    private AspectRatioFitter aspectRatioFitter;
    

    private void Start()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            Application.RequestUserAuthorization(UserAuthorization.WebCam);

        WebCamDevice[] deviceCameras = WebCamTexture.devices;
        foreach(WebCamDevice deviceCam in deviceCameras)
        {
            if (deviceCam.kind == WebCamKind.WideAngle && !(deviceCam.isFrontFacing ^ frontCamera)) //choose default(i.e. wideangle) camera either front or back based on frontcamera bool
            {
                cameraOutput = new WebCamTexture(deviceCam.name, Screen.width, Screen.height);
                cameraAvaialable = true;
            }
        }

        if (cameraAvaialable)
        {
            cameraDisplay.texture = cameraOutput;
            cameraOutput.autoFocusPoint = null;  //continious autofocus
            cameraOutput.Play();
            if (cameraDisplay.GetComponent<AspectRatioFitter>() == null)
                cameraDisplay.AddComponent<AspectRatioFitter>();
            aspectRatioFitter = cameraDisplay.GetComponent<AspectRatioFitter>();
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            float aspectRatio = (float)cameraOutput.width / (float)cameraOutput.height;
            aspectRatioFitter.aspectRatio = aspectRatio;
        }
        else
            cameraDisplay.texture = defaultTexture; 
    }

    private void Update()
    {
        if (!cameraAvaialable)
            return;

        //handle camera sacle and rotation
        float scaleY = cameraOutput.videoVerticallyMirrored ? -1.0f : 1f;
        cameraDisplay.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);
        int orient = cameraOutput.videoRotationAngle;
        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0f, 0f, orient);
       
    }

}
