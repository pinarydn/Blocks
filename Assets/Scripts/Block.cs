﻿using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public BlockInfo info;
    private Image blockImg;
    public GameObject hammerImage;
    void Awake()
    {
        blockImg = GetComponent<Image>();
    }

    public void FillInfo(int _row, int _column, Color _color)
    {
        info = new BlockInfo(_row, _column, _color);
        blockImg.color = _color;
    }
    public void SetColor(Color clr)
    {
        info.BlockColor = new SerializableColor(clr);
        blockImg.color = clr;
    }
    public void Clear()
    {
        info.BlockColor = new SerializableColor(ColorBase.defaultColor);
        blockImg.color = ColorBase.defaultColor;
        info.IsChecked = false;
    }

}