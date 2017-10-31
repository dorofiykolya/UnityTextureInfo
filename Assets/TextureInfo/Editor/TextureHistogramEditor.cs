using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dorofiy.TextureInfo
{
  public class TextureHistogramEditor : EditorWindow
  {
    [MenuItem("Assets/Texture/Histogram")]
    private static void ShowHistogram()
    {
      if (ShowHistogramValidation())
      {
        var texture2d = Selection.activeObject as Texture2D;
        var path = AssetDatabase.GetAssetPath(texture2d);

        GetWindow<TextureHistogramEditor>("Histogram").SetData(path, texture2d).Show(true);
      }
    }

    [MenuItem("Assets/Texture/Histogram", true)]
    private static bool ShowHistogramValidation()
    {
      var texture2d = Selection.activeObject as Texture2D;
      return texture2d != null;
    }


    private TextureHistogramEditor SetData(string path, Texture2D texture2d)
    {
      Texture2D tex = new Texture2D(texture2d.width, texture2d.height);
      tex.LoadImage(File.ReadAllBytes(path));

      _pixels = tex.GetPixels32();
      _histogramData = CalculateHistogram(_pixels, 256);
      _histogramTexture = null;

      DestroyImmediate(tex, true);

      //minSize = new Vector2(263, 300);
      //maxSize = new Vector2(263, 300);

      return this;
    }

    private Texture2D[] _histogramTexture;
    private int _log10;
    private int _selection;
    private HistogramRawData _histogramData;
    private Color32[] _pixels;
    private string[] _buttons = new string[] { "rgb", "r", "g", "b", "a" };
    private string[] _algorithmButtons = new string[] { "linear", "log10" };

    private void OnGUI()
    {
      var newLog10 = GUILayout.SelectionGrid(_log10, _algorithmButtons, 2, EditorStyles.miniButtonMid);
      if (newLog10 != _log10)
      {
        _log10 = newLog10;
        _histogramData = CalculateHistogram(_pixels, 256);
        _histogramTexture = null;
      }
      _selection = GUILayout.SelectionGrid(_selection, _buttons, 5, EditorStyles.miniButtonMid);
      var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

      if ((_histogramTexture == null || _histogramTexture[_selection] == null) && _histogramData != null)
      {
        if (_histogramTexture == null) _histogramTexture = new Texture2D[5];
        _histogramTexture[_selection] = GenerateTexture(256, 256, _histogramData, _selection == 0 || _selection == 1, _selection == 0 || _selection == 2, _selection == 0 || _selection == 3, _selection == 4);
      }
      else if (_histogramTexture == null && _histogramData == null)
      {
        GUILayout.Label("select texture");
      }
      else if (_histogramTexture != null && _histogramTexture[_selection] != null)
      {
        EditorGUI.DrawTextureTransparent(rect, _histogramTexture[_selection]);
      }
    }

    private HistogramRawData CalculateHistogram(Color32[] pixels, int height)
    {
      var length = pixels.Length;
      var r = new long[256];
      var g = new long[256];
      var b = new long[256];
      var a = new long[256];
      foreach (var pixel in pixels)
      {
        r[pixel.r] = r[pixel.r] + 1;
        g[pixel.g] = g[pixel.g] + 1;
        b[pixel.b] = b[pixel.b] + 1;
        a[pixel.a] = a[pixel.a] + 1;
      }

      var rTotal = 0.0;
      var gTotal = 0.0;
      var bTotal = 0.0;
      var aTotal = 0.0;
      for (var i = 0; i < 256; i++)
      {
        rTotal = Math.Max(rTotal, r[i]);
        gTotal = Math.Max(gTotal, g[i]);
        bTotal = Math.Max(bTotal, b[i]);
        aTotal = Math.Max(aTotal, a[i]);
      }

      if (_log10 == 1)
      {
        rTotal = Math.Log10(rTotal);
        gTotal = Math.Log10(gTotal);
        bTotal = Math.Log10(bTotal);
        aTotal = Math.Log10(aTotal);
      }

      var rawData = new HistogramRawData();
      for (var i = 0; i < 256; i++)
      {
        var rd = _log10 == 1 ? Math.Log10(r[i]) : r[i];
        var gd = _log10 == 1 ? Math.Log10(g[i]) : g[i];
        var bd = _log10 == 1 ? Math.Log10(b[i]) : b[i];
        var ad = _log10 == 1 ? Math.Log10(a[i]) : a[i];

        rawData.R[i] = rd / rTotal;
        rawData.G[i] = gd / gTotal;
        rawData.B[i] = bd / bTotal;
        rawData.A[i] = ad / aTotal;
      }

      return rawData;
    }

    private Texture2D GenerateTexture(int width, int height, HistogramRawData rawData, bool r, bool g, bool b, bool a)
    {
      var texture = new Texture2D(width, height);

      for (var i = 0; i < rawData.R.Length; i++)
      {
        for (var y = 0; y < height; y++)
        {
          texture.SetPixel(i, y, new Color(.1f, .1f, .1f, 1));
        }

        if (r)
        {
          var size = (int)(rawData.R[i] * height);
          for (var y = 0; y < size; y++)
          {
            var pixel = texture.GetPixel(i, y);
            pixel.r = 1;
            pixel.a = 1;
            texture.SetPixel(i, y, pixel);
          }
        }
        if (g)
        {
          var size = (int)(rawData.G[i] * height);
          for (var y = 0; y < size; y++)
          {
            var pixel = texture.GetPixel(i, y);
            pixel.g = 1;
            pixel.a = 1;
            texture.SetPixel(i, y, pixel);
          }
        }
        if (b)
        {
          var size = (int)(rawData.B[i] * height);
          for (var y = 0; y < size; y++)
          {
            var pixel = texture.GetPixel(i, y);
            pixel.b = 1;
            pixel.a = 1;
            texture.SetPixel(i, y, pixel);
          }
        }
        if (a)
        {
          var size = (int)(rawData.A[i] * height);
          for (var y = 0; y < size; y++)
          {
            var pixel = texture.GetPixel(i, y);
            pixel.r = pixel.a;
            pixel.g = pixel.a;
            pixel.b = pixel.a;
            pixel.a = 1;
            texture.SetPixel(i, y, pixel);
          }
        }
      }
      texture.Apply();
      return texture;
    }

    private class HistogramRawData
    {
      public double[] R = new double[256];
      public double[] G = new double[256];
      public double[] B = new double[256];
      public double[] A = new double[256];
    }
  }
}