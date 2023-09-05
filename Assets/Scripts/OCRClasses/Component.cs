using System;

public class Component
{
    private const int MaxComp = 99999;

    private int minX;
    private int maxX;
    private int minY;
    private int maxY;
    private int height;
    private int width;
    private int countOfBlackPixels;
    private bool componentSet;
    private ImageInt selfImage;

    private float areaDensityScore;
    private float pageSizeScore;
    private float aspectRatioScore;

    public Component()
    {
        minX = MaxComp;
        maxX = 0;
        minY = MaxComp;
        maxY = 0;
        countOfBlackPixels = 0;
        areaDensityScore = 0;
        pageSizeScore = 0;
        aspectRatioScore = 0;
        componentSet = false;
    }

    public void SetRect()
    {
        width = maxX - minX + 1;
        height = maxY - minY + 1;
    }

    public int[] GetRect()
    {
        int[] rectt = new int[4];
        rectt[0] = minX;
        rectt[1] = minY;
        rectt[2] = width;
        rectt[3] = height;
        return rectt;
    }

    public float GetArea()
    {
        SetRect();
        return height * width;
    }

    public void IncreaseCountOfBlackPixels()
    {
        countOfBlackPixels++;
    }

    public void SetValues(int x, int y)
    {
        if (minX == MaxComp && maxX == 0 && minY == MaxComp && maxY == 0)
        {
            componentSet = true;
        }

        if (x < minX)
        {
            minX = x;
        }

        if (x > maxX)
        {
            maxX = x;
        }

        if (y < minY)
        {
            minY = y;
        }

        if (y > maxY)
        {
            maxY = y;
        }
    }

    public int GetMinX()
    {
        return minX;
    }

    public int GetMaxX()
    {
        return maxX;
    }

    public int GetMinY()
    {
        return minY;
    }

    public int GetMaxY()
    {
        return maxY;
    }

    public int GetCountOfBlackPixels()
    {
        return countOfBlackPixels;
    }

    public void MergeComp(Component c)
    {
        minX = c.GetMinX() < minX ? c.GetMinX() : minX;
        minY = c.GetMinY() < minY ? c.GetMinY() : minY;
        maxX = c.GetMaxX() > maxX ? c.GetMaxX() : maxX;
        maxY = c.GetMaxY() > maxY ? c.GetMaxY() : maxY;
        countOfBlackPixels += c.GetCountOfBlackPixels();
    }

    public void ShowValues(int i)
    {
        Console.WriteLine($"{i}: {minX} {minY} {maxX} {maxY}");
    }

    public void SetImage(ImageInt img)
    {
        if (componentSet)
        {
            SetRect();
            selfImage = new ImageInt(width, height, ImageInt.TYPE.BIN);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    selfImage.pixel[i, j] = img.pixel[i + minX, j + minY];
                }
            }
        }
    }

    public ImageInt GetImage()
    {
        return selfImage;
    }

    public void AddComponentOnImage(ImageInt img)
    {
        if (componentSet)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    img.pixel[i + minX, j + minY] = selfImage.pixel[i, j];
                }
            }
        }
    }

    private void SetAreaDensityScore()
    {
        if (componentSet)
        {
            SetRect();
            areaDensityScore = (float)GetCountOfBlackPixels() / GetArea();
        }
    }

    public float GetAreaDensityScore()
    {
        SetAreaDensityScore();
        return areaDensityScore;
    }

    private void SetPageSizeScore(int imgSizeX, int imgSizeY)
    {
        pageSizeScore = GetArea() / (float)(imgSizeX * imgSizeY) * 100f;
    }

    public float GetPageSizeScore(int imgSizeX, int imgSizeY)
    {
        SetPageSizeScore(imgSizeX, imgSizeY);
        return pageSizeScore;
    }

    private void SetAspectRatioScore()
    {
        aspectRatioScore = (float)selfImage.GetWidth() / (float)selfImage.GetWidth();
        if (aspectRatioScore > 3f)
        {
            aspectRatioScore = 1 / aspectRatioScore;
        }

        if (aspectRatioScore > 1)
        {
            aspectRatioScore = 1 - 1 / aspectRatioScore;
        }
        aspectRatioScore = 1 - aspectRatioScore;
    }

    public float GetAspectRatioScore()
    {
        SetAspectRatioScore();
        return aspectRatioScore;
    }
}
