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
        }
        else
            cameraDisplay.texture = defaultTexture; 

    }

    private void Update()
    {
        if (!cameraAvaialable)
            return;

        //handle camera rotation and so on
        float aspectRatio = (float)cameraOutput.width / (float)cameraOutput.height;
        aspectRatioFitter.aspectRatio = aspectRatio;
    }

}
