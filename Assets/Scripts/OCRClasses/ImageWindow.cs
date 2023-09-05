using System;

public class ImageWindow
{
    ImageInt image;
    int windowSideX;
    int windowSideY;

    // Integral image use will allow for calculating the sum of the window efficiently irrespective of window size
    private IntegralImage integralImage;
    // Square integral image use will allow for calculating variance (along with integralImage) of the window efficiently irrespective of window size
    private SqrIntegralImage sqrIntegralImage;

    private bool createdIntegralImage = false;
    private bool createdSqrIntegralImage = false;

    // Actual bounds of the window (may vary based on center pixel)
    private int noOfPixels;  // These are used for integral image
    private int wxs, wxs_clamped, wxe, wys, wys_clamped, wye; // wye and wxe are clamped value unlike wxs and wys i.e. wye = wye_clamped
    private int wl, wt, wc;

    private int sum;
    private long sqrSum;

    private static readonly int MAX_INT = int.MaxValue;  // Maximum value for any integer pixel value (signed)
    private static readonly int MIN_INT = 0;  // Minimum value for any integer pixel value (signed)

    public int GetWindowSizeX()
    {
        return windowSideX * 2 + 1;
    }

    public int GetWindowSizeY()
    {
        return windowSideY * 2 + 1;
    }

    public void SetImage(ImageInt _image)
    {
        if (image != _image)
        {
            createdSqrIntegralImage = false;
            createdIntegralImage = false;
        }
        image = _image;
    }

    public ImageInt GetImage()
    {
        return image;
    }

    public void SetWindowSize(int _windowSizeX, int _windowSizeY)
    {
        windowSideX = Math.Max(0, (_windowSizeX - 1) / 2);   // Window must be at least 3 pixels
        windowSideY = Math.Max(0, (_windowSizeY - 1) / 2);   // Window side is the number of pixels on the side of the center pixel
    }

    public void SetWindowSize(int _windowSize)
    {
        SetWindowSize(_windowSize, _windowSize);
    }

    private void Initialize(ImageInt _image, int _windowSizeX, int _windowSizeY)
    {
        SetImage(_image);
        SetWindowSize(_windowSizeX, _windowSizeY);
        CreateIntegralImage();
    }

    public ImageWindow(ImageInt _image, int _windowSize)
    {
        Initialize(_image, _windowSize, _windowSize);
    }

    public ImageWindow(ImageInt _image, int _windowSizeX, int _windowSizeY)
    {
        Initialize(_image, _windowSizeX, _windowSizeY);
    }

    private void CreateIntegralImage()
    {
        if (!createdIntegralImage)
            integralImage = new IntegralImage(image);
        createdIntegralImage = true;
    }

    private void CreateSqrIntegralImage()
    {
        if (!createdSqrIntegralImage)
            sqrIntegralImage = new SqrIntegralImage(image);
        createdSqrIntegralImage = true;
    }

    public void UseSqrIntegralImage()
    {
        CreateSqrIntegralImage();
    }

    void CalculateBounds(int x, int y)
    {
        wxs = x - windowSideX - 1;
        wxs_clamped = Math.Max(wxs, -1);  // Coord before starting point for the actual window
        wxe = Math.Min(x + windowSideX, image.GetWidth() - 1);
        wys = y - windowSideY - 1;
        wys_clamped = Math.Max(wys, -1);
        wye = Math.Min(y + windowSideY, image.GetHeight() - 1);
        noOfPixels = (wxe - wxs_clamped) * (wye - wys_clamped); // No. of pixels inside the actual window
    }

    public int[] GetBounds()
    {
        int[] bounds = { wxs_clamped + 1, wxe, wys_clamped + 1, wye };
        return bounds;
    }

    public int[] GetMaxMin(int x, int y)
    {
        CalculateBounds(x, y);
        int maxval = MIN_INT, minval = MAX_INT;
        for (int i = wxs_clamped + 1; i <= wxe; i++)
        {
            for (int j = wys_clamped + 1; j <= wye; i++)
            {
                int pixelValue = image.pixel[i, j];
                if (pixelValue > maxval)
                    maxval = pixelValue;
                if (pixelValue < minval)
                    minval = pixelValue;
            }
        }
        int[] maxmin = { maxval, minval };
        return maxmin;
    }

    public int GetMax(int x, int y)
    {
        CalculateBounds(x, y);
        int maxval = MIN_INT;
        for (int i = wxs_clamped + 1; i <= wxe; i++)
        {
            for (int j = wys_clamped + 1; j <= wye; j++)
            {
                int pixelValue = image.pixel[i, j];
                if (pixelValue > maxval)
                    maxval = pixelValue;
            }
        }
        return maxval;
    }

    public int GetMin(int x, int y)
    {
        CalculateBounds(x, y);
        int minval = MAX_INT;
        for (int i = wxs_clamped + 1; i <= wxe; i++)
        {
            for (int j = wys_clamped + 1; j <= wye; j++)
            {
                int pixelValue = image.pixel[i, j];
                if (pixelValue < minval)
                    minval = pixelValue;
            }
        }
        return minval;
    }

    public int GetMaxForBinarized(int x, int y)
    {
        if (image.GetType() != ImageInt.TYPE.BIN)
        {
            Console.WriteLine("Cannot find max for un-binarized image");
            return -1;
        }

        FindSum(x, y);
        if (sum > 0) // If any element is greater than zero max is 1
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public int GetMinForBinarized(int x, int y)
    {
        if (image.GetType() != ImageInt.TYPE.BIN)
        {
            Console.WriteLine("Cannot find max for un-binarized image");
            return -1;
        }

        FindSum(x, y);
        if (sum < noOfPixels) // If any element is less than 1, min is 0
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public int GetSum()
    {
        return sum;
    }

    public long GetSqrSum()
    {
        return sqrSum;
    }

    public void FindSum(int x, int y)
    {
        CalculateBounds(x, y);

        if (wxs < 0)
        {
            wl = 0;
        }
        else
        {
            wl = integralImage.pixel[wxs, wye];
        }

        if (wys < 0)
        {
            wt = 0;
        }
        else
        {
            wt = integralImage.pixel[wxe, wys];
        }

        if (wxs < 0 || wys < 0)
        {
            wc = 0;
        }
        else
        {
            wc = integralImage.pixel[wxs, wys];
        }

        sum = integralImage.pixel[wxe, wye] - wl - wt + wc;
    }

    private void FindSqrSum(int x, int y)
    {
        CalculateBounds(x, y);

        long wl, wt, wc;
        if (wxs < 0)
        {
            wl = 0;
        }
        else
        {
            wl = sqrIntegralImage.pixel[wxs, wye];
        }

        if (wys < 0)
        {
            wt = 0;
        }
        else
        {
            wt = sqrIntegralImage.pixel[wxe, wys];
        }

        if (wxs < 0 || wys < 0)
        {
            wc = 0;
        }
        else
        {
            wc = sqrIntegralImage.pixel[wxs, wys];
        }

        sqrSum = sqrIntegralImage.pixel[wxe, wye] - wl - wt + wc;
    }

    public void FindSumAndSqrSum(int x, int y)
    {
        CalculateBounds(x, y);

        if (wxs < 0)
        {
            wl = 0;
        }
        else
        {
            wl = integralImage.pixel[wxs, wye];
        }

        if (wys < 0)
        {
            wt = 0;
        }
        else
        {
            wt = integralImage.pixel[wxe, wys];
        }

        if (wxs < 0 || wys < 0)
        {
            wc = 0;
        }
        else
        {
            wc = integralImage.pixel[wxs, wys];
        }

        sum = integralImage.pixel[wxe, wye] - wl - wt + wc;

        long wln, wtn, wcn;
        if (wxs < 0)
        {
            wln = 0;
        }
        else
        {
            wln = sqrIntegralImage.pixel[wxs, wye];
        }

        if (wys < 0)
        {
            wtn = 0;
        }
        else
        {
            wtn = sqrIntegralImage.pixel[wxe, wys];
        }

        if (wxs < 0 || wys < 0)
        {
            wcn = 0;
        }
        else
        {
            wcn = sqrIntegralImage.pixel[wxs, wys];
        }

        sqrSum = sqrIntegralImage.pixel[wxe, wye] - wln - wtn + wcn;
    }

    public int Mean(int x, int y)
    {
        FindSum(x, y);
        return sum / noOfPixels;
    }

    public int GetImageMean()
    {
        return (integralImage.pixel[image.GetWidth() - 1, image.GetHeight() - 1] / (image.GetWidth() * image.GetHeight()));
    }

    public int Variance(int x, int y, int _mean)
    {
        FindSqrSum(x, y);
        return (int)(sqrSum / noOfPixels - _mean * _mean);
    }

    public int GetImageVariance(int _mean)
    {
        return (int)(sqrIntegralImage.pixel[image.GetWidth() - 1, image.GetHeight() - 1] / (image.GetWidth() * image.GetHeight()) - _mean * _mean);
    }

    public int Variance(int x, int y)
    {
        FindSumAndSqrSum(x, y);
        int _mean = sum / noOfPixels;
        return (int)(sqrSum / noOfPixels - _mean * _mean);
    }
}
