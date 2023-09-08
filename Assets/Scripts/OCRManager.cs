using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class OCRManager : MonoBehaviour
{
    private Texture2D imageToOCR;
    public RawImage imageToSet;

    ImageInt[] ocrImages;   //images for each of the stage
    string[] ocrImagesFileNames = { "original", "binarized","noisered", "segmented", "final"}; //filename to saves above ocr images
    string imageSaveDirectory;  //directory path where to save the image files
    List<RectangularBound<int>> segments;  //segmentation step also produces list of segments for image
    string ocrText;  //the string output from ocr process

    enum ProcessingStage { NONE, BINARIZE, NOISE, SEGMENT, POSTNOISE, OCR, COMPLETE};  //what process is ongoing
    //if state is NONE no processing is ongoing, if state is BINARIZE then binarization is ongoing, 
    //if state is NOISE then binarization has finished and noise reduction is going on and so on
    //COMPLETE means all processing has recently completed
    ProcessingStage currentState;  //current state is set by processing thread as it moves to diffferent stages
    ProcessingStage previousCheckState;  //state in previous check/update
    bool oneStepAvailable;  //onestep functionality can be activated or not
    bool isProcessing; //track whether any processing is ongoing as inerface for external programs

    float updateInterval = 0.01f;  //interval in which to check for processing state //check every 0.1s
    

    private void Start()
    {
        ocrImages = new ImageInt[5];  //a default representing the initial image and 4 different processing stages
        SetImageToOCR(imageToOCR);
        currentState = ProcessingStage.NONE;
        previousCheckState = ProcessingStage.NONE;
        oneStepAvailable = true;
        imageSaveDirectory = Application.dataPath + Path.DirectorySeparatorChar;

        StartCoroutine(OnUpdate());
    }

    public void SetImageToOCR(Texture2D texture)
    {
        if (texture == null)
            return;
        imageToOCR = texture;
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

    private void ShowProcessedImageAndSave(ImageInt imageToShow, string filename)
    {
        Texture2D imgtexture = ImageUtility.GetTexture(imageToShow);
        imageToSet.texture = imgtexture;
        imgtexture.Apply();
        SaveTexture(imgtexture, filename);
    }
    private void ShowProcessedImage(ImageInt imageToShow)
    {
        Texture2D imgtexture = ImageUtility.GetTexture(imageToShow);
        imageToSet.texture = imgtexture;
        imgtexture.Apply();
    }
    private void SaveAllImages()
    {
        if(imageToOCR.isReadable)
            SaveTexture(imageToOCR, ocrImagesFileNames[0]);
        for(int i=1; i < ocrImagesFileNames.Length; i++)
            SaveTexture(ImageUtility.GetTexture(ocrImages[i]), ocrImagesFileNames[i]);
    }

    IEnumerator OnUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(updateInterval);
            CheckState();
        }
    }

    private void CheckState()
    {//the processing thread below cannot invoke unity APIs so polling is done to check what is the status of processing thread and modify images accordingly
        //Debug.Log("state: " + currentState);

        if (currentState == previousCheckState)
            return;  //no update is required

        switch(currentState)
        {
            case ProcessingStage.NONE:
            case ProcessingStage.BINARIZE:
                imageToSet.texture = imageToOCR;
                break;
            case ProcessingStage.NOISE: //binarization has finished
                ShowProcessedImage(ocrImages[1]);
                break;
            case ProcessingStage.SEGMENT: //noise reduction just finished
                ShowProcessedImage(ocrImages[2]);
                break;
            case ProcessingStage.POSTNOISE: //segmentation just finished
                ShowProcessedImage(ocrImages[3]);
                break;
            case ProcessingStage.OCR:
                ShowProcessedImage(ocrImages[4]);
                break;
            case ProcessingStage.COMPLETE:
                Debug.Log("OCR complete");
                SaveAllImages();
                imageToSet.texture = imageToOCR;
                currentState = ProcessingStage.NONE;
                oneStepAvailable = true;
                isProcessing = false;
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
        System.Threading.Thread.Sleep(1000);
    }


    public void OneStep()
    {//performs one stage of processs per call
        if (!oneStepAvailable)
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
        oneStepAvailable = true;
        isProcessing = false;
    }
    public void ResetState()
    {
        if (isProcessing) //if some processing is going on 
            return;
        currentState = ProcessingStage.NONE;
    }

    private void SaveTexture(Texture2D texture, string filename)
    {
        byte[] imageRaw = texture.EncodeToPNG();
        string filepath = imageSaveDirectory + filename + ".png";
        File.WriteAllBytes(filepath, imageRaw);
    }
}
