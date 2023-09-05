public class SqrIntegralImage
{
    protected ImageInt image;  // the image of which to create the integral image for
    public long[,] pixel;  // size must be big enough to hold the sum of square of all pixels of the image

    public SqrIntegralImage(ImageInt _image)
    {
        image = _image;
        ConstructIntegralImage();
    }

    protected void ConstructIntegralImage()
    {
        int width = image.GetWidth();
        int height = image.GetHeight();

        pixel = new long[width, height];
        pixel[0, 0] = image.pixel[0, 0] * image.pixel[0, 0]; // first value of integral image is the initial pixel value squared 

        // Summing pixel-squares along the first row
        for (int i = 1; i < width; i++)
            pixel[i, 0] = image.pixel[i, 0] * image.pixel[i, 0] + pixel[i - 1, 0];

        // Summing pixel-squares along the first column
        for (int i = 1; i < height; i++)
            pixel[0, i] = image.pixel[0, i] * image.pixel[0, i] + pixel[0, i - 1];

        // Summing for the remaining positions
        for (int i = 1; i < width; i++)
        {
            for (int j = 1; j < height; j++)
            {
                pixel[i, j] = pixel[i - 1, j] + pixel[i, j - 1] + image.pixel[i, j] * image.pixel[i, j] - pixel[i - 1, j - 1];
            }
        }
    }
}
