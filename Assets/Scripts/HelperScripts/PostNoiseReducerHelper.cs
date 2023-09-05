using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostNoiseReducerHelper
{
    public ImageInt ReduceNoise(ImageInt sourceImage, List<RectangularBound<int>> segments)
    {
        if (sourceImage.GetType() != ImageInt.TYPE.BIN)
        {
            Debug.Log("Non-binary image sent to post noise reducer");
            return null;
        }

        Debug.Log("Performing post noise reduction - "+ Time.realtimeSinceStartup);

        ImageInt combinedImage = new ImageInt(sourceImage.GetWidth(), sourceImage.GetHeight(), ImageInt.TYPE.BIN);
        foreach (RectangularBound<int> bound in segments)
        {
            ImageInt img = sourceImage.GetCroppedImage(bound);
            NoiseReducer noiseReducer = new NoiseReducer();
            int[] widthHeight = noiseReducer.GetAverageDimensions(img);
            int width = widthHeight[0];
            int height = widthHeight[1];

            int heightScoreThreshold = height * 10;
            int widthScoreThreshold = width * 10;

            Segmentation segmentation = new Segmentation(img);
            segmentation.Segment();

            Component[] heightFilteredcomponents = noiseReducer.CheckHeightScorePX(segmentation.GetComponents(),
                    segmentation.GetComponentsCount(),
                    heightScoreThreshold);
            segmentation.SetComponentsArray(heightFilteredcomponents);
            Component[] widthFilteredcomponents = noiseReducer.CheckWidthScorePX(segmentation.GetComponents(),
                    segmentation.GetComponentsCount(),
                    widthScoreThreshold);
            segmentation.SetComponentsArray(widthFilteredcomponents);
            img = ImageUtility.AddComponentsOnNewImage(segmentation.GetComponents(), segmentation.GetComponentsCount(), img.GetWidth(), img.GetHeight());
            for (int i = 0; i < img.GetWidth(); i++)
            {
                for (int j = 0; j < img.GetHeight(); j++)
                {
                    combinedImage.pixel[i + bound.MinX, j + bound.MinY] = img.pixel[i, j];
                }
            }
        }

        return combinedImage;
    }
}
