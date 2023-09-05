using System;

public class ProjectionProfile
{
    private ImageInt image;

    public ProjectionProfile(ImageInt image)
    {
        SetImage(image);
    }

    public void SetImage(ImageInt image)
    {
        if (image.GetType() != ImageInt.TYPE.BIN)
        {
            Console.WriteLine("ImageInt not in binarized form. <ProjectionProfile>");
        }
        this.image = image;
    }

    public int[] GetHorizontalProfile(int xs, int xe, int ys, int ye)
    {
        if (xs > image.GetMaxX() || xe > image.GetMaxX() || ys > image.GetMaxY() || ye > image.GetMaxY())
        {
            return null;
        }
        if (xs < 0 || xe < 0 || ys < 0 || ye < 0)
        {
            return null;
        }
        if (xs >= xe || ys >= ye)
        {
            return null;
        }

        int[] horizontalProfile = new int[ye - ys + 1];
        for (int i = 0; i < horizontalProfile.Length; i++)
        {
            int ycoord = i + ys;
            for (int xcoord = xs; xcoord <= xe; xcoord++)
            {
                horizontalProfile[i] += image.pixel[xcoord, ycoord];
            }
        }
        return horizontalProfile;
    }

    public int[] GetHorizontalProfile()
    {
        return GetHorizontalProfile(0, image.GetMaxX(), 0, image.GetMaxY());
    }

    public int[] GetVerticalProfile(int xs, int xe, int ys, int ye)
    {
        if (xs > image.GetMaxX() || xe > image.GetMaxX() || ys > image.GetMaxY() || ye > image.GetMaxY())
        {
            return null;
        }
        if (xs < 0 || xe < 0 || ys < 0 || ye < 0)
        {
            return null;
        }
        if (xs >= xe || ys >= ye)
        {
            return null;
        }

        int[] verticalProfile = new int[xe - xs + 1];
        for (int i = 0; i < verticalProfile.Length; i++)
        {
            int xcoord = i + xs;
            for (int ycoord = ys; ycoord <= ye; ycoord++)
            {
                verticalProfile[i] += image.pixel[xcoord, ycoord];
            }
        }
        return verticalProfile;
    }

    public int[] GetVerticalProfile()
    {
        return GetVerticalProfile(0, image.GetMaxX(), 0, image.GetMaxY());
    }
}
