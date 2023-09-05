using System;

public class ImageHistogram
{
    public int[] Level;
    public double[] LevelNormalized;
    private ImageInt image;
    int L;

    private void Initialize(ImageInt _image)
    {
        image = _image;

        if (image.GetType() == ImageInt.TYPE.BIN)
            L = 2; // 1 bit depth for binary image
        else if (image.GetType() == ImageInt.TYPE.GRAY)
            L = 256; // 8 bit depth for grayscale image
        else if (image.GetType() == ImageInt.TYPE.RGB)
            L = 256 * 256 * 256; // 24 bit depth for RGB image

        Level = new int[L]; // Default initialized value for each element is 0
        LevelNormalized = new double[L]; // Level is normalized to [0-1]
    }

    private void ConstructHistogram(int xs, int xe, int ys, int ye)
    {
        // Check if any bound is greater than image size, including both starting and ending bound
        int imgXE = image.GetMaxX();
        int imgYE = image.GetMaxY();
        if (xs > imgXE || xe > imgXE || ys > imgYE || ye > imgYE)
        {
            Console.WriteLine("Given bound exceeds ImageInt bound (in histogram)");
            return;
        }
        if (xs >= xe || ys >= ye || xs < 0 || xe < 0 || ys < 0 || ye < 0)
        {
            Console.WriteLine("Given bounds are not appropriate in histogram");
            return;
        }

        // Constructing histogram
        for (int i = xs; i <= xe; i++)
            for (int j = ys; j <= ye; j++)
                Level[image.pixel[i, j]]++; // Increment value for each level

        // Normalizing level values
        int N = (xe - xs + 1) * (ye - ys + 1); // Number of pixels in image

        for (int i = 0; i < L; i++)
            LevelNormalized[i] = (double)Level[i] / N;
    }

    public ImageHistogram(ImageInt _image)
    {
        Initialize(_image);
        int imgXE = image.GetMaxX();
        int imgYE = image.GetMaxY();
        ConstructHistogram(0, imgXE, 0, imgYE);
    }

    public ImageHistogram(ImageInt _image, int xs, int xe, int ys, int ye)
    {
        Initialize(_image);
        ConstructHistogram(xs, xe, ys, ye);
    }

    public int GetMode()
    {
        int mode = 0;
        for (int i = 1; i < L; i++)
        {
            if (Level[i] > mode)
                mode = i;
        }
        return mode;
    }

    public double GetEntropy()
    {
        double entropy = 0.0;
        double log2base = Math.Log(2);
        for (int i = 0; i < L; i++)
        {
            if (LevelNormalized[i] != 0)
                entropy += -LevelNormalized[i] * Math.Log(LevelNormalized[i]) / log2base;
        }
        return entropy;
    }

    public int GetLevel() { return L; }
}
