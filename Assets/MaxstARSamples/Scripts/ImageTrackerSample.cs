﻿using UnityEngine;
using System.Collections.Generic;
using System.Text;

using maxstAR;

public class ImageTrackerSample : MonoBehaviour
{
	private Dictionary<string, ImageTrackableBehaviour> imageTrackablesMap =
		new Dictionary<string, ImageTrackableBehaviour>();
	private bool startTrackerDone = false;
	private bool cameraStartDone = false;

	void Start()
	{
		imageTrackablesMap.Clear();
		ImageTrackableBehaviour[] imageTrackables = FindObjectsOfType<ImageTrackableBehaviour>();
		foreach (var trackable in imageTrackables)
		{
			imageTrackablesMap.Add(trackable.TrackableName, trackable);
			Debug.Log("Trackable add: " + trackable.TrackableName);
		}

		AddTrackerData();
	}

	private void AddTrackerData()
	{
		foreach (var trackable in imageTrackablesMap)
		{
			if (trackable.Value.TrackerDataFileName.Length == 0)
			{
				continue;
			}

			if (trackable.Value.StorageType == StorageType.AbsolutePath)
			{
				TrackerManager.GetInstance().AddTrackerData(trackable.Value.TrackerDataFileName);
			}
			else
			{
				if (Application.platform == RuntimePlatform.Android)
				{
					TrackerManager.GetInstance().AddTrackerData(trackable.Value.TrackerDataFileName, true);
				}
				else
				{
					TrackerManager.GetInstance().AddTrackerData(Application.streamingAssetsPath + "/" + trackable.Value.TrackerDataFileName);
				}
			}
		}

		TrackerManager.GetInstance().LoadTrackerData();
	}

	private void DisableAllTrackables()
	{
		foreach (var trackable in imageTrackablesMap)
		{
			trackable.Value.OnTrackFail();
		}
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			SceneStackManager.Instance.LoadPrevious();
		}

		StartCamera();

		if (!startTrackerDone)
		{
			TrackerManager.GetInstance().StartTracker(MaxstARUtils.TrackerMask.IMAGE_TRACKER);
			SetExtendedMode();
			startTrackerDone = true;
		}

		DisableAllTrackables();

		TrackingResult trackingResult = TrackerManager.GetInstance().GetTrackingResult();

		for (int i = 0; i < trackingResult.GetCount(); i++)
		{
			Trackable trackable = trackingResult.GetTrackable(i);

			Debug.Log("Name : " + trackable.GetName());
			if (!imageTrackablesMap.ContainsKey(trackable.GetName()))
			{
				return;
			}

			imageTrackablesMap[trackable.GetName()].OnTrackSuccess(trackable.GetId(), trackable.GetName(),
																   trackable.GetPose());
		}
	}

	public void SetNormalMode()
	{
		TrackerManager.GetInstance().SetTrackingOption(MaxstARUtils.ImageTrackerMode.NORMAL_MODE);
	}

	public void SetExtendedMode()
	{
		TrackerManager.GetInstance().SetTrackingOption(MaxstARUtils.ImageTrackerMode.EXTEND_MODE);
	}

	public void SetMultiMode()
	{
		TrackerManager.GetInstance().SetTrackingOption(MaxstARUtils.ImageTrackerMode.MULTI_MODE);
	}

	void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			TrackerManager.GetInstance().StopTracker();
			startTrackerDone = false;
			StopCamera();
		}
	}

	void OnDestroy()
	{
		imageTrackablesMap.Clear();
		TrackerManager.GetInstance().SetTrackingOption(MaxstARUtils.ImageTrackerMode.NORMAL_MODE);
		TrackerManager.GetInstance().StopTracker();
		TrackerManager.GetInstance().DestroyTracker();
		StopCamera();
	}

	void StartCamera()
	{
		if (!cameraStartDone)
		{
			Debug.Log("Unity StartCamera");
			ResultCode result = CameraDevice.GetInstance().Start();
			if (result == ResultCode.Success)
			{
				cameraStartDone = true;
			}
		}
	}

	void StopCamera()
	{
		if (cameraStartDone)
		{
			Debug.Log("Unity StopCamera");
			CameraDevice.GetInstance().Stop();
			cameraStartDone = false;
		}
	}
}