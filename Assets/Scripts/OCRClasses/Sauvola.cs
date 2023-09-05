using System;
using UnityEngine;

public class Sauvola : Binarization
{
    int w;
    float k; // k ranges from 0 to 1
    int R;

    public Sauvola(float _k, int _windowSize)
    {
        k = _k;
        w = _windowSize;
    }

    public void SetParameters(float _k, int _w)
    {
        k = _k;
        w = _w;
    }

    public override void SetImage(ImageInt image)
    {
        base._SetImage(image);
        //int maxP = image.GetMaxValue();
        //int minP = image.GetMinValue();
        //R = (maxP - minP) / 2;
        R = 128;
    }

    public override void ForceSetImage(ImageInt image)
    {
        base._ForceSetImage(image);
        //int maxP = image.GetMaxValue();
        //int minP = image.GetMinValue();
        //R = (maxP - minP) / 2;
        R = 128;
    }

    public void Binarize(ImageWindow imageWindow)
    {
        if (sourceImage == null)
        {
            Debug.Log("No source image set");
            return;
        }
        else if (binarizedImage == null)
        {
            Debug.Log("Binarized image not set for ImageInt for binarization");
            binarizedImage = new ImageInt(sourceImage.GetWidth(), sourceImage.GetHeight());
            binarizedImage.SetType(ImageInt.TYPE.BIN);
        }

        if (imageWindow.GetImage() != sourceImage)
        {
            Debug.Log("ImageInt window has a different source image");
            return;
        }

        int threshold;
        int mean; // mean centered around a window of size w
        double sd; // standard deviation centered around a window of size w

        imageWindow.UseSqrIntegralImage();

        // For every pixel in the image, calculate threshold value and compare to assign binarization
        for (int i = 0; i < sourceImage.GetWidth(); i++)
        {
            for (int j = 0; j < sourceImage.GetHeight(); j++)
            {
                mean = imageWindow.Mean(i, j);
                sd = Math.Sqrt(imageWindow.Variance(i, j, mean));
                threshold = (int)(mean * (1 + k * (sd / R - 1)));

                if (sourceImage.pixel[i, j] < threshold)
                {
                    binarizedImage.pixel[i, j] = 1;
                }
                else
                {
                    binarizedImage.pixel[i, j] = 0;
                }
            }
        }
    }

    public override void Binarize()
    {
        ImageWindow imageWindow = new ImageWindow(sourceImage, w);
        Binarize(imageWindow);
    }

    public void Binarize(int secondaryMean, float weight)
    {
        binarizedImage = new ImageInt(sourceImage.GetWidth(), sourceImage.GetHeight());
        int threshold;
        int mean; // mean centered around a window of size w
        double sd; // standard deviation centered around a window of size w
        int R = 128; // dynamic range of standard deviation
        ImageWindow imageWindow = new ImageWindow(sourceImage, w);

        // For every pixel in the image, calculate threshold value and compare to assign binarization
        for (int i = 0; i < sourceImage.GetWidth(); i++)
        {
            for (int j = 0; j < sourceImage.GetHeight(); j++)
            {
                mean = imageWindow.Mean(i, j);
                mean = (int)(mean * (1 - weight) + secondaryMean * weight);
                sd = Math.Sqrt(imageWindow.Variance(i, j, mean));
                threshold = (int)(mean * (1 + k * (sd / R - 1)));

                if (sourceImage.pixel[i, j] < threshold)
                {
                    binarizedImage.pixel[i, j] = 1;
                }
                else
                {
                    binarizedImage.pixel[i, j] = 0;
                }
            }
        }
    }
}
