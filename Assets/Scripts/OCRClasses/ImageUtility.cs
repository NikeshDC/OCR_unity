using UnityEngine;

public class ImageUtility
{
    public static ImageInt GetImage(Texture2D texture)
    {
        return GetImage(texture.GetPixels(), texture.width, texture.height);
    }
        public static ImageInt GetImage(Color[] pixelsarray, int width, int height)
    {
        Debug.Log("creating New image");
        ImageInt image = new ImageInt(width, height, ImageInt.TYPE.RGB);

        Debug.Log("New ImageInt created");

        for (int i = 0; i < image.GetWidth(); i++)
        {
            for (int j = 0; j < image.GetHeight(); j++)
            {
                //Debug.Log("Pixel: "+i+", "+j);
                int pixelValue = ConvertColorToPackedInt(pixelsarray[i + j * width]);
                image.pixel[i, j] = pixelValue;
            }
        }

        return image;
    }

    public static Texture2D GetTexture(ImageInt image)
    {
        if (image == null)
        {
            Debug.Log("Cannot create BufferedImage object. ImageInt object null.");
            return null;
        }

        Texture2D texture = new Texture2D(image.GetWidth(), image.GetHeight());

        if (image.GetType() == ImageInt.TYPE.RGB)
        {
            for (int i = 0; i < image.GetWidth(); i++)
            {
                for (int j = 0; j < image.GetHeight(); j++)
                {
                    texture.SetPixel(i, j, ConvertPackedIntToColor(image.pixel[i, j]));
                }
            }
        }
        else if (image.GetType() == ImageInt.TYPE.GRAY)
        {
            for (int i = 0; i < image.GetWidth(); i++)
            {
                for (int j = 0; j < image.GetHeight(); j++)
                {
                    float grayValue = (float)image.pixel[i, j] / 0xFF;
                    Color grayColor = new Color(grayValue, grayValue, grayValue);
                    texture.SetPixel(i, j, grayColor);
                }
            }
        }
        else if (image.GetType() == ImageInt.TYPE.BIN)
        {
            Debug.Log("hello bin to texture");
            int blacks = 0;
            for (int i = 0; i < image.GetWidth(); i++)
            {
                for (int j = 0; j < image.GetHeight(); j++)
                {
                    if (image.pixel[i, j] == 1)
                    {
                        texture.SetPixel(i, j, Color.black);
                        blacks++;
                    }
                    else
                        texture.SetPixel(i, j, Color.white);
                }
            }
            Debug.Log("text: "+ blacks);
        }

        return texture;
    }

    public static int ConvertColorToPackedInt(Color color)
    {
        int r = (int)(color.r * 0xFF);
        int g = (int)(color.g * 0xFF);
        int b = (int)(color.b * 0xFF);
        int pixelValue = r << 16 | g << 8 | b;
        return pixelValue;
    }
    public static Color ConvertPackedIntToColor(int pixelValue)
    {
        float r = (float)((pixelValue >> 16) & 0xFF) /0xFF;
        float g = (float)((pixelValue >> 8) & 0xFF) / 0xFF;
        float b = (float)(pixelValue & 0xFF) / 0xFF;
        Color color = new Color(r,g,b);
        return color;
    }

    public static void ConvertRGB2Gray(ImageInt image)
    {
        if (image.GetType() == ImageInt.TYPE.GRAY)
        {
            return; // ImageInt is already in grayscale type
        }

        for (int i = 0; i < image.GetWidth(); i++)
        {
            for (int j = 0; j < image.GetHeight(); j++)
            {
                int pixelValue = image.pixel[i, j];
                int r = (pixelValue >> 16) & 0xFF;
                int g = (pixelValue >> 8) & 0xFF;
                int b = pixelValue & 0xFF;
                int grayValue = (int)(0.2126 * r + 0.7152 * g + 0.0722 * b); // Luminance formula
                image.pixel[i, j] = grayValue;
            }
        }
        image.SetType(ImageInt.TYPE.GRAY);
    }

    //image processing techniques below
    public static ImageInt EnhanceText(ImageInt img, int imageMean, int imageSD)
    {//perform enhancement using pixel range segmentation approach for garyscale image
        //sd- standard deviation of the whole image
        ImageInt enhancedImg = new ImageInt(img.GetWidth(), img.GetHeight());

        int Na = 9;
        int Imax = img.GetMaxValue();  //maximum intensity of pixel
        int Imin = img.GetMinValue();  //minimum intensity of pixel

        int Fseg = Imin + (Imax - Imin) / Na;   //first segment (text segment) largest value
        int Lseg = Imin + (Imax - Imin) / Na;   //last segment (background) smallest value

        for (int i = 0; i < img.GetWidth(); i++)
        {
            for (int j = 0; j < img.GetHeight(); j++)
            {
                //segmentation into first or last segments
                if (img.pixel[i, j] < Fseg)
                    enhancedImg.pixel[i, j] = 0;   //intensity value for text pixels
                else if (img.pixel[i, j] > Lseg)
                    enhancedImg.pixel[i, j] = imageMean;
                else
                    enhancedImg.pixel[i, j] = img.pixel[i, j];

                //uniform illumination
                if (enhancedImg.pixel[i, j] >= (imageMean - imageSD / 2) && enhancedImg.pixel[i, j] <= (imageMean + imageSD / 2))
                    enhancedImg.pixel[i, j] = imageMean;
            }
        }
        return enhancedImg;
    }


    //morphological operations
    public static ImageInt Dilate(ImageInt image, ImageWindow imageWindow)
    {
        if (image.GetType() != ImageInt.TYPE.BIN)
        { Debug.Log("Doesn't support dilation for non-binary image"); return null; }

        ImageInt dilatedImg = new ImageInt(image.GetWidth(), image.GetHeight(), ImageInt.TYPE.BIN);
        //perform dilation
        for (int i = 0; i < image.GetWidth(); i++)
            for (int j = 0; j < image.GetHeight(); j++)
                dilatedImg.pixel[i, j] = imageWindow.GetMaxForBinarized(i, j);

        return dilatedImg;
    }
    public static ImageInt Dilate(ImageInt image, int windowSizeX, int windowSizeY)
    {
        ImageWindow imageWindow = new ImageWindow(image, windowSizeX, windowSizeY);
        ImageInt dilatedImg = Dilate(image, imageWindow);
        return dilatedImg;
    }
    public static ImageInt Dilate(ImageInt img, int windowSize)
    {
        ImageInt dilatedImg = Dilate(img, windowSize, windowSize);
        return dilatedImg;
    }

    public static ImageInt Erode(ImageInt image, ImageWindow imageWindow)
    {
        if (image.GetType() != ImageInt.TYPE.BIN)
        { Debug.Log("Doesn't support erosion for non-binary image"); return null; }

        ImageInt erodedImg = new ImageInt(image.GetWidth(), image.GetHeight(), ImageInt.TYPE.BIN);
        //perform erosion
        for (int i = 0; i < image.GetWidth(); i++)
            for (int j = 0; j < image.GetHeight(); j++)
                erodedImg.pixel[i, j] = imageWindow.GetMinForBinarized(i, j);
        return erodedImg;
    }
    public static ImageInt Erode(ImageInt image, int windowSizeX, int windowSizeY)
    {
        ImageWindow imageWindow = new ImageWindow(image, windowSizeX, windowSizeY);
        ImageInt erodedImg = Erode(image, imageWindow);
        return erodedImg;
    }
    public static ImageInt Erode(ImageInt img, int windowSize)
    {
        ImageInt erodedImg = Erode(img, windowSize, windowSize);
        return erodedImg;
    }

    public static ImageInt Open(ImageInt img, int windowSizeX, int windowSizeY)
    {
        ImageInt erodedImage = Erode(img, windowSizeX, windowSizeY);
        ImageInt openedImage = Dilate(erodedImage, windowSizeX, windowSizeY);
        return openedImage;
    }
    public static ImageInt Open(ImageInt img, int windowSize)
    {
        return Open(img, windowSize, windowSize);
    }

    public static ImageInt Close(ImageInt img, int windowSizeX, int windowSizeY)
    {
        ImageInt dilatedImage = Dilate(img, windowSizeX, windowSizeY);
        ImageInt closedImage = Erode(dilatedImage, windowSizeX, windowSizeY);
        return closedImage;
    }
    public static ImageInt Close(ImageInt img, int windowSize)
    {
        return Close(img, windowSize, windowSize);
    }

    public static ImageInt AddComponentsOnNewImage(Component[] components, int componentsCount, int sizeX, int sizeY)
    {
        ImageInt newImage = new ImageInt(sizeX, sizeY);
        newImage.SetType(ImageInt.TYPE.BIN);
        for (int i = 0; i < componentsCount; i++)
        {
            components[i].AddComponentOnImage(newImage);
        }
        return newImage;
    }
}
