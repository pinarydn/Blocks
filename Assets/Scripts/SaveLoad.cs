﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class SaveLoad
{
    private static string savePath = Application.persistentDataPath + "/gamesave.data";
    public static void SaveGame(BlockInfo[,] gridBlockInfos, List<BlockInfo> currentBlockInfos, GameVariables gameVariables, List<BlockInfo> hintBlocks)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate);
        bf.Serialize(fs, gridBlockInfos);
        bf.Serialize(fs, currentBlockInfos);
        bf.Serialize(fs, gameVariables);
        bf.Serialize(fs, hintBlocks);
        fs.Close();
    }
    public static object LoadGame(ref long position)
    {
        object obj = null;
        if (File.Exists(savePath))
        {
            FileStream fs = new FileStream(savePath, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            fs.Seek(position, SeekOrigin.Begin);
            obj = bf.Deserialize(fs);
            position = fs.Position;
            fs.Close();
        }
        return obj;
    }
    public static bool IsFileExists()
    {
        return File.Exists(savePath);
    }
}
