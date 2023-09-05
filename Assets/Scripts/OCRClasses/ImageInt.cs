using UnityEngine;

public class ImageInt
{
    // Only supports 8-bit depth by default
    public int[,] pixel;
    protected int sizeX; // width
    protected int sizeY; // height
    public enum TYPE { GRAY, RGB, BIN, COMP };
    protected TYPE type;

    private void Initialize(int _sizeX, int _sizeY, TYPE _type)
    {
        sizeX = _sizeX;
        sizeY = _sizeY;
        pixel = new int[sizeX, sizeY];
        type = _type;
    }

    public ImageInt GetCroppedImage(int minX, int maxX, int minY, int maxY)
    {
        ImageInt croppedImage = new ImageInt(maxX - minX + 1, maxY - minY + 1, this.type);
        for (int i = 0; i < croppedImage.GetWidth(); i++)
        {
            for (int j = 0; j < croppedImage.GetHeight(); j++)
            {
                croppedImage.pixel[i, j] = pixel[i + minX, j + minY];
            }
        }
        return croppedImage;
    }
    public ImageInt GetCroppedImage(RectangularBound<int> bound)
    {
        return GetCroppedImage(bound.MinX, bound.MaxX, bound.MinY, bound.MaxY);
    }

    public ImageInt(ImageInt _image)
    {
        Initialize(_image.GetWidth(), _image.GetHeight(), _image.GetType());
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                pixel[i, j] = _image.pixel[i, j];
            }
        }
    }

    public ImageInt(int _sizeX, int _sizeY, TYPE _type)
    {
        Initialize(_sizeX, _sizeY, _type);
    }

    public ImageInt(int _sizeX, int _sizeY)
    {
        Initialize(_sizeX, _sizeY, TYPE.GRAY);
    }

    public void SetType(TYPE _type)
    { type = _type; }

    public new TYPE GetType()
    { return type; }

    public int GetWidth()
    { return sizeX; }

    public int GetMaxX()
    { return sizeX - 1; }

    public int GetHeight()
    { return sizeY; }

    public int GetMaxY()
    { return sizeY - 1; }

    public int GetMaxValue()
    {
        int maxValue = 0;
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (pixel[i, j] > maxValue) 
                    maxValue = pixel[i, j];
            }
        }
        return maxValue;
    }
    public int GetMinValue()
    {
        int minValue = 99999;
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (pixel[i, j] < minValue)
                    minValue = pixel[i, j];
            }
        }
        return minValue;
    }


    public void LogicalAnd(ImageInt image)
    {// perform pixelwise subtraction
        if (image.GetType() != TYPE.BIN || this.type != TYPE.BIN)
        {
            Debug.Log("Cannot logical-and unbinarized Image!");
            return;
        }
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                pixel[i, j] = pixel[i, j] & image.pixel[i, j];
            }
        }
    }

    public void Subtract(ImageInt image)
    {
        Subtract(image, 1.0f);
    }

    public void Subtract(ImageInt image, float reducer)
    {// perform pixelwise subtraction
        if (image.GetWidth() != GetWidth() || image.GetHeight() != GetHeight())
        {
            Debug.Log("Cannot subtract images of different size!");
            return;
        }
        if (image.GetType() != GetType())
        {
            Debug.Log("Cannot subtract images of different types!");
            return;
        }
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                pixel[i, j] = Mathf.Max(pixel[i, j] - (int)(image.pixel[i, j] * reducer), 0);
            }
        }
    }
}
