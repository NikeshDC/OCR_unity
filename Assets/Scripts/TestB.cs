using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestB : MonoBehaviour
{
    public Texture2D texture;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Texture Width: " + texture.width + ", "+ texture.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
