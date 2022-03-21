using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public class SerializableData
{
	public SerializableData(string instanceIdentifier)
	{
		this.instanceIdentifier = instanceIdentifier;
	}

	public string instanceIdentifier { get; protected set; }
}

public static class SerializationManager
{

	static SerializationManager()
	{
		//Application.wantsToQuit;
	}

	public enum SerializationMode
	{
		Binary,
		JSON,
	}

	public delegate void SaveEvent();
	public static event SaveEvent WantsToSave;

	private static Dictionary<string, SerializableData> followedData = new Dictionary<string, SerializableData>();
	private const string SAVEFILE_EXTENSION = ".sav";
	private const string SAVESLOT_NAME = "Saveslot_";
	private static SerializationMode saveslotFileType = SerializationMode.JSON;


	public static void SetMassSavingFileType(SerializationMode fileType)
	{
		saveslotFileType = fileType;
	}

	#region Manually File Saving & Loading

	public static void SaveFile(SerializableData toSave, SerializationMode saveMode, string directory, string fileName, string fileExtension = SAVEFILE_EXTENSION)
	{
		CheckDirectory(directory, true);

		switch (saveMode)
		{
			case SerializationMode.Binary:
				SaveByBinary(new SerializableData[] { toSave }, Path.Combine(directory, fileName + fileExtension));
				break;
			case SerializationMode.JSON:
				SaveByJSON(new SerializableData[] { toSave }, Path.Combine(directory, fileName + fileExtension));
				break;
		}

		Debug.Log("File saved to " + directory);
	}

	public static void SaveFiles(SerializableData[] toSave, SerializationMode saveMode, string directory, string fileName, string fileExtension = SAVEFILE_EXTENSION)
	{
		CheckDirectory(directory, true);

		switch (saveMode)
		{
			case SerializationMode.Binary:
				SaveByBinary(toSave, Path.Combine(directory, fileName + fileExtension));
				break;
			case SerializationMode.JSON:
				SaveByJSON(toSave, Path.Combine(directory, fileName + fileExtension));
				break;
		}

		Debug.Log("Files saved to " + directory);
	}

	public static SerializableData LoadFile(SerializationMode loadMode, string directory, string fileName, string fileExtension = SAVEFILE_EXTENSION)
	{
		if (File.Exists(Path.Combine(directory, fileName + fileExtension)))
		{
			SerializableData toReturn = null;

			switch (loadMode)
			{
				case SerializationMode.Binary:
					toReturn = LoadByBinary(Path.Combine(directory, fileName + fileExtension))[0];
					break;
				case SerializationMode.JSON:
					toReturn = LoadByJSON(Path.Combine(directory, fileName + fileExtension))[0];
					break;
			}

			return toReturn;
		}
		return null;
	}

	public static SerializableData[] LoadFiles(SerializationMode loadMode, string directory, string fileName, string fileExtension = SAVEFILE_EXTENSION)
	{
		if (File.Exists(Path.Combine(directory, fileName + fileExtension)))
		{
			SerializableData[] toReturn = null;

			switch (loadMode)
			{
				case SerializationMode.Binary:
					toReturn = LoadByBinary(Path.Combine(directory, fileName + fileExtension));
					break;
				case SerializationMode.JSON:
					toReturn = LoadByJSON(Path.Combine(directory, fileName + fileExtension));
					break;
			}

			return toReturn;
		}
		return null;
	}

	#endregion

	#region slot saving

	public static void AddFileToSaveSlot(SerializableData toAdd)
	{
		if (followedData.ContainsKey(toAdd.instanceIdentifier))
		{
			followedData[toAdd.instanceIdentifier] = toAdd;
		}
		else
		{
			followedData.Add(toAdd.instanceIdentifier, toAdd);
		}
	}

	public static void RemoveFileFromSaveSlot(SerializableData toRemove)
	{
		if (followedData.ContainsKey(toRemove.instanceIdentifier))
		{
			followedData.Remove(toRemove.instanceIdentifier);
		}
	}

	public static bool LoadSaveSlot(int slotNumber)
	{
		string fileName = SAVESLOT_NAME + slotNumber;
		SerializableData[] toReturn = LoadFiles(saveslotFileType, Application.persistentDataPath, fileName);

		if (toReturn.Length > 0)
		{
			followedData = ConvertToDictionary(toReturn);
			return true;
		}
		else
		{
			return false;
		}

	}

	public static void SaveToSaveSlot(int slotNumber)
	{
		string fileName = SAVESLOT_NAME + slotNumber;
		WantsToSave?.Invoke();
		SerializableData[] data = followedData.Values.ToArray();
		SaveFiles(data, saveslotFileType, Application.persistentDataPath, fileName);
	}

	public static T GetDataByInstanceIdentifier<T>(string instanceIdentifier) where T : SerializableData
	{
		if (followedData.ContainsKey(instanceIdentifier))
		{
			return (T)followedData[instanceIdentifier];
		}
		else
		{
			return null;
		}
	}

	public static string[] GetAllSaveSlotFileNames()
	{
		return GetFileNamesInFolder(Application.persistentDataPath);
	}

	#endregion

	#region Private functions

	private static void SaveByBinary(SerializableData[] toSave, string path)
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fileStream = File.Create(path);

		bf.Serialize(fileStream, toSave);
		fileStream.Close();
	}

	private static void SaveByJSON(SerializableData[] toSave, string path)
	{
		JsonSerializerSettings settings = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.All,
		};

		string json = JsonConvert.SerializeObject(toSave, Formatting.Indented, settings);
		StreamWriter sw = new StreamWriter(path, false);
		sw.WriteLine(json);
		sw.Close();
		sw.Dispose();
	}

	private static SerializableData[] LoadByBinary(string path)
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fileStream = File.Open(path, FileMode.Open);

		SerializableData[] toReturn = (SerializableData[])bf.Deserialize(fileStream);
		fileStream.Close();
		fileStream.Dispose();

		return toReturn;
	}


	private static SerializableData[] LoadByJSON(string path)
	{
		JsonSerializerSettings settings = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.All,
		};

		StreamReader sr = new StreamReader(path);
		string json = sr.ReadToEnd();
		SerializableData[] toReturn = JsonConvert.DeserializeObject<SerializableData[]>(json, settings);
		//List<SerializableData> toReturn = new List<SerializableData>();

		return toReturn;
	}


	private static bool CheckDirectory(string directory, bool force)
	{
		if (!Directory.Exists(directory))
		{
			if (force)
			{
				Debug.LogWarning("The directory " + directory + " did not exist, but has now been created.");
				Directory.CreateDirectory(directory);
				return true;
			}
			else
			{
				Debug.LogError("The directory " + directory + " Does not exist.");
				return false;
			}
		}

		return false;
	}

	private static int FindDataIndexInList(SerializableData toFind, List<SerializableData> searchIn)
	{
		for (int i = 0; i < searchIn.Count; i++)
		{
			if (searchIn[i] == toFind)
			{
				return i;
			}
		}

		return -1;
	}

	private static string[] SearchNamesInFolderByExtension(string extension, string directory)
	{
		List<string> toReturn = new List<string>();
		string[] allFileNames = GetFileNamesInFolder(directory);
		for (int i = 0; i < allFileNames.Length; i++)
		{
			string found = Path.GetExtension(Path.Combine(directory, allFileNames[i]));

			if (found == extension)
				toReturn.Add(allFileNames[i]);
		}

		return toReturn.ToArray();
	}

	private static Dictionary<string, SerializableData> ConvertToDictionary(SerializableData[] array)
	{
		Dictionary<string, SerializableData> toReturn = new Dictionary<string, SerializableData>();

		for (int i = 0; i < array.Length; i++)
		{
			toReturn.Add(array[i].instanceIdentifier, array[i]);
		}

		return toReturn;
	}

	#endregion

	#region File Functions

	public static string[] GetFileNamesInFolder(string directory)
	{
		List<string> toReturn = new List<string>();
		DirectoryInfo info = new DirectoryInfo(directory);
		FileInfo[] fileInfos = info.GetFiles();
		for (int i = 0; i < fileInfos.Length; i++)
		{
			toReturn.Add(fileInfos[i].Name);
		}

		return toReturn.ToArray();
	}

	public static string GetFullPathOfGameObject(GameObject getPathOf, bool endWithNumber)
	{
		string toReturn = string.Empty;
		GameObject check = getPathOf;

		if (endWithNumber)
			toReturn = "/" + getPathOf.transform.GetSiblingIndex().ToString();
		else
			toReturn = "/" + getPathOf.name;

		while (check.transform.parent != null)
		{
			check = check.transform.parent.gameObject;
			toReturn = "/" + check.name + toReturn;
		}

		return toReturn;
	}

	public static void DeleteFile(string directory, string fileName, string fileExtension)
	{
		string path = Path.Combine(directory, fileName + fileExtension);

		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	#endregion

}
