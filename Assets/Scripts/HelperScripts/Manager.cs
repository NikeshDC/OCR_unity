using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Manager : MonoBehaviour
{
    public Texture2D imageToOCR;
    public RawImage imageToSet;
    public Texture2D testImg;

    private int textureWidth;
    private int textureHeight;
    private Color[] texturePixels;

    private bool binarizationFinished = false;
    ImageInt ocrImage;

    private void Update()
    {

        if(binarizationFinished)
        {
            Texture2D bintexture = ImageUtility.GetTexture(ocrImage);
            imageToSet.texture = bintexture;
            bintexture.Apply();
            SaveImage(bintexture);
            binarizationFinished = false;
        }
    }

    private void SaveImage(Texture2D texture)
    {
        byte[] imageRaw = texture.EncodeToPNG();
        string filepath = Application.dataPath + Path.DirectorySeparatorChar + "new.png";
        File.WriteAllBytes(filepath, imageRaw);
    }

    public void Run()
    {
        Console.WriteLine("Hello");
        textureWidth = imageToOCR.width;
        textureHeight = imageToOCR.height;
        texturePixels = imageToOCR.GetPixels();
        Task.Run(() => { _run(); });
    }
    private void _run()
    {
        if (imageToOCR == null)
            return;

        ImageInt srcImage = ImageUtility.GetImage(texturePixels, textureWidth, textureHeight);
        BinarizeHelper binarizer = new BinarizeHelper();
        ocrImage = binarizer.Binarize(srcImage);
        binarizationFinished = true;
    }
}
