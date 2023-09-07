using System;
using System.Collections.Generic;

public class TopDownSegmenter
{
    private float whitespaceThresholdFactor = 0.002f; // 1% of the height or width
    private int segmentationThresholdWhitespaceLength; // segment up to 20 whitespaces of layout
    private ImageInt image;
    private ProjectionProfile pp;
    private BinaryTree<RectangularBound<int>> layoutTree;
    private List<RectangularBound<int>> segments;

    public TopDownSegmenter(ImageInt image)
    {
        this.image = image;
        segmentationThresholdWhitespaceLength = Math.Min(30, (int)(image.GetWidth() * 0.01));
        pp = new ProjectionProfile(this.image);
    }

    public List<RectangularBound<int>> GetSegments()
    {
        return segments;
    }

    public void Segment()
    {
        RectangularBound<int> rootLayout = new RectangularBound<int>
        {
            MinX = 0,
            MaxX = image.GetMaxX(),
            MinY = 0,
            MaxY = image.GetMaxY()
        };
        Segment(rootLayout);
        segments = new List<RectangularBound<int>>();
        FindSegments();
    }

    public void Segment(RectangularBound<int> rootLayout)
    {
        layoutTree = new BinaryTree<RectangularBound<int>>(rootLayout);
        Segment(layoutTree.GetRoot());
    }

    private void Segment(BinaryTree<RectangularBound<int>>.Node<RectangularBound<int>> node)
    {
        RectangularBound<int> bound = node.Item;
        int[] hp = pp.GetHorizontalProfile(bound.MinX, bound.MaxX, bound.MinY, bound.MaxY);
        int[] vp = pp.GetVerticalProfile(bound.MinX, bound.MaxX, bound.MinY, bound.MaxY);

        if (hp == null || vp == null)
        {
            bound.Invalidate();
            return;
        }

        FilterBorderWhitespace(bound, hp, vp);

        hp = pp.GetHorizontalProfile(bound.MinX, bound.MaxX, bound.MinY, bound.MaxY);
        vp = pp.GetVerticalProfile(bound.MinX, bound.MaxX, bound.MinY, bound.MaxY);

        if (hp == null || vp == null)
        {
            bound.Invalidate();
            return;
        }

        var horizantalMaxWhitespace = new Range<int>(0, 0, 0);
        var horizantalCurWhitespace = new Range<int>(0, 0, 0);

        int horizantalWhitespaceThreshold = (int)(whitespaceThresholdFactor * hp.Length);

        for (int i = 0; i < hp.Length;)
        {
            while (i < hp.Length && hp[i] > horizantalWhitespaceThreshold)
                i++;

            horizantalCurWhitespace.Start = bound.MinY + i;

            while (i < hp.Length && hp[i] <= horizantalWhitespaceThreshold)
                i++;

            horizantalCurWhitespace.End = bound.MinY + i;
            horizantalCurWhitespace.Value = horizantalCurWhitespace.End - horizantalCurWhitespace.Start + 1;

            if (horizantalCurWhitespace.Value > horizantalMaxWhitespace.Value)
            {
                horizantalMaxWhitespace.SetTo(horizantalCurWhitespace);
            }
        }

        var verticalMaxWhitespace = new Range<int>(0, 0, 0);
        var verticalCurWhitespace = new Range<int>(0, 0, 0);

        int verticalWhitespaceThreshold = (int)(whitespaceThresholdFactor * vp.Length);

        for (int i = 0; i < vp.Length;)
        {
            while (i < vp.Length && vp[i] > verticalWhitespaceThreshold)
                i++;

            verticalCurWhitespace.Start = bound.MinX + i;

            while (i < vp.Length && vp[i] <= verticalWhitespaceThreshold)
                i++;

            verticalCurWhitespace.End = bound.MinX + i;
            verticalCurWhitespace.Value = verticalCurWhitespace.End - verticalCurWhitespace.Start + 1;

            if (verticalCurWhitespace.Value > verticalMaxWhitespace.Value)
            {
                verticalMaxWhitespace.SetTo(verticalCurWhitespace);
            }
        }

        if (verticalMaxWhitespace.Value < segmentationThresholdWhitespaceLength &&
            horizantalMaxWhitespace.Value < segmentationThresholdWhitespaceLength)
            return;

        var firstBound = new RectangularBound<int>();
        var secondBound = new RectangularBound<int>();

        if (horizantalMaxWhitespace.Value > verticalMaxWhitespace.Value)
        {
            firstBound.MinX = bound.MinX;
            firstBound.MaxX = bound.MaxX;
            firstBound.MinY = bound.MinY;
            firstBound.MaxY = horizantalMaxWhitespace.Start;

            secondBound.MinX = bound.MinX;
            secondBound.MaxX = bound.MaxX;
            secondBound.MinY = horizantalMaxWhitespace.End;
            secondBound.MaxY = bound.MaxY;
        }
        else
        {
            firstBound.MinX = bound.MinX;
            firstBound.MaxX = verticalMaxWhitespace.Start;
            firstBound.MinY = bound.MinY;
            firstBound.MaxY = bound.MaxY;

            secondBound.MinX = verticalMaxWhitespace.End;
            secondBound.MaxX = bound.MaxX;
            secondBound.MinY = bound.MinY;
            secondBound.MaxY = bound.MaxY;
        }

        node.InsertToLeft(firstBound);
        node.InsertToRight(secondBound);

        Segment(node.GetLeftChild());
        Segment(node.GetRightChild());
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

    private void FindSegments()
    {
        segments.Clear();
        AddSegmentsAtLeaf(layoutTree.GetRoot());
    }

    private void AddSegmentsAtLeaf(BinaryTree<RectangularBound<int>>.Node<RectangularBound<int>> node)
    {
        if (node.IsLeaf())
        {
            if (node.Item != null && node.Item.IsValid())
                segments.Add(node.Item);
            return;
        }

        AddSegmentsAtLeaf(node.GetLeftChild());
        AddSegmentsAtLeaf(node.GetRightChild());
    }
}
