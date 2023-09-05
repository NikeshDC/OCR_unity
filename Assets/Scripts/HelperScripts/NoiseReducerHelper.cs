using UnityEngine;

public class NoiseReducerHelper 
{
    public ImageInt ReduceNoise(ImageInt sourceImage)
    {
        if(sourceImage.GetType() != ImageInt.TYPE.BIN)
        {
            Debug.Log("Non-binary image sent to noise reducer");
            return null;
        }

        Debug.Log("Performing noise reduction - " + Time.realtimeSinceStartup);

        NoiseReducer noiseRed = new NoiseReducer(sourceImage, 0.005f, 0.015f, 0.2f, 0.1f, 0.05f, 0.005f);
        ImageInt denoisedImage = noiseRed.GetCleanImage();

        //NoiseReducer noiseRed2 = new NoiseReducer(denoisedImage, 0.005f, 0.015f, 0.2f, 0.1f, 0.05f, 0.005f);
        ImageInt denoisedImage2 = noiseRed.GetCleanImage2();

        return denoisedImage2;
    }
}
