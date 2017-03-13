using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlockInfo
{
    public BlockInfo()
    {
        Row = -1;
        Column = -1;
        blockColor = new SerializableColor(ColorBase.defaultColor);
    }
    public BlockInfo(int _row, int _column, Color _color)
    {
        Row = _row;
        Column = _column;
        BlockColor = new SerializableColor(_color);
    }
    private int column;
    public int Column
    {
        get
        {
            return column;
        }

        set
        {
            column = value;
        }
    }

    private int row;
    public int Row
    {
        get
        {
            return row;
        }

        set
        {
            row = value;
        }
    }
    private SerializableColor blockColor;
    public SerializableColor BlockColor
    {
        get
        {
            return blockColor;
        }

        set
        {
            blockColor = value;
        }
    }

    private bool isChecked; // flag to control adjacent blogs
    public bool IsChecked
    {
        get
        {
            return isChecked;
        }

        set
        {
            isChecked = value;
        }
    }

    private bool isDeadEnd; // flag to control empty blogs
    public bool IsDeadEnd
    {
        get
        {
            return isDeadEnd;
        }

        set
        {
            isDeadEnd = value;
        }
    }
    public void Clear()
    {
        isDeadEnd = false;
        isChecked = false;

    }


}