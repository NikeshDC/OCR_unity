using System;

public abstract class Binarization
{
    protected ImageInt sourceImage;
    protected ImageInt binarizedImage;

    public abstract void Binarize();

    protected void CheckBeforeBinarize()
    {
        if (sourceImage == null)
        {
            Console.WriteLine("No source image set");
            return;
        }
        else if (binarizedImage == null)
        {
            Console.WriteLine("Binarized image not set for ImageInt for binarization");
            binarizedImage = new ImageInt(sourceImage.GetWidth(), sourceImage.GetHeight());
            binarizedImage.SetType(ImageInt.TYPE.BIN);
        }
    }

    private static void RectifyImage(ImageInt image)
    {
        if (image == null)
        {
            Console.WriteLine("Source image null given!! <Binarization ImageInt Setting>");
        }
        else if (image.GetType() == ImageInt.TYPE.RGB)
        {
            // Console.WriteLine("Converted RGB image to gray for binarization");
            ImageUtility.ConvertRGB2Gray(image);
        }
        else if (image.GetType() == ImageInt.TYPE.BIN)
        {
            Console.WriteLine("Source image for binarization in binary format!!");
        }
    }

    private void SetImage(ImageInt _srcImage, bool forceSet)
    {
        // If force set, then redo everything required; else, check the image to see if it is already being used
        if (!forceSet && _srcImage == sourceImage)
            return;
        sourceImage = _srcImage;
        // Convert RGB image to grayscale if necessary
        RectifyImage(_srcImage);

        binarizedImage = new ImageInt(sourceImage.GetWidth(), sourceImage.GetHeight(), ImageInt.TYPE.BIN);
    }

    public abstract void SetImage(ImageInt srcImage);
    protected void _SetImage(ImageInt srcImage)  //use this to perform basic rectification to images
    {
        SetImage(srcImage, false);
    }

    public abstract void ForceSetImage(ImageInt srcImage);
    public void _ForceSetImage(ImageInt srcImage)
    {
        SetImage(srcImage, true);
    }

    public ImageInt GetBinarizedImage()
    {
        return binarizedImage;
    }

    public static ImageInt SimpleThreshold(ImageInt image, int threshold)
    {
        // Convert RGB image to grayscale if necessary
        RectifyImage(image);

        ImageInt binarizedImage = new ImageInt(image.GetWidth(), image.GetHeight());
        binarizedImage.SetType(ImageInt.TYPE.BIN);
        for (int i = 0; i < image.GetWidth(); i++)
        {
            for (int j = 0; j < image.GetHeight(); j++)
            {
                if (image.pixel[i, j] < threshold)
                {
                    binarizedImage.pixel[i, j] = 1;
                }
                else
                {
                    binarizedImage.pixel[i, j] = 0;
                }
            }
        }
        return binarizedImage;
    }
}