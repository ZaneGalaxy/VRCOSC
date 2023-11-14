﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Framework.Timing;
using VRCOSC.Game.Modules.SDK.Parameters;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules.SDK;

public class Module
{
    private Scheduler scheduler = null!;
    private AppManager appManager = null!;

    internal Bindable<bool> Enabled = new();

    internal readonly Bindable<ModuleState> State = new(ModuleState.Stopped);

    internal string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    internal string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    internal ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    private Dictionary<Enum, ModuleParameter> moduleParameters = new();

    protected Module()
    {
        State.BindValueChanged(onModuleStateChange);
    }

    private void onModuleStateChange(ValueChangedEvent<ModuleState> e)
    {
        Log($"State changed to {e.NewValue}");
    }

    internal void InjectDependencies(IClock clock, AppManager appManager)
    {
        scheduler = new Scheduler(() => ThreadSafety.IsUpdateThread, clock);
        this.appManager = appManager;
    }

    internal void Load()
    {
        OnLoad();
    }

    internal void FrameworkUpdate()
    {
        scheduler.Update();
    }

    internal Task Start()
    {
        State.Value = ModuleState.Starting;

        var startTask = OnModuleStart();
        startTask.GetAwaiter().OnCompleted(() =>
        {
            State.Value = ModuleState.Started;

            initialiseUpdateAttributes(GetType());
        });
        return startTask;
    }

    internal Task Stop()
    {
        State.Value = ModuleState.Stopping;

        scheduler.CancelDelayedTasks();

        var stopTask = OnModuleStop();
        stopTask.GetAwaiter().OnCompleted(() => State.Value = ModuleState.Stopped);
        return stopTask;
    }

    private void updateMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            PushException(new Exception($"{className} experienced an exception calling method {method.Name}", e));
        }
    }

    private void initialiseUpdateAttributes(Type? type)
    {
        if (type is null) return;

        initialiseUpdateAttributes(type.BaseType);

        type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ForEach(method =>
            {
                var updateAttribute = method.GetCustomAttribute<ModuleUpdateAttribute>();
                if (updateAttribute is null) return;

                switch (updateAttribute.Mode)
                {
                    case ModuleUpdateMode.Custom:
                        scheduler.AddDelayed(() => updateMethod(method), updateAttribute.DeltaMilliseconds, true);
                        if (updateAttribute.UpdateImmediately) updateMethod(method);
                        break;
                }
            });
    }

    #region SDK Exposed

    protected virtual Task OnModuleStart() => Task.CompletedTask;
    protected virtual Task OnModuleStop() => Task.CompletedTask;

    /// <summary>
    /// Registers a parameter with a lookup to allow the user to customise the parameter name
    /// </summary>
    /// <param name="lookup">The lookup of this parameter, used as a reference when calling <see cref="SendParameter(Enum,object)"/></param>
    /// <param name="defaultName">The default name of the parameter</param>
    /// <param name="title">The title of the parameter</param>
    /// <param name="description">A short description of the parameter</param>
    /// <param name="mode">Whether the parameter can read to or write from VRChat</param>
    protected void RegisterParameter<T>(Enum lookup, string defaultName, string title, string description, ParameterMode mode) where T : struct
    {
        moduleParameters.Add(lookup, new ModuleParameter(defaultName, title, description, mode, typeof(T)));
    }

    protected virtual void OnLoad()
    {
    }

    /// <summary>
    /// Logs to the terminal when the module is running
    /// </summary>
    /// <param name="message">The message to log to the terminal</param>
    protected void Log(string message)
    {
        Logger.Log($"[{Title}]: {message}", TerminalLogger.TARGET_NAME);
    }

    /// <summary>
    /// Allows you to send any parameter name and value.
    /// If you want the user to be able to customise the parameter, register a parameter and use <see cref="SendParameter(Enum,object)"/>
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    protected void SendParameter(string name, object value)
    {
        appManager.VRChatOscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", value);
    }

    /// <summary>
    /// Allows you to send a customisable parameter using its lookup and a value
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    protected void SendParameter(Enum lookup, object value)
    {
        if (!moduleParameters.TryGetValue(lookup, out var moduleParameter))
        {
            PushException(new InvalidOperationException($"Lookup `{lookup}` has not been registered. Please register it using RegisterParameter<T>(Enum,object)"));
            return;
        }

        appManager.VRChatOscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{moduleParameter.Name.Value}", value);
    }

    #endregion

    private string className => GetType().Name.ToLowerInvariant();

    protected internal void PushException(Exception e)
    {
        State.Value = ModuleState.Exception;
        Logger.Error(e, $"{className} experienced an exception");
    }
}
