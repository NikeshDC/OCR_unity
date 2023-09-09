using System;

public class Segmentation
{
    private int MAX_COMP = 50000;
    private int IMG_X = 10;
    private int IMG_Y = 10;

    private ImageInt image; // component labeled image
    private ImageInt binaryImage; // input binary image
    private Component[] listedComp;
    private Component[] components;

    // values in components are correct
    private bool COMPONENTS_SET;
    private bool COMPONENT_IMAGE_SET;
    // Count of components after merging siblings
    private int componentsCount;

    // component index i.e. label count
    private int componentTotalCount;

    private int[] componentSequence;
    private int[] componentRoot;
    private int[] componentTrailer;

    public Segmentation(ImageInt binImage)
    {
        if (binImage.GetType() != ImageInt.TYPE.BIN)
        {
            Console.WriteLine("Cannot apply segmentation for unbinarized image.");
            binImage = null;
            return;
        }
        binaryImage = binImage;
        image = new ImageInt(binImage);
        IMG_X = binImage.GetWidth();
        IMG_Y = binImage.GetHeight();

        MAX_COMP = IMG_X * IMG_Y / 2 + 3;
        listedComp = new Component[MAX_COMP];
        componentSequence = new int[MAX_COMP];
        componentRoot = new int[MAX_COMP];
        componentTrailer = new int[MAX_COMP];
        COMPONENTS_SET = false;
        COMPONENT_IMAGE_SET = false;
    }

    public void PrintImg()
    {
        for (int j = 0; j < IMG_Y; j++)
        {
            for (int i = 0; i < IMG_X; i++)
            {
                Console.Write(image.pixel[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public void LabelComponents()
    {
        int componentIndex = 2; // initial component index (0 and 1 already used for labeling binarized image)
        image.SetType(ImageInt.TYPE.COMP);
        for (int j = 0; j < IMG_Y; j++)
        {
            for (int i = 0; i < IMG_X; i++)
            {
                if (image.pixel[i, j] == 1)
                {
                    if (((j > 0 && image.pixel[i, j - 1] != 0)))
                    {
                        image.pixel[i, j] = image.pixel[i, j - 1];
                        if (((i > 0 && image.pixel[i - 1, j] != 0)) && image.pixel[i, j] != image.pixel[i - 1, j])
                        {
                            int rootX = componentRoot[image.pixel[i, j]];
                            int rootY = componentRoot[image.pixel[i - 1, j]];
                            if (rootX == rootY)
                            {
                                continue;
                            }
                            componentSequence[rootY] = componentTrailer[rootX];
                            componentTrailer[rootX] = componentTrailer[rootY];

                            int temp1 = componentTrailer[rootY];

                            while (temp1 != componentSequence[rootY])
                            {
                                componentRoot[temp1] = rootX;
                                temp1 = componentSequence[temp1];
                            }
                        }
                    }
                    else if (((i > 0 && image.pixel[i - 1, j] != 0)))
                    {
                        image.pixel[i, j] = image.pixel[i - 1, j];
                    }
                    else
                    {
                        image.pixel[i, j] = componentIndex;
                        componentIndex++;
                        componentSequence[image.pixel[i, j]] = image.pixel[i, j];
                        componentRoot[image.pixel[i, j]] = image.pixel[i, j];
                        componentTrailer[image.pixel[i, j]] = image.pixel[i, j];
                    }
                }
            }
        }
        componentTotalCount = componentIndex;
    }

    public void PrepareComponentList()
    {
        for (int i = 0; i < IMG_X; i++)
        {
            for (int j = 0; j < IMG_Y; j++)
            {
                if (image.pixel[i, j] != 0)
                {
                    if (listedComp[image.pixel[i, j]] == null)
                    {
                        listedComp[image.pixel[i, j]] = new Component();
                        listedComp[image.pixel[i, j]].SetValues(i, j);
                        listedComp[image.pixel[i, j]].IncreaseCountOfBlackPixels();
                    }
                    else
                    {
                        listedComp[image.pixel[i, j]].SetValues(i, j);
                        listedComp[image.pixel[i, j]].IncreaseCountOfBlackPixels();
                    }
                }
            }
        }
    }

    public void MergeSiblings()
    {
        componentsCount = componentTotalCount - 2;
        for (int i = 2; i < componentTotalCount; i++)
        {
            if (componentRoot[i] != i)
            {
                listedComp[componentRoot[i]].MergeComp(listedComp[i]);
                SetPixelVal(image, listedComp[i], componentRoot[i]);
                listedComp[i] = null;
                componentsCount--;
            }
        }
    }

    private void SetPixelVal(ImageInt img, Component c, int val)
    {
        for (int i = c.GetMinX(); i < c.GetMaxX() + 1; i++)
        {
            for (int j = c.GetMinY(); j < c.GetMaxY() + 1; j++)
            {
                if (img.pixel[i, j] != 0)
                {
                    img.pixel[i, j] = val;
                }
            }
        }
    }

    public Component[] GetComponents()
    {
        if (COMPONENTS_SET)
        {
            return components;
        }
        else
        {
            StoreComponents();
        }
        return components;
    }

    public void StoreComponents()
    {
        components = new Component[componentsCount];

        for (int i = 0, j = 0; i < componentTotalCount; i++)
        {
            if (listedComp[i] != null)
            {
                components[j] = listedComp[i];
                j++;
            }
        }
        COMPONENTS_SET = true;
    }

    public void SetComponentsImage()
    {
        for (int i = 0; i < componentTotalCount; i++)
        {
            if (listedComp[i] != null)
            {
                listedComp[i].SetImage(binaryImage);
            }
        }
        COMPONENT_IMAGE_SET = true;
    }

    public void Segment()
    {
        LabelComponents();
        PrepareComponentList();
        MergeSiblings();
        SetComponentsImage();
    }

    public void GetRectangles()
    {
        int count = 0;
        for (int i = 0; i < components.Length - 1; i++)
        {
            if (components[i] != null)
            {
                components[i].ShowValues(i);
                count++;
            }
        }
        Console.WriteLine("components = " + count);
    }

    public ImageInt GetSegmentedImage()
    {
        return image;
    }

    public void SetComponentsArray(Component[] comps)
    {
        components = comps;
        int count = 0;
        for (int i = 0; i < componentsCount; i++)
        {
            if (comps[i] != null)
            {
                count++;
            }
        }
        componentsCount = count;
    }

    public int GetComponentsCount()
    {
        return componentsCount;
    }
}
