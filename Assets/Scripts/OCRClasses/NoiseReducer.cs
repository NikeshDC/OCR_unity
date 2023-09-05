using System;

public class NoiseReducer
{
    private ImageInt img;
    private Segmentation segmentation;
    private float borderProximityThreshold1;
    private float borderProximityThreshold2;
    private float heightScoreThreshold1;
    private float heightScoreThreshold2;
    private float dilationPercentage;
    private float whitespaceThresholdFactor;

    public NoiseReducer()
    {

    }

    public NoiseReducer(ImageInt _img, float _proximityThreshold1, float _proximityThreshold2, float _heightThreshold1,
        float _heightThreshold2, float _dilationPercentage, float _whitespaceThresholdFactor)
    {
        img = _img;
        img = RemoveBorderWhiteSpace();
        borderProximityThreshold1 = _proximityThreshold1;
        borderProximityThreshold2 = _proximityThreshold2;
        heightScoreThreshold1 = _heightThreshold1;
        heightScoreThreshold2 = _heightThreshold2;
        dilationPercentage = _dilationPercentage;
        whitespaceThresholdFactor = _whitespaceThresholdFactor;
        segmentation = new Segmentation(img);
        segmentation.Segment();
    }

    public ImageInt GetCleanImage()
    {
        GetTextComponents(segmentation.GetComponents(),
            segmentation.GetComponentsCount());
        ImageInt denoisedImage = ImageUtility.AddComponentsOnNewImage(segmentation.GetComponents(),
            segmentation.GetComponentsCount(), img.GetWidth(), img.GetHeight());
        return denoisedImage;
    }

    public ImageInt GetCleanImage2()
    {
        GetTextComponents2(segmentation.GetComponents(),
            segmentation.GetComponentsCount());
        ImageInt denoisedImage = ImageUtility.AddComponentsOnNewImage(segmentation.GetComponents(),
            segmentation.GetComponentsCount(), img.GetWidth(), img.GetHeight());
        return denoisedImage;
    }

    public void GetTextComponents(Component[] comps, int componentsCount)
    {
        Component[] filteredComponents1 = CheckBorderProximity(segmentation.GetComponents(),
            segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents1);

        Component[] filteredComponents2 = CheckHeightScore(filteredComponents1, segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents2);

        Component[] filteredComponents3 = CheckWidthScore(filteredComponents2, segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents3);
    }

    public void GetTextComponents2(Component[] comps, int componentsCount)
    {
        Component[] filteredComponents1 = CheckBorderProximity2(segmentation.GetComponents(),
            segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents1);

        Component[] filteredComponents2 = CheckHeightScore(filteredComponents1, segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents2);

        Component[] filteredComponents3 = CheckWidthScore(filteredComponents2, segmentation.GetComponentsCount());
        segmentation.SetComponentsArray(filteredComponents3);
    }

    public Component[] CheckBorderProximity(Component[] comps, int componentsCount)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if (comps[i] == null)
            {

            }
            else if ((compRect[0] < (int)(borderProximityThreshold1 * img.GetWidth()))
                    || (compRect[1] < (int)(borderProximityThreshold1 * img.GetHeight()))
                    || ((compRect[0] + compRect[2]) > (int)((1 - borderProximityThreshold1) * img.GetWidth()))
                    || ((compRect[1] + compRect[3]) > (int)((1 - borderProximityThreshold1) * img.GetHeight())))
            {

            }
            else
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }

    public Component[] CheckHeightScore(Component[] comps, int componentsCount)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if ((compRect[3] < (int)(heightScoreThreshold1 * img.GetWidth())))
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }

    public Component[] CheckWidthScore(Component[] comps, int componentsCount)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if ((compRect[2] < (int)(heightScoreThreshold1 * img.GetWidth())))
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }

    public Component[] CheckBorderProximity2(Component[] comps, int componentsCount)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if ((compRect[0] < (int)(borderProximityThreshold2 * img.GetWidth()))
                || (compRect[1] < (int)(borderProximityThreshold2 * img.GetHeight()))
                || ((compRect[0] + compRect[2]) >= (int)((1 - borderProximityThreshold2) * img.GetWidth()))
                || ((compRect[1] + compRect[3]) >= (int)((1 - borderProximityThreshold2) * img.GetHeight())))
            {

                if ((compRect[2] > (int)(heightScoreThreshold2 * img.GetWidth()))
                    || (compRect[3] > (int)(heightScoreThreshold2 * img.GetHeight())))
                {
                }
                else
                {
                    newComps[count] = comps[i];
                    count++;
                }

            }
            else
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }

    public void RemoveProbableImages(ImageInt denoisedImage)
    {
        ImageInt dilatedImage = ImageUtility.Dilate(img, (int)(img.GetWidth() * dilationPercentage), 0);
        Segmentation segment = new Segmentation(dilatedImage);
        segment.Segment();
        Component[] heightFilteredComponents = CheckHeightScore(segment.GetComponents(), segment.GetComponentsCount());

        segment.SetComponentsArray(heightFilteredComponents);
        ImageInt heightfilteredImage = ImageUtility.AddComponentsOnNewImage(segment.GetComponents(),
            segment.GetComponentsCount(), img.GetWidth(), img.GetHeight());
        denoisedImage.LogicalAnd(heightfilteredImage);
    }

    public void FilterBorderWhitespace(RectangularBound<int> bound, int[] hp, int[] vp)
    {
        int horizantalWhitespaceThreshold = (int)(whitespaceThresholdFactor * hp.Length);
        int hfi = 0;
        while (hfi < hp.Length && hp[hfi] <= horizantalWhitespaceThreshold)
            hfi++;
        int hli = hp.Length - 1;
        while (hli >= 0 && hp[hli] <= horizantalWhitespaceThreshold)
            hli--;

        int verticalWhitespaceThreshold = (int)(whitespaceThresholdFactor * vp.Length);
        int vfi = 0;
        while (vfi < vp.Length && vp[vfi] <= verticalWhitespaceThreshold)
            vfi++;
        int vli = vp.Length - 1;
        while (vli >= 0 && vp[vli] <= verticalWhitespaceThreshold)
            vli--;

        if (vfi > vli || hfi > hli)
        {
            bound.Invalidate();
            return;
        }

        bound.MaxX = bound.MinX + vli;
        bound.MaxY = bound.MinY + hli;

        bound.MinX += vfi;
        bound.MinY += hfi;
    }

    public ImageInt RemoveBorderWhiteSpace()
    {
        RectangularBound<int> recBound = new RectangularBound<int>();
        recBound.MinX = 0;
        recBound.MinY = 0;
        recBound.MaxX = img.GetWidth();
        recBound.MaxY = img.GetHeight();
        ProjectionProfile pp = new ProjectionProfile(img);
        int[] hp = pp.GetHorizontalProfile();
        int[] vp = pp.GetVerticalProfile();
        FilterBorderWhitespace(recBound, hp, vp);
        ImageInt croppedImage = new ImageInt(recBound.MaxX - recBound.MinX + 1, recBound.MaxY - recBound.MinY + 1,
            ImageInt.TYPE.BIN);
        for (int i = 0; i < croppedImage.GetWidth(); i++)
        {
            for (int j = 0; j < croppedImage.GetHeight(); j++)
            {
                croppedImage.pixel[i,j] = img.pixel[i + recBound.MinX, j + recBound.MinY];
            }
        }
        return croppedImage;
    }

    public int[] GetAverageDimensions(ImageInt _image)
    {
        if (_image.GetType() != ImageInt.TYPE.BIN)
        {
            _image = Binarization.SimpleThreshold(_image, 128);
        }
        int[] widthHeight = new int[2];
        Segmentation segmentation = new Segmentation(_image);
        segmentation.Segment();
        Component[] comps = segmentation.GetComponents();
        int componentsCount = segmentation.GetComponentsCount();

        int widhtSum = 0;
        int heightSum = 0;
        for (int i = 0; i < componentsCount; i++)
        {
            widhtSum += comps[i].GetRect()[2];
            heightSum += comps[i].GetRect()[3];
        }
        widthHeight[0] = widhtSum / componentsCount;
        widthHeight[1] = heightSum / componentsCount;
        return widthHeight;
    }

    public Component[] CheckHeightScorePX(Component[] comps, int componentsCount, int heightScoreThresholdPX)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if ((compRect[3] < (heightScoreThresholdPX)))
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }

    public Component[] CheckWidthScorePX(Component[] comps, int componentsCount, int heightScoreThresholdPX)
    {
        Component[] newComps = new Component[componentsCount];
        int count = 0;

        for (int i = 0; i < componentsCount; i++)
        {
            int[] compRect = comps[i].GetRect();
            if ((compRect[2] < (heightScoreThresholdPX)))
            {
                newComps[count] = comps[i];
                count++;
            }
        }
        return newComps;
    }
}
