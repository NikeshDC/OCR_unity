using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class OCRManager : MonoBehaviour
{
    private Texture2D imageToOCR;
    public RawImage ocrImageContainer;
    AspectRatioFitter ocrImageAspectRatioFitter;

    public bool saveOCRImages;  //should the processed images be saved

    ImageInt[] ocrImages;   //images for each of the stage
    string[] ocrImagesFileNames = { "original", "binarized","noisered", "segmented", "final"}; //filename to saves above ocr images
    string imageSaveDirectory;  //directory path where to save the image files
    List<RectangularBound<int>> segments;  //segmentation step also produces list of segments for image
    string ocrText;  //the string output from ocr process
    public TextMeshProUGUI ocrTextMesh;
    public GameObject ocrTextViewer;

    public enum ProcessingStage { NONE, BINARIZE, NOISE, SEGMENT, POSTNOISE, OCR, COMPLETE};  //what process is ongoing
    //if state is NONE no processing is ongoing, if state is BINARIZE then binarization is ongoing, 
    //if state is NOISE then binarization has finished and noise reduction is going on and so on
    //COMPLETE means all processing has recently completed
    ProcessingStage currentState;  //current state is set by processing thread as it moves to diffferent stages
    ProcessingStage previousCheckState;  //state in previous check/update
    bool oneStepAvailable;  //onestep functionality can be activated or not
    bool isProcessing; //track whether any processing is ongoing as inerface for external programs

    public delegate void EmptyCallback();
    public EmptyCallback OnOCRComplete;

    public TextMeshProUGUI statusBar;
    public GameObject statusBarObject;
    

    private void Start()
    {
        //add aspect ratio fitter to imagecontainer
        if (ocrImageContainer.GetComponent<AspectRatioFitter>() == null)
            ocrImageContainer.gameObject.AddComponent<AspectRatioFitter>();
        ocrImageAspectRatioFitter = ocrImageContainer.GetComponent<AspectRatioFitter>();
        ocrImageAspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; //fit entire image within the screen

        ocrImages = new ImageInt[5];  //a default representing the initial image and 4 different processing stages
        SetImageToOCR(imageToOCR);
        currentState = ProcessingStage.NONE;
        previousCheckState = ProcessingStage.NONE;
        oneStepAvailable = true;
        imageSaveDirectory = Application.persistentDataPath + Path.DirectorySeparatorChar;
        statusBarObject.SetActive(false);

        ocrTextViewer.SetActive(false);
    }

    public void SetImageToOCR(Texture2D texture)
    {
        if (texture == null)
            return;
        imageToOCR = texture;
        float aspectRatio = (float)texture.width / (float)texture.height;
        ocrImageAspectRatioFitter.aspectRatio = aspectRatio;
        SetDefaultImageInt();
    }
    private void SetDefaultImageInt()
    {
        int textureWidth = imageToOCR.width;
        int textureHeight = imageToOCR.height;
        Color[] texturePixels = imageToOCR.GetPixels();
        ocrImages[0] = ImageUtility.GetImage(texturePixels, textureWidth, textureHeight); //by default first image is raw image
        //SaveTexture(imageToOCR, ocrImagesFileNames[0]);
    }

    private void ShowProcessedImage(ImageInt imageToShow)
    {
        Texture2D imgtexture = ImageUtility.GetTexture(imageToShow);
        ocrImageContainer.texture = imgtexture;
        imgtexture.Apply();
        float aspectRatio = (float)imgtexture.width / (float)imgtexture.height;
        ocrImageAspectRatioFitter.aspectRatio = aspectRatio;
    }
    private void ShowProcessedImageAndSave(ImageInt imageToShow, string filename)
    {
        Texture2D imgtexture = ImageUtility.GetTexture(imageToShow);
        ocrImageContainer.texture = imgtexture;
        imgtexture.Apply();
        float aspectRatio = (float)imgtexture.width / (float)imgtexture.height;
        ocrImageAspectRatioFitter.aspectRatio = aspectRatio;
        SaveTexture(imgtexture, filename);
    }
    private void SaveAllImages()
    {
        if(imageToOCR.isReadable)
            SaveTexture(imageToOCR, ocrImagesFileNames[0]);
        for(int i=1; i < ocrImagesFileNames.Length; i++)
            SaveTexture(ImageUtility.GetTexture(ocrImages[i]), ocrImagesFileNames[i]);
    }

    private void Update()
    {
        CheckState();
    }

    private void CheckState()
    {//the processing thread below cannot invoke unity APIs so polling is done to check what is the status of processing thread and modify images accordingly
        //Debug.Log("state: " + currentState);

        if (currentState == previousCheckState)
            return;  //no update is required

        statusBarObject.SetActive(true);

        switch (currentState)
        {
            case ProcessingStage.NONE:
                statusBarObject.SetActive(false);
                ocrImageContainer.texture = imageToOCR;
                break;
            case ProcessingStage.BINARIZE:
                statusBar.text = "Binarization ->";
                ocrImageContainer.texture = imageToOCR;
                break;
            case ProcessingStage.NOISE: //binarization has finished
                statusBar.text = "Noise reductuction ->";
                ShowProcessedImage(ocrImages[1]);
                break;
            case ProcessingStage.SEGMENT: //noise reduction just finished
                statusBar.text = "Segmentation ->";
                ShowProcessedImage(ocrImages[2]);
                break;
            case ProcessingStage.POSTNOISE: //segmentation just finished
                statusBar.text = "Post-noise reduction ->";
                ShowProcessedImage(ocrImages[3]);
                break;
            case ProcessingStage.OCR:
                statusBar.text = "OCR ->";
                ShowProcessedImage(ocrImages[4]);
                break;
            case ProcessingStage.COMPLETE:
                Debug.Log("OCR complete");
                isProcessing = false;
                ResetState();
                statusBar.text = "OCR Completed";
                if(saveOCRImages)
                    SaveAllImages();
                ShowOCRText();
                if(OnOCRComplete != null)
                    OnOCRComplete();
                break;
        }
        previousCheckState = currentState;
    }

    public void PerformFullOCR()
    {
        if (currentState != ProcessingStage.NONE)
            return; //if process is already ongoing then dont call a new thread to do the job
        Task.Run(() => { ProcessImageAtOnce(); });//perform whole ocr process
    }

    public bool IsProcessing()
    {
        return isProcessing;
    }

    private void ProcessImageAtOnce()
    {
        if (imageToOCR == null)
            return;
        oneStepAvailable = false;
        isProcessing = true;
        ProcessImage_binarize();
        ProcessImage_noisereduce();
        ProcessImage_segment();
        ProcessImage_postnoisereduce();
        ProcessImage_ocr();
        currentState = ProcessingStage.COMPLETE;
    }

    private void ProcessImage_binarize()
    {
        currentState = ProcessingStage.BINARIZE;
        BinarizeHelper binarizer = new BinarizeHelper();
        ocrImages[1] = binarizer.GetBinarizedImage(ocrImages[0]);
    }
    private void ProcessImage_noisereduce()
    {
        currentState = ProcessingStage.NOISE;
        NoiseReducerHelper noiseReducer = new NoiseReducerHelper();
        ocrImages[2] = noiseReducer.GetDeNoisedImage(ocrImages[1]);
    }
    private void ProcessImage_segment()
    {
        currentState = ProcessingStage.SEGMENT;
        TopDownSegmenterHelper topDownSegmenterHelper = new TopDownSegmenterHelper();
        segments = topDownSegmenterHelper.GetSegments(ocrImages[2]);
        ocrImages[3] = ImageUtility.GetSegmentsAddedImage(ocrImages[2], segments, Color.blue);
    }
    private void ProcessImage_postnoisereduce()
    {
        currentState = ProcessingStage.POSTNOISE;
        PostNoiseReducerHelper postNoiseReducer = new PostNoiseReducerHelper();
        //segmented image has box drawn in image surrounding segments so use image before segmentation
        ocrImages[4] = postNoiseReducer.GetDeNoisedImage(ocrImages[2], segments); 
    }
    private void ProcessImage_ocr() 
    {
        currentState = ProcessingStage.OCR;
        Debug.Log("performing ocr");
        ocrText = "Hello world";
        System.Threading.Thread.Sleep(1000);
    }


    public void OneStep()
    {//performs one stage of processs per call
        if (!oneStepAvailable || imageToOCR == null)
            return;

        oneStepAvailable = false;
        isProcessing = true;
        if(currentState == ProcessingStage.NONE)
            currentState = ProcessingStage.BINARIZE;//currentState is used as if to mean which stage to perform this call
        Task.Run(() => { ProcessImageOneStep(); });
    }
    private void ProcessImageOneStep()
    {
        //currentState is used as if to mean which stage to perform this call
        if (currentState == ProcessingStage.BINARIZE)
        {
            ProcessImage_binarize();
            //set current state to NOISE so that update check will know that binarization has finished and update the image
            currentState = ProcessingStage.NOISE; 
        }
        else if(currentState == ProcessingStage.NOISE)
        {
            ProcessImage_noisereduce();
            currentState = ProcessingStage.SEGMENT;
        }
        else if(currentState == ProcessingStage.SEGMENT)
        {
            ProcessImage_segment();
            currentState = ProcessingStage.POSTNOISE;
        }
        else if(currentState == ProcessingStage.POSTNOISE)
        {
            ProcessImage_postnoisereduce();
            currentState = ProcessingStage.OCR;
        }
        else if(currentState == ProcessingStage.OCR)
        {
            ProcessImage_ocr();
            currentState = ProcessingStage.COMPLETE;
        }

        if (currentState == ProcessingStage.COMPLETE)
        {//completion state is handled by CheckState so mark as still processing
            oneStepAvailable = false;
            isProcessing = true;
        }
        else
        {
            oneStepAvailable = true;
            isProcessing = false;
        }
    }
    public void ResetState()
    {
        if (isProcessing) //if some processing is going on 
        {
            Debug.Log("Processing in progress");
            return;
        }
        ocrImageContainer.texture = imageToOCR;
        float aspectRatio = (float)imageToOCR.width / (float)imageToOCR.height;
        ocrImageAspectRatioFitter.aspectRatio = aspectRatio;

        ocrTextViewer.SetActive(false);
        statusBarObject.SetActive(false);

        currentState = ProcessingStage.NONE;
        oneStepAvailable = true;
    }

    private void ShowOCRText()
    {
        ocrTextMesh.text = ocrText;
        ocrTextViewer.SetActive(true);
    }

    public ProcessingStage GetState() { return currentState; }

    private void SaveTexture(Texture2D texture, string filename)
    {
        byte[] imageRaw = texture.EncodeToPNG();
        string filepath = imageSaveDirectory + filename + ".png";
        File.WriteAllBytes(filepath, imageRaw);
    }
}
