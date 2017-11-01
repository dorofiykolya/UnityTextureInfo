using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Dorofiy.TextureInfo
{
  public class TextureChannelEditor : EditorWindow
  {
    [MenuItem("Assets/Texture/Channel")]
    private static void ShowChannel()
    {
      if (ShowChannelValidate())
      {
        var texture2d = Selection.activeObject as Texture2D;
        var path = AssetDatabase.GetAssetPath(texture2d);

        GetWindow<TextureChannelEditor>("Channel").SetData(path, texture2d).Show(true);
      }
    }

    [MenuItem("Assets/Texture/Channel", true)]
    private static bool ShowChannelValidate()
    {
      var texture2d = Selection.activeObject as Texture2D;
      return texture2d != null;
    }

    private TextureChannelEditor SetData(string path, Texture2D texture2d)
    {
      TextureImporter textureImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture2d)) as TextureImporter;
      var lastReadable = textureImporter.isReadable;
      textureImporter.isReadable = true;
      textureImporter.SaveAndReimport();
      _texture2d = new Texture2D(texture2d.width, texture2d.height);
      _texture2d.SetPixels32(texture2d.GetPixels32());
      textureImporter.isReadable = lastReadable;
      textureImporter.SaveAndReimport();
      if (_renderTexture != null)
      {
        DestroyImmediate(_renderTexture, true);
      }
      _renderTexture = null;
      return this;
    }

    private int _selection;
    private Texture2D _texture2d;
    private string[] _buttons = new string[] { "rgb", "r", "g", "b", "a" };
    private Vector2 _scrollPosition;
    private Texture2D _renderTexture;

    private void OnGUI()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.EndHorizontal();
      var newSelection = GUILayout.SelectionGrid(_selection, _buttons, 5, EditorStyles.miniButtonMid);
      if (newSelection != _selection)
      {
        _selection = newSelection;
        if (_renderTexture != null)
        {
          DestroyImmediate(_renderTexture, true);
        }
        _renderTexture = null;
      }

      if (_renderTexture == null)
      {
        _renderTexture = new Texture2D(_texture2d.width, _texture2d.height);
        for (var x = 0; x < _texture2d.width; x++)
        {
          for (var y = 0; y < _texture2d.height; y++)
          {
            var pixel = _texture2d.GetPixel(x, y);
            if (_selection == 0)
            {
              _renderTexture.SetPixel(x, y, pixel);
            }
            else if (_selection == 1)
            {
              pixel.b = pixel.r;
              pixel.g = pixel.r;
              pixel.a = 1;
            }
            else if (_selection == 2)
            {
              pixel.r = pixel.g;
              pixel.b = pixel.g;
              pixel.a = 1;
            }
            else if (_selection == 3)
            {
              pixel.r = pixel.b;
              pixel.g = pixel.b;
              pixel.a = 1;
            }
            else if (_selection == 4)
            {
              pixel.r = pixel.a;
              pixel.g = pixel.a;
              pixel.b = pixel.a;
              pixel.a = 1;
            }
            _renderTexture.SetPixel(x, y, pixel);
          }
        }
        _renderTexture.Apply();
      }

      _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

      var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
      EditorGUI.DrawTextureTransparent(rect, _renderTexture, ScaleMode.ScaleToFit);

      EditorGUILayout.EndScrollView();
    }
  }
}