using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using StarterKit;
using UnityEngine;

public class SavedDataHandler : IndestructibleSingleton<SavedDataHandler>
{
    [Header("Data Credentials")] public string password;

    [Header("Current Save Data")] public SaveData _saveData;

    [Header("Default Data")] public SaveData _DefaultSaveData;

    // [Header("Data Containers")] 
    // public UserData userData;


    public static Action<SaveData> OnDataLoaded;

    public bool IsDataLoaded { get; private set; }

    public override void OnAwake()
    {
        base.OnAwake();
        IsDataLoaded = false;
		if (!File.Exists(SaveGameData.filePath))
		{
			_saveData = SaveGameData.Load(_DefaultSaveData, password);
            ApplyData(_saveData);
			SetFirstLaunch();
		}
        else
        {
			_saveData = SaveGameData.Load(_saveData, password);
			ApplyData(_saveData);
		}
	}

    [ContextMenu ("Clear Data")]
    public void Clear()
    {
        SaveGameData.Delete();
        Debug.Log("Save File has been deleted!");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            FetchDataFromGame();
            SaveGameData.Save(_saveData, password);
        }
        else
        {
            _saveData = SaveGameData.Load(_DefaultSaveData, password);
            ApplyData(_saveData);
        }
    }

    public void SaveData()
    {
        FetchDataFromGame();
        SaveGameData.Save(_saveData, password);
    }
    public void SaveDataLocally()
    {
        FetchDataFromGame();
        SaveGameData.Save(_saveData, password);
    }

    public void ResetToDefault()
    {
        _saveData = SaveGameData.Clear(_DefaultSaveData, password);
        ApplyData(_saveData);
    }

    private void SetFirstLaunch()
	{
		if (!_saveData.isFirstLaunch)
		{
			_saveData.isFirstLaunch = true;
		}

        SaveData();
	}

    public void ApplyData(SaveData saveData)
    {
        // userData.LoadSavedUserData(saveData.savedPlayerProfileData);
    }
    
    public void FetchDataFromGame()
    {
        // _saveData.savedPlayerProfileData.UpdateUserData(userData.totalScore, userData.lives, userData.topicProgressList);
    }
}

[Serializable]
public class SaveData
{
    public bool isFirstLaunch;
    public SavedPlayerProfileData savedPlayerProfileData;
}

[Serializable]
public class SavedPlayerProfileData
{
    // public List<UserTopicProgress> topicProgressList;
	public int totalScore;
	public int lives;

    // public void UpdateUserData(int totalScore, int lives, List<UserTopicProgress> topicProgressList)
    // {
    //     ClearTopicProgressList();
    //     this.totalScore = totalScore;
    //     this.lives = lives;
	// 	this.topicProgressList = new List<UserTopicProgress>(topicProgressList);
	// }

    // public void ClearTopicProgressList()
    // {
	// 	if (this.topicProgressList.Count > 0)
	// 	{
	// 		topicProgressList.Clear();
	// 		topicProgressList = null;
	// 	}
	// }
}