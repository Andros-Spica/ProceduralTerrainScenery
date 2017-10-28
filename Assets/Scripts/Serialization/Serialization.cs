using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using VectorExtension;

public static class Serialization
{
    public static string saveFolderName = "data";

    public static bool LoadArray(string fileName, int saveSlot, out float[] array)
    {
        string saveFile = GetSaveFileNameAndDirectory(fileName, saveSlot);

        array = new float[0];

        if (!File.Exists(saveFile))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);
        float[] newArray = (float[])formatter.Deserialize(stream);
        array = newArray;

        stream.Close();
        return true;
    }

    public static bool SaveArray(string fileName, float[] array, int saveSlot)
    {
        string saveFile = GetSaveFileNameAndDirectory(fileName, saveSlot);

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, array);

        stream.Close();
        return true;
    } 

    #region class data

    public static bool LoadClassData(string fileName, int saveSlot, out System.Object serializedObject)
    {
        string saveFile = GetSaveFileNameAndDirectory(fileName, saveSlot);
        
        serializedObject = new System.Object();

        if (!File.Exists(saveFile))
            return false;
        
        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);
        serializedObject = formatter.Deserialize(stream);

        stream.Close();
        return true;
    }

    public static bool SaveClassData(IPersistence objectToSave, int saveSlot)
    {
        string saveFile = GetSaveFileNameAndDirectory(objectToSave.fileName, saveSlot);
        
        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, objectToSave);
        
        stream.Close();
        return true;
    }

    private static string GetSaveFileNameAndDirectory(string fileName, int saveSlot)
    {
        string[] saveFolders = { "saves", "slot_" + saveSlot.ToString() };
        if (fileName.Contains("year"))
            saveFolders = new string[] { saveFolders[0], saveFolders[1], "trajectory"}; // tick data are saved in the trajectory folder
        string saveFile = SaveLocation(saveFolders);
        saveFile += fileName;
        return saveFile;
    }

    #endregion

    #region raster

    public static bool LoadRaster2D(string fileName, Rect sector, out float[,] output)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += fileName;
        int sectorWidth = Mathf.FloorToInt(sector.width);
        int sectorHeight = Mathf.FloorToInt(sector.height);

        output = new float[sectorWidth, sectorHeight];

        if (!File.Exists(saveFile))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        float[,] raster = (float[,])formatter.Deserialize(stream);
        
        if (raster.GetLength(0) < sectorWidth || raster.GetLength(1) < sectorHeight)
        {
            Debug.Log("Error in Serialization: sector is too large or raster file too small.");
            return false;
        }

        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                output[y, x] = raster[Mathf.FloorToInt(sector.y) + y, Mathf.FloorToInt(sector.x) + x];
            }
        }
        stream.Close();
        return true;
    }

    public static bool LoadRaster3D(string fileName, Rect sector, int var3D, out float[,,] output)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += fileName;
        int sectorWidth = Mathf.FloorToInt(sector.width);
        int sectorHeight = Mathf.FloorToInt(sector.height);

        output = new float[sectorWidth, sectorHeight, var3D];

        if (!File.Exists(saveFile))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        float[,,] raster = (float[,,])formatter.Deserialize(stream);
        
        if (raster.GetLength(0) < sectorWidth || raster.GetLength(1) < sectorHeight)
        {
            Debug.LogError("Sector is too large or raster file too small.");
            return false;
        }
        if (raster.GetLength(2) != var3D)
        {
            Debug.LogError("Raster in file has " + raster.GetLength(2) + " variables while " + var3D + " are requested.");
        }
        
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                for (int i = 0; i < var3D; i++)
                {
                    output[y, x, i] = raster[Mathf.FloorToInt(sector.y) + y, Mathf.FloorToInt(sector.x) + x, i];
                } 
            }
        }
        stream.Close();
        return true;
    }

    public static void SaveRaster2D(string fileName, float[,] raster)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += fileName;

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, raster);
        stream.Close();
    }

    public static void SaveRaster3D(string fileName, float[,,] raster)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += fileName;

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, raster);
        stream.Close();
    }

    public static void SaveAlphamap(float[,,] alphamap)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += "alphamap";

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, alphamap);
        stream.Close();
    }

    public static void SaveHeightmap(float[,] heightmap)
    {
        string saveFile = SaveLocation("terrain");
        saveFile += "heightmap";

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, heightmap);
        stream.Close();
    }

    #endregion
    
    public static string SaveLocation(string folderName)
    {
        string[] folderNames = { folderName };
        return SaveLocation(folderNames);
    }
    public static string SaveLocation(string[] folderNames)
    {
        string saveLocation = saveFolderName + "/";
        
        foreach (string folderName in folderNames)
        {
            saveLocation += folderName + "/";
            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }
        }
        
        return saveLocation;
    }
}
