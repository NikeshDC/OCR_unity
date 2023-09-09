using System.Collections;
using UnityEngine;

public class TesseractHelper
{
    private bool tesseractEngineReady = false;
    TesseractDriver tesseractDriver;

    public bool IsTesseractEngineReady() { return tesseractEngineReady; }

    // Start is called before the first frame update
    public TesseractHelper()
    {
        tesseractDriver = new TesseractDriver();
        tesseractDriver.Setup(OnTesseractSetupComplete);
    }

    public void OnTesseractSetupComplete()
    { 
        tesseractEngineReady = true;
    }

    public string GetText(Texture2D imageToOCR)
    {
        if (!tesseractEngineReady)
            return null;
        return tesseractDriver.Recognize(imageToOCR);
    }
}
