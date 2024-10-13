using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedyScreensaver : MonoBehaviour
{
    public KMNeedyModule Module;
    public KMSelectable Button;
    public KMAudio Audio;

    private static readonly float MinX = -0.0557f;
    private static readonly float MaxX = 0.0557f;
    private static readonly float MinZ = -0.0664f;
    private static readonly float MaxZ = 0.01709f;
    //private static readonly float CornerOffset = 0.005f;

    private bool IsActive;
    private bool FirstActivate;
    private float PosX;
    private float PosZ;
    private bool ForX;
    private bool ForZ;

    private static int _moduleIdCounter;
    private int _moduleId;

    void Start()
    {
        _moduleId = ++_moduleIdCounter;
        Button.gameObject.SetActive(false);
        Module.OnNeedyActivation += OnNeedyActivation;
        Module.OnNeedyDeactivation += OnNeedyDeactivation;
        Module.OnTimerExpired += OnTimerExpired;
        Button.OnHighlightEnded += OnHighlightEnded;
        Button.OnInteract += OnInteract;
    }

    private void Log(string format, params object[] args)
	{
        Debug.LogFormat("[Screensaver #{0}] {1}", _moduleId, string.Format(format, args));
	}

    void OnNeedyActivation()
    {
        Log("Activated");
        ChangeColor();
        PosX = UnityEngine.Random.Range(MinX, MaxX);
        PosZ = UnityEngine.Random.Range(MinZ, MaxZ);
        ForX = UnityEngine.Random.value > 0.5f;
        ForZ = UnityEngine.Random.value > 0.5f;
        IsActive = true;
        FirstActivate = true;
    }

    void OnNeedyDeactivation()
    {
        Log("Deactivated");
        IsActive = false;
        Button.gameObject.SetActive(false);
    }

    void OnTimerExpired()
    {
        Log("Time expired");
        Module.OnStrike();
        OnNeedyDeactivation();
    }

    void OnHighlightEnded()
    {
        float time = Module.GetNeedyTimeRemaining();
        if (time > 4f)
        {
            Log("Subtracted 3 seconds from time ({0} -> {1})", Mathf.Floor(time), Mathf.Floor(time - 3f));
            Module.SetNeedyTimeRemaining(time - 3f);
        }
    }

    bool OnInteract()
    {
        Log("Disarmed");
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
        Button.AddInteractionPunch(0.5f);
        Module.HandlePass();
        OnNeedyDeactivation();
        return false;
    }

    void Update()
    {
        if (IsActive)
        {
            bool logged = false;
            float NextX = GetNextX();
            if (!IsInBoundsX(NextX))
            {
                Log("Hit {0} wall", ForX ? "right" : "left");
                logged = true;
                ForX = !ForX;
                NextX = GetNextX();
                ChangeColor();
            }
            PosX = NextX;
            float NextZ = GetNextZ();
            if (!IsInBoundsZ(NextZ))
            {
                if (!logged)
				{
                    Log("Hit {0} wall", ForZ ? "top" : "bottom");
                }
                ForZ = !ForZ;
                NextZ = GetNextZ();
                ChangeColor();
            }
            PosZ = NextZ;
            Button.transform.localPosition = new Vector3(PosX, Button.transform.localPosition.y, PosZ);
            if (FirstActivate)
            {
                FirstActivate = false;
                Button.gameObject.SetActive(true);
            }
        }
    }

    void ChangeColor()
    {
        Button.GetComponent<MeshRenderer>().material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }

    float GetNextX()
    {
        return GetNext(PosX, ForX);
    }

    float GetNextZ()
    {
        return GetNext(PosZ, ForZ);
    }

    float GetNext(float pos, bool forw)
    {
        return pos + Time.deltaTime * 0.15f * (forw ? 1 : -1);
    }

    bool IsInBoundsX(float position)
    {
        return position >= MinX && position <= MaxX;
    }

    bool IsInBoundsZ(float position)
    {
        return position >= MinZ && position <= MaxZ;
    }
}
