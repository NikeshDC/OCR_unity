using System;

public class Otsu : Binarization
{
    int L; // Assuming bit depth of the image is 8-bits; max level for the histogram is 255
    private int threshold;
    ImageHistogram histogram;

    public double Sb;
    public double Sw;
    public double U0;
    public double U1;
    public double S0;
    public double S1;
    public double W0;
    public double W1;

    public override void SetImage(ImageInt image)
    {
        if (image != sourceImage)
        {
            base._SetImage(image);
            histogram = new ImageHistogram(image);
        }
    }
    public override void ForceSetImage(ImageInt image)
    {
        base._ForceSetImage(image);
        histogram = new ImageHistogram(image);
    }

    public ImageHistogram GetHistogram()
    {
        return histogram;
    }

    public int GetThreshold()
    { return threshold; }

    public void CalculateThreshold()
    {
        CalculateThreshold(false, 0, 0, 0, 0); // Calculate globally
    }
    public void CalculateThresholdLocally(int xs, int xe, int ys, int ye)
    {
        CalculateThreshold(true, xs, xe, ys, ye);
    }
    private void CalculateThreshold(bool local, int xs, int xe, int ys, int ye)
    {
        ImageHistogram histogram;
        if (local)
            histogram = new ImageHistogram(sourceImage, xs, xe, ys, ye);
        else
            histogram = this.histogram;
        L = histogram.GetLevel();

        double uT = 0.0;
        for (int i = 0; i < L; i++)
            uT += i * histogram.LevelNormalized[i];

        double uk, wk; // Expected level (mean) and probability of occurrence for probable text pixels separated by threshold 'k'
        double sb; // Between-class variance that is a measure of the goodness of threshold separating the background and text
        double maxSb = 0.0; // Sb must be positive
        for (int k = 0; k < (L - 1); k++)
        {
            uk = 0.0;
            wk = 0.0;
            for (int i = 0; i <= k; i++)
                uk += i * histogram.LevelNormalized[i];

            for (int i = 0; i <= k; i++)
                wk += histogram.LevelNormalized[i];
            if(wk != 0)
                sb = Math.Pow((uT * wk - uk), 2) / (wk * (1 - wk));
            else
                sb = 0.0;

            if (maxSb < sb)
            {
                maxSb = sb;
                threshold = k; // All pixels from 0 up to and including k are 'text'
            }
        }

        // Calculate all measures with the threshold obtained
        uk = 0.0;
        wk = 0.0;
        for (int i = 0; i <= threshold; i++)
            uk += i * histogram.LevelNormalized[i];

        for (int i = 0; i <= threshold; i++)
            wk += histogram.LevelNormalized[i];

        W0 = wk;
        W1 = 1.0 - W0;
        U0 = uk / wk;
        U1 = (uT - uk) / (1 - wk);
        Sb = Math.Pow((uT * wk - uk), 2) / (wk * (1 - wk));

        S0 = 0.0;
        for (int i = 0; i <= threshold; i++)
            S0 += Math.Pow((i - U0), 2) * histogram.LevelNormalized[i];
        S0 = S0 / W0;

        S1 = 0.0;
        for (int i = threshold + 1; i < L; i++)
            S1 += Math.Pow((i - U1), 2) * histogram.LevelNormalized[i];
        S1 = S1 / W1;

        Sw = W0 * S0 + W1 * S1;
    }

    public override void Binarize()
    {
        CalculateThreshold();
        for (int i = 0; i < sourceImage.GetWidth(); i++)
        {
            for (int j = 0; j < sourceImage.GetHeight(); j++)
            {
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
