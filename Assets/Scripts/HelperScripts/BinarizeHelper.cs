using UnityEngine;

public class BinarizeHelper
{
    public ImageInt Binarize(ImageInt sourceImage)
    {
        Debug.Log("Performing binarization");

        KPrediction predictor = new KPrediction(sourceImage);
        float k = (float)predictor.GetK();
        Debug.Log("Predicted K: " + k);
        int w = (int)(Mathf.Min(sourceImage.GetWidth(), sourceImage.GetHeight()) * 0.1f);
        Binarization bin = new Sauvola(k, w);
        bin.SetImage(sourceImage);
        bin.Binarize();

        if (w * 10 > 1000)
        {//if image size greater than thousand apply erosion/dilation
            ImageInt openedImage = ImageUtility.Open(bin.GetBinarizedImage(), 3);
            ImageInt closedImage = ImageUtility.Close(openedImage, 3);
            return closedImage;
        }
        else
            return bin.GetBinarizedImage();
    }
}
