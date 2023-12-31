using UnityEngine;
using System.Collections.Generic;

public class TopDownSegmenterHelper 
{
   public List<RectangularBound<int>> GetSegments(ImageInt sourceImage)
   {
        if (sourceImage.GetType() != ImageInt.TYPE.BIN)
        {
            Debug.Log("Non-binary image sent to top-down segmenter");
            return null;
        }

        Debug.Log("Performing top down segmentation");

        TopDownSegmenter segmenter = new TopDownSegmenter(sourceImage);
        segmenter.Segment();
        List<RectangularBound<int>> segments = segmenter.GetSegments();

        return segments;
    }
}
