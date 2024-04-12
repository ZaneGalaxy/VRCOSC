﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Clips.Variables.Instances;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules;

public class ChatBoxModule : AvatarModule
{
    #region Runtime

    protected void ChangeState(Enum lookup)
    {
        ChangeState(lookup.ToLookup());
    }

    protected void ChangeState(string lookup)
    {
        ChatBoxManager.GetInstance().ChangeStateTo(SerialisedName, lookup);
    }

    protected void TriggerEvent(Enum lookup)
    {
        TriggerEvent(lookup.ToLookup());
    }

    protected void TriggerEvent(string lookup)
    {
        ChatBoxManager.GetInstance().TriggerEvent(SerialisedName, lookup);
    }

    #endregion

    #region States

    protected ClipStateReference? CreateState(Enum lookup, string displayName, string defaultFormat = "", List<ClipVariableReference>? defaultVariables = null)
    {
        return CreateState(lookup.ToLookup(), displayName, defaultFormat, defaultVariables);
    }

    protected ClipStateReference? CreateState(string lookup, string displayName, string defaultFormat = "", List<ClipVariableReference>? defaultVariables = null)
    {
        if (GetState(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{SerialisedName}]: You cannot add the same lookup ({lookup}) for a state more than once");
            return null;
        }

        var clipStateReference = new ClipStateReference
        {
            ModuleID = SerialisedName,
            StateID = lookup,
            DefaultFormat = defaultFormat,
            DefaultVariables = defaultVariables ?? [],
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateState(clipStateReference);
        return clipStateReference;
    }

    protected void DeleteState(Enum lookup)
    {
        DeleteState(lookup.ToLookup());
    }

    protected void DeleteState(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteState(SerialisedName, lookup);
    }

    protected ClipStateReference? GetState(Enum lookup)
    {
        return GetState(lookup.ToLookup());
    }

    protected ClipStateReference? GetState(string lookup)
    {
        return ChatBoxManager.GetInstance().GetState(SerialisedName, lookup);
    }

    #endregion

    #region Events

    protected ClipEventReference? CreateEvent(Enum lookup, string displayName, string defaultFormat = "", List<ClipVariableReference>? defaultVariables = null, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        return CreateEvent(lookup.ToLookup(), displayName, defaultFormat, defaultVariables, defaultLength, defaultBehaviour);
    }

    protected ClipEventReference? CreateEvent(string lookup, string displayName, string defaultFormat = "", List<ClipVariableReference>? defaultVariables = null, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        if (GetEvent(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{SerialisedName}]: You cannot add the same lookup ({lookup}) for an event more than once");
            return null;
        }

        var clipEventReference = new ClipEventReference
        {
            ModuleID = SerialisedName,
            EventID = lookup,
            DefaultFormat = defaultFormat,
            DefaultVariables = defaultVariables ?? [],
            DefaultLength = defaultLength,
            DefaultBehaviour = defaultBehaviour,
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateEvent(clipEventReference);
        return clipEventReference;
    }

    protected void DeleteEvent(Enum lookup)
    {
        DeleteEvent(lookup.ToLookup());
    }

    protected void DeleteEvent(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteEvent(SerialisedName, lookup);
    }

    protected ClipEventReference? GetEvent(Enum lookup)
    {
        return GetEvent(lookup.ToLookup());
    }

    protected ClipEventReference? GetEvent(string lookup)
    {
        return ChatBoxManager.GetInstance().GetEvent(SerialisedName, lookup);
    }

    #endregion

    #region Variables

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName)
    {
        Type? clipVariableType = null;

        if (typeof(T) == typeof(bool))
            clipVariableType = typeof(BoolClipVariable);
        else if (typeof(T) == typeof(int))
            clipVariableType = typeof(IntClipVariable);
        else if (typeof(T) == typeof(float))
            clipVariableType = typeof(FloatClipVariable);
        else if (typeof(T) == typeof(string))
            clipVariableType = typeof(StringClipVariable);
        else if (typeof(T) == typeof(DateTime))
            clipVariableType = typeof(DateTimeClipVariable);
        else if (typeof(T) == typeof(TimeSpan))
            clipVariableType = typeof(TimeSpanClipVariable);

        if (clipVariableType is null)
            throw new InvalidOperationException("No clip variable exists for that type. Request it is added to the SDK or make a custom clip variable");

        return CreateVariable<T>(lookup, displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName, Type clipVariableType)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName, Type clipVariableType)
    {
        if (GetVariable(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{SerialisedName}]: You cannot add the same lookup ({lookup}) for a variable more than once");
            return null;
        }

        var clipVariableReference = new ClipVariableReference
        {
            ModuleID = SerialisedName,
            VariableID = lookup,
            ClipVariableType = clipVariableType,
            ValueType = typeof(T),
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateVariable(clipVariableReference);
        return clipVariableReference;
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected void DeleteVariable(Enum lookup)
    {
        DeleteVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected void DeleteVariable(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteVariable(SerialisedName, lookup);
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? GetVariable(Enum lookup)
    {
        return GetVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected ClipVariableReference? GetVariable(string lookup)
    {
        return ChatBoxManager.GetInstance().GetVariable(SerialisedName, lookup);
    }

    #endregion
}
