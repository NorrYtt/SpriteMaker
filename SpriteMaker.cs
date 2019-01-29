// SpriteMaker.cs
// tutorial from https://www.youtube.com/watch?v=cIIaKdlZ4Cw&list=PL5KbKbJ6Gf9-1VAsllNBn175nF4fqnBCF&index=1
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteMaker : MonoBehaviour
{
  public Texture2D[] textureArray; // source image

  public Color[] colorArray; // colors from the layers

  public Sprite Make() // Use this for making sprite
  {
    return CreateSprite(CreateTexture(textureArray, colorArray));
  }

  public Sprite Make (Texture2D texture, Color color) // Use this for making a simple sprite with one color
  { // makes a single sprite from a single texture and single color
    return CreateSprite(CreateTexture(texture, color));
  }

  public Texture2D CreateTexture(Texture2D texture, Color color) // single apply
  {
    // creates a texture2D
    Texture2D myTex = new Texture2D(texture.width, texture.height);

    // loop through all pixels
    for (int x = 0; x < myTex.width; x++) // for each x
    {
      for (int y = 0; y < myTex.height; y++) // height
      {
        Color srcPixel = texture.GetPixel(x, y); // saves each source pixel
                                                
        // Apply color if necessary
        if (srcPixel.a != 0) // if not black or invisible and there's a corresponding color for this texture
        {
          srcPixel = ApplyColorToPixel(srcPixel, color);
        }
        myTex.SetPixel(x, y, srcPixel); // save
      }
    } // end of loop through all pixels and layers
    myTex.Apply(); // don't forget to apply
    myTex.wrapMode = TextureWrapMode.Clamp; // won't repeat
    myTex.filterMode = FilterMode.Point; // pixely
    //myTex.filterMode = FilterMode.Bilinear; // smooth
    return myTex; //
  } // end of CreateTexture(Texture2D texture, Color color)


  public Texture2D CreateTexture(Texture2D[] layers, Color[] layerColors)
  {
    if (layers.Length == 0)
    {
      Debug.LogError("SpriteMaker:CreatureTexture() - No image layer info in texture array.");
      return Texture2D.whiteTexture; // return something basic so we don't crash
    }

    // creates a texture2D
    Texture2D myTex = new Texture2D(layers[0].width, layers[0].height);

    // array to store the destination texture's pixels
    Color[] colorArray = new Color[myTex.width * myTex.height];

    // array of colors derived from the source texture
    Color[][] adjustedLayers = new Color[layers.Length][];

    // populate source array with cropped or expanded layer arrays
    for (int i = 0; i < layers.Length; i++)
    { // Check if there's just one layer or a perfectly sized one, we don't have to grow or shrink
      if ((i == 0) || (layers[i].width == myTex.width && layers[i].height == myTex.height))
      { // simply grab all the pixels
        adjustedLayers[i] = layers[i].GetPixels(); // copies each layer into srcarray
      }
      else // need to grow or shrink accordingly
      { // create a clear texture

        // horizontal grow or shrink
        int getX, getWidth, setX, setWidth;

        // conditional operator to figure out the correct X - inset or not
        getX = (layers[i].width > myTex.width) ? (layers[i].width - myTex.width) / 2 : 0;
        getWidth = (layers[i].width > myTex.width) ? myTex.width : layers[i].width;
        setX = (layers[i].width < myTex.width) ? (myTex.width - layers[i].width) / 2 : 0;
        setWidth = (layers[i].width < myTex.width) ? layers[i].width : myTex.width;

        // vertical grow or shrink
        int getY, getHeight, setY, setHeight;

        // conditional operator to figure out the correct Y - inset or not
        getY = (layers[i].height > myTex.height) ? (layers[i].height - myTex.height) / 2 : 0;
        getHeight = (layers[i].height > myTex.height) ? myTex.height : layers[i].height;
        setY = (layers[i].height < myTex.height) ? (myTex.height - layers[i].height) / 2 : 0;
        setHeight = (layers[i].height < myTex.height) ? layers[i].height : myTex.height;


        Color[] getPixels = layers[i].GetPixels(getX, getY, getWidth, getHeight);
        if (layers[i].width >= myTex.width && layers[i].height >= myTex.height)
        { // this is a crop so don't need to include blank pixels around
          adjustedLayers[i] = getPixels; // 
        }
        else // need the transparency pixels around the border
        {
          Texture2D sizedLayer = ClearTexture(myTex.width, myTex.height);
          sizedLayer.SetPixels(setX, setY, setWidth, setHeight, getPixels);
          adjustedLayers[i] = sizedLayer.GetPixels(); // can get entire because its sized correctly
        }

        adjustedLayers[i] = layers[i].GetPixels(); // copies each layer into srcarray
      } // end of must find horizontal and vertical grow/shrink/crop
    } // end of else need to grow or shrink accordingly

    // loop through all pixels and layers
    for (int x = 0; x < layers.Length; x++)
    {
      for (int y = 0; y < myTex.width * myTex.height; y++) // for each layer
      {
        Color srcPixel = adjustedLayers[x][y]; // saves each source pixel

        // Apply layer color if necessary
        if (srcPixel.r != 0 && srcPixel.a != 0 && layerColors.Length > x) // if not black or invisible and there's a corresponding color for this texture
        {
          srcPixel = ApplyColorToPixel(srcPixel, layerColors[x]);
        }
        // Normal blending based on color
        if (srcPixel.a == 1) colorArray[y] = srcPixel; // no alpha (transparancy) so overwrite fully 
        else if (srcPixel.a < 1) colorArray[y] = NormalBlend(colorArray[y], srcPixel); // blend 
      }
    } // end of loop through all pixels and layers
    myTex.SetPixels(colorArray);
    myTex.Apply(); // don't forget to apply
    myTex.wrapMode = TextureWrapMode.Clamp; // won't repeat
    //tex.filterMode = FilterMode.Point; // pixely
    myTex.filterMode = FilterMode.Bilinear; // smooth
    return myTex; //
  } // end of CreateTexture

  public Sprite CreateSprite(Texture2D texture)
  {
    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
  } // end of CreateSprite

  Color NormalBlend(Color dest, Color src)
  {
    float srcAlpha = src.a;
    float destAlpha = (1 - srcAlpha) * dest.a; // figure out the final alpha value
    Color destLayer = dest * destAlpha;
    Color srcLayer = src * srcAlpha;
    return destLayer + srcLayer; // return blended
  } // end of NormalBlend

  Color ApplyColorToPixel(Color pixel, Color applyColor)
  {
    if (pixel.r == 1f) // white, always return
      return applyColor;
    // assuming grayscale
    return pixel * applyColor; //
  }
  Texture2D ClearTexture(int width, int height) // returns a clear texture of width and height
  {
    Texture2D clearTexture = new Texture2D(width, height);
    Color[] clearPixels = new Color[width * height]; // create clear pixel array of correct length
    clearTexture.SetPixels(clearPixels);
    return clearTexture; // return the clear texturess
  }
} // end of SpriteMaker class
