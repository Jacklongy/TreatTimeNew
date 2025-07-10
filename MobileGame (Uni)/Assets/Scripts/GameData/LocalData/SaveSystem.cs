using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem
{
   public static void Save(GameOverseer overseer)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/Player.Data";

        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(overseer);

        Debug.Log("Save file created at: " + path);


        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadData()
    {
        string path = Application.persistentDataPath + "/Player.Data";

        if (File.Exists(path))
        {

            BinaryFormatter formatter = new BinaryFormatter(); 
            FileStream fileStream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(fileStream) as PlayerData;
            fileStream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found at path: " + path);
            Debug.LogError("Persistent Data Path: " + Application.persistentDataPath);
            return null;
         
          
        }
    }

    
}
