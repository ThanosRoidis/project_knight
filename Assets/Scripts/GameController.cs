using System;
using UnityEngine;
using Shared;

public class GameController : Shared.BaseGameController
{
    public static new GameController Instance { get => (GameController)BaseGameController.Instance; }

    [SerializeField] private float hitstopSlowdown = 0.01f;
    private float hitstopStartTime;
    private float hitstopDuration;

    protected override void Awake()
    {
        base.Awake();
        hitstopStartTime = -1f;
        hitstopDuration = -1f;
    }

    protected void Start()
    {
        LockPlayerControl();

        FadeImage.Instance.ExecuteOnClear += () => {
            StartGameplay();
        };
    }

    /// <summary>
    /// Called by countdown timer after it is completed
    /// </summary>
    public override void StartGameplay()
    {
        base.StartGameplay();
        UnlockPlayerControl();
    }

    private void Update()
    {
        if (hitstopStartTime > 0f) {
            if (GenericUtils.TimeElapsed(hitstopStartTime, hitstopDuration, 1 / Time.timeScale)) {
                Time.timeScale /= hitstopSlowdown;
                hitstopStartTime = -1f;
                hitstopDuration = -1f;
            }
        }
    }


    public override void PauseTime()
    {
        base.PauseTime();
        LockPlayerControl();
    }


    public override void ContinueTime()
    {
        base.ContinueTime();
        UnlockPlayerControl();
    }

    public override void LockPlayerControl()
    {
        PlayerController.Instance.InputLocked = true;
    }

    public override void UnlockPlayerControl()
    {
        PlayerController.Instance.InputLocked = false;
    }

    public void ApplyHitstop(float duration)
    {
        if (duration == 0) {
            return;
        }
        //If hitstop is already happening, reset it
        if (hitstopStartTime > 0f) {
            if (duration > hitstopDuration - (Time.time - hitstopStartTime)) {
                hitstopDuration = duration;
                hitstopStartTime = Time.time;
            }
        } else {
            hitstopStartTime = Time.time;
            hitstopDuration = duration;
            Time.timeScale *= hitstopSlowdown;
        }
    }
}