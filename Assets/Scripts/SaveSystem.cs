using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    public static void SaveGame(string saveFileName, int fieldSize, int animalCount, int animalVelocity, GameObject[] animalsGO, GameObject[] foodGO)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string saveFilePath = Application.persistentDataPath + saveFileName;
        FileStream stream = new FileStream(saveFilePath, FileMode.Create);
        GameData data = new GameData(fieldSize, animalCount, animalVelocity, animalsGO, foodGO);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static GameData LoadGame(string saveFileName)
    {
        string saveFilePath = Application.persistentDataPath + saveFileName;
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(saveFilePath, FileMode.Open);
            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();
            return data;
        } else
        {
            Debug.Log("Save file " + saveFilePath + " doesn't exist");
            return null;
        }
    }

    public static bool isSaveFileExists(string saveFileName)
    {
        string saveFilePath = Application.persistentDataPath + saveFileName;
        return File.Exists(saveFilePath);
    }
}
