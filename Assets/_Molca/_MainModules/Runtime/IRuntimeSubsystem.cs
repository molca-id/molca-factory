using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRuntimeSubsystem
{
    bool IsActive { get; }
    /// <summary>
    /// Runtime manager will wait until all subsystems calls finishCallback, always invoke the callback at the end of initialization.
    /// </summary>
    /// <param name="finishCallback"></param>
    void Initialize(Action<IRuntimeSubsystem> finishCallback);
    void Activate();
    void Deactivate();
}