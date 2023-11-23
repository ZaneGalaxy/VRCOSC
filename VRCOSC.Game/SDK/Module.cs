﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Framework.Timing;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.SDK.Attributes;
using VRCOSC.Game.SDK.Attributes.Parameters;
using VRCOSC.Game.SDK.Attributes.Settings;
using VRCOSC.Game.SDK.Graphics.Settings.Values;
using VRCOSC.Game.SDK.Parameters;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.SDK;

public class Module
{
    public string PackageId { get; set; } = null!;

    private GameHost host = null!;
    private Scheduler scheduler = null!;
    private AppManager appManager = null!;

    private SerialisationManager serialisationManager = null!;

    internal Bindable<bool> Enabled = new();

    internal readonly Bindable<ModuleState> State = new(ModuleState.Stopped);

    internal string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    internal string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    internal ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();
    internal readonly Dictionary<string, ModuleSetting> Settings = new();
    internal readonly Dictionary<string, List<string>> Groups = new();

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    internal string SerialisedName => $"{PackageId}.{GetType().Name.ToLowerInvariant()}";

    protected Module()
    {
        State.BindValueChanged(onModuleStateChange);
    }

    private void onModuleStateChange(ValueChangedEvent<ModuleState> e)
    {
        Log(e.NewValue.ToString());
    }

    internal void InjectDependencies(GameHost host, IClock clock, AppManager appManager, SerialisationManager serialisationManager)
    {
        scheduler = new Scheduler(() => ThreadSafety.IsUpdateThread, clock);
        this.host = host;
        this.appManager = appManager;
        this.serialisationManager = serialisationManager;
    }

    internal void Serialise()
    {
        serialisationManager.Serialise();
    }

    internal void Deseralise()
    {
        serialisationManager.Deserialise();
    }

    internal void Load()
    {
        OnLoad();

        Settings.Values.ForEach(moduleSetting => moduleSetting.Load());
        Parameters.Values.ForEach(moduleParameter => moduleParameter.Load());

        Deseralise();

        Enabled.BindValueChanged(_ => Serialise());
        Settings.Values.ForEach(moduleSetting => moduleSetting.RequestSerialisation = Serialise);
        Parameters.Values.ForEach(moduleParameter => moduleParameter.RequestSerialisation = Serialise);

        OnPostLoad();
    }

    internal void FrameworkUpdate()
    {
        scheduler.Update();
    }

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = parameterName.Replace("/", @"\/").Replace("*", @"(\S*)");
        pattern += "$";
        return new Regex(pattern);
    }

    internal async Task Start()
    {
        State.Value = ModuleState.Starting;

        parameterNameEnum.Clear();
        Parameters.ForEach(pair => parameterNameEnum.Add(pair.Value.Name.Value, pair.Key));

        parameterNameRegex.Clear();
        Parameters.ForEach(pair => parameterNameRegex.Add(pair.Value.Name.Value, parameterToRegex(pair.Value.Name.Value)));

        var startResult = await OnModuleStart();

        if (!startResult)
        {
            await Stop();
            return;
        }

        State.Value = ModuleState.Started;

        initialiseUpdateAttributes(GetType());
    }

    internal async Task Stop()
    {
        State.Value = ModuleState.Stopping;

        scheduler.CancelDelayedTasks();
        await OnModuleStop();

        State.Value = ModuleState.Stopped;
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

    protected virtual Task<bool> OnModuleStart() => Task.FromResult(true);
    protected virtual Task OnModuleStop() => Task.CompletedTask;

    internal void AvatarChange()
    {
        OnAvatarChange();
    }

    protected virtual void OnAvatarChange()
    {
    }

    /// <summary>
    /// Registers a parameter with a lookup to allow the user to customise the parameter name
    /// </summary>
    /// <param name="lookup">The lookup of this parameter, used as a reference when calling <see cref="SendParameter(Enum,object)"/></param>
    /// <param name="defaultName">The default name of the parameter</param>
    /// <param name="title">The title of the parameter</param>
    /// <param name="description">A short description of the parameter</param>
    /// <param name="mode">Whether the parameter can read to or write from VRChat</param>
    protected void RegisterParameter<T>(Enum lookup, string defaultName, ParameterMode mode, string title, string description) where T : struct
    {
        Parameters.Add(lookup, new ModuleParameter(new ModuleParameterMetadata(title, description, mode, typeof(T)), defaultName));
    }

    /// <summary>
    /// Specifies a list of settings to group together in the UI
    /// </summary>
    /// <param name="title">The title of the group</param>
    /// <param name="lookups">The settings lookups to put in this group</param>
    protected void CreateGroup(string title, params Enum[] lookups)
    {
        Groups.Add(title, lookups.Select(lookup => lookup.ToLookup()).ToList());
    }

    /// <summary>
    /// Allows you to define a completely custom <see cref="ModuleSetting"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <param name="moduleSetting">The custom <see cref="ModuleSetting"/></param>
    protected void CreateCustom(Enum lookup, ModuleSetting moduleSetting)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), moduleSetting);
    }

    protected void CreateToggle(Enum lookup, string title, string description, bool defaultValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new BoolModuleSetting(new ModuleSettingMetadata(title, description, typeof(DrawableBoolModuleSetting)), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, string defaultValue, bool emptyIsValid = false)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new StringModuleSetting(new ModuleSettingMetadata(title, description, typeof(DrawableStringModuleSetting)), emptyIsValid, defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, int defaultValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new IntModuleSetting(new ModuleSettingMetadata(title, description, typeof(DrawableIntModuleSetting)), defaultValue));
    }

    protected void CreateDropdown<T>(Enum lookup, string title, string description, T defaultValue) where T : Enum
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new EnumModuleSetting<T>(new ModuleSettingMetadata(title, description, typeof(DrawableEnumModuleSetting<T>)), defaultValue));
    }

    protected void CreateSlider(Enum lookup, string title, string description, int defaultValue, int minValue, int maxValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new RangedIntModuleSetting(new ModuleSettingMetadata(title, description, typeof(DrawableIntSliderModuleSetting)), defaultValue, minValue, maxValue));
    }

    protected void CreateSlider(Enum lookup, string title, string description, float defaultValue, float minValue, float maxValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new RangedFloatModuleSetting(new ModuleSettingMetadata(title, description, typeof(DrawableFloatSliderModuleSetting)), defaultValue, minValue, maxValue));
    }

    /// <summary>
    /// Create your settings and register your parameters by overriding this method
    /// </summary>
    protected virtual void OnLoad()
    {
    }

    /// <summary>
    /// Link your settings together and define callbacks for custom UI behaviour by overriding this method
    /// </summary>
    protected virtual void OnPostLoad()
    {
    }

    protected void OpenUrlExternally(string url) => host.OpenUrlExternally(url);

    /// <summary>
    /// Maps a value <paramref name="source"/> from a source range to a destination range
    /// </summary>
    protected static float Map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));

    private void validateSettingsLookup(Enum lookup)
    {
        if (!Settings.ContainsKey(lookup.ToLookup())) return;

        PushException(new InvalidOperationException("Cannot add multiple of the same key for settings"));
    }

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    protected ModuleSetting? GetSetting(Enum lookup) => GetSetting<ModuleSetting>(lookup.ToLookup());

    internal T? GetSetting<T>(string lookup) where T : ModuleSetting
    {
        if (Settings.TryGetValue(lookup, out var setting)) return (T)setting;

        PushException(new InvalidOperationException($"Cannot access setting of lookup {lookup} as it has not been created"));
        return default;
    }

    internal ModuleParameter? GetParameter(string lookup)
    {
        return Parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup).Value;
    }

    /// <summary>
    /// Retrieves a <see cref="ModuleSetting"/>'s value as a shorthand for <see cref="ModuleAttribute.GetValue{TValueType}"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <typeparam name="T">The value type of the setting</typeparam>
    /// <returns>The value if successful, otherwise pushes an exception and returns default</returns>
    protected T? GetSettingValue<T>(Enum lookup)
    {
        if (!Settings.ContainsKey(lookup.ToLookup()))
        {
            PushException(new InvalidOperationException($"Cannot access setting of lookup {lookup} as it has not been created"));
            return default;
        }

        if (Settings[lookup.ToLookup()].GetValue<T>(out var value)) return value;

        PushException(new InvalidOperationException($"Could not get setting of lookup {lookup} and of type {typeof(T)}"));
        return default;
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
        if (!Parameters.TryGetValue(lookup, out var moduleParameter))
        {
            PushException(new InvalidOperationException($"Lookup `{lookup}` has not been registered. Please register it using `RegisterParameter<T>(Enum,object)`"));
            return;
        }

        appManager.VRChatOscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{moduleParameter.Name.Value}", value);
    }

    #endregion

    private string className => GetType().Name.ToLowerInvariant();

    protected internal async void PushException(Exception e)
    {
        Logger.Error(e, $"{className} experienced an exception");
        await Stop();
    }

    internal void PlayerUpdate()
    {
        OnPlayerUpdate();
    }

    protected virtual void OnPlayerUpdate()
    {
    }

    internal void OnParameterReceived(VRChatOscMessage message)
    {
        var receivedParameter = new ReceivedParameter(message.ParameterName, message.ParameterValue);

        try
        {
            OnAnyParameterReceived(receivedParameter);
        }
        catch (Exception e)
        {
            PushException(e);
        }

        var parameterName = Parameters.Values.FirstOrDefault(moduleParameter => parameterNameRegex[moduleParameter.Name.Value].IsMatch(receivedParameter.Name))?.Name.Value;
        if (parameterName is null) return;

        if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = Parameters[lookup];

        if (!parameterData.Metadata.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!receivedParameter.IsValueType(parameterData.Metadata.ExpectedType))
        {
            Log($"Cannot accept input parameter. `{lookup}` expects type `{parameterData.Metadata.ExpectedType.ToReadableName()}` but received type `{receivedParameter.Value.GetType().ToReadableName()}`");
            return;
        }

        var registeredParameter = new RegisteredParameter(receivedParameter, lookup, parameterData);

        try
        {
            OnRegisteredParameterReceived(registeredParameter);
        }
        catch (Exception e)
        {
            PushException(e);
        }
    }

    protected virtual void OnAnyParameterReceived(ReceivedParameter receivedParameter)
    {
    }

    protected virtual void OnRegisteredParameterReceived(RegisteredParameter registeredParameter)
    {
    }
}