using System;
using UnityEngine;

public class KPrediction
{
    private double predictedK;

    public KPrediction(ImageInt image)
    {
        if (image.GetType() != ImageInt.TYPE.GRAY)
            Debug.Log("ImageInt not in grayscale. <KPrediction>");
        Otsu otsu = new Otsu();
        otsu.SetImage(image);
        otsu.CalculateThreshold();
        double s1 = otsu.S1;
        double u0 = otsu.U0;
        double s1byu0SqrRoot = Math.Pow((s1 / u0), 0.5);

        ImageHistogram hist = otsu.GetHistogram();
        double entropy = hist.GetEntropy();

        // Linear regression formula obtained by experimenting with dataset
        predictedK = 0.149662 * s1byu0SqrRoot + 0.025375 * entropy - 0.11769;
    }

    public double GetK()
    {
        return predictedK;
    }
}
