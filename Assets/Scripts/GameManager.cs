using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Texture2D cameraCapture;  //saves captured image when button pressed

    public Canvas UIcanvas;   //briefly hides ui canvas to take screen capture equivalent to taking photo using camera
    public RawImage ocrImageFeed;  //container for showing processed image
    public OCRManager ocrManager;
    public RawImage cameraFeed;

    public Button[] ocrButtons;  //ocr related buttons, used for hiding or showing them

    private bool screenImageCaptured;
    private bool capturingImage;

    public GameObject InProgressIcon;

    // Start is called before the first frame update
    void Awake()
    {
        screenImageCaptured = false;
        capturingImage = false; 
        ocrManager.ocrImageContainer = ocrImageFeed;
        ocrManager.OnOCRComplete = OnOCRFinish;
        InProgressIcon.SetActive(false);

        ToggleCameraFeed(true);  //initailly show camera feed and hide ocr image
    }


    public void OnFullOCRButtonPress()
    {
        if (ocrManager.IsProcessing() && !capturingImage)
            return;
        screenImageCaptured=false;
        capturingImage=true;  //image capturing takes few frames
        StartCoroutine(CaptureScreenImage());
        StartCoroutine(PerformFullOCR());
    }
    IEnumerator CaptureScreenImage()
    {
        UIcanvas.gameObject.SetActive(false);
        //yield return null; //wait so as to let the ui elements have enough time to be hidden
        yield return new WaitForEndOfFrame();  
        cameraCapture = ScreenCapture.CaptureScreenshotAsTexture();
        ocrManager.SetImageToOCR(cameraCapture);
        UIcanvas.gameObject.SetActive(true);
        screenImageCaptured = true;
    }
    private IEnumerator PerformFullOCR()
    {
        while(!screenImageCaptured)
        {
            yield return null;
        }
        ToggleCameraFeed(false);
        ocrManager.PerformFullOCR();
        capturingImage = false;
    }

    public void OnOneStepOCRButtonPress()
    {
        if (ocrManager.IsProcessing() || capturingImage)
            return;
        screenImageCaptured = false;
        capturingImage = true;  //image capturing takes few frames
        if (ocrManager.GetState() == OCRManager.ProcessingStage.NONE) //if image has not been captured take it now
            StartCoroutine(CaptureScreenImage());
        else
            screenImageCaptured = true;
        StartCoroutine(PerformOneStepOCR());
    }
    private IEnumerator PerformOneStepOCR()
    {
        while (!screenImageCaptured)
        {
            yield return null;
        }
        ToggleCameraFeed(false);
        ocrManager.OneStep();
        capturingImage = false;
    }

    public void OnReturnButtonPressed()
    {
        ToggleCameraFeed(true);
        ocrManager.ResetState();
        ShowOCRButtons(true);
    }
    private void ShowOCRButtons(bool value)
    {
        foreach(Button button in ocrButtons)
            button.gameObject.SetActive(value);
    }

    public void OnOCRFinish()
    {
        ShowOCRButtons(false);
    }

    private void ToggleCameraFeed(bool toggleTo)
    {//when one is shown other should be hidden
        cameraFeed.gameObject.SetActive(toggleTo);
        ocrImageFeed.gameObject.SetActive(!toggleTo);
    }

    private void Update()
    {
        if (ocrManager.IsProcessing())
            InProgressIcon.SetActive(true);
        else
            InProgressIcon.SetActive(false);

    }

}
