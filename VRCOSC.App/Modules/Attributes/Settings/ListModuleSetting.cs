﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Attributes.Settings;

public abstract class ListModuleSetting<T> : ModuleSetting
{
    public ObservableCollection<T> Attribute { get; private set; } = null!;
    protected readonly IEnumerable<T> DefaultValues;

    internal override object GetRawValue() => Attribute.ToList();

    internal override void Load()
    {
        Attribute = new ObservableCollection<T>(getClonedDefaults());
        Attribute.CollectionChanged += (_, _) => RequestSerialisation?.Invoke();
    }

    internal override bool IsDefault() => Attribute.SequenceEqual(DefaultValues);

    internal override void SetDefault()
    {
        Attribute.Clear();
        getClonedDefaults().ForEach(item => Attribute.Add(item));
    }

    private IEnumerable<T> getClonedDefaults() => DefaultValues.Select(CloneValue);
    private IEnumerable<T> jArrayToEnumerable(JArray array) => array.Select(ConstructValue);

    protected abstract T CloneValue(T value);
    protected abstract T ConstructValue(JToken token);

    internal void AddItem() => Attribute.Add(CreateNewItem());
    protected abstract T CreateNewItem();

    internal override bool Deserialise(object value)
    {
        jArrayToEnumerable((JArray)value).ForEach(item => Attribute.Add(item));
        return true;
    }

    protected ListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<T> defaultValues)
        : base(metadata)
    {
        DefaultValues = defaultValues;
    }
}

public abstract class ValueListModuleSetting<T> : ListModuleSetting<Observable<T>>
{
    internal override object GetRawValue() => Attribute.Select(observable => observable.Value).ToList();
    internal override bool IsDefault() => Attribute.Select(observable => observable.Value).SequenceEqual(DefaultValues.Select(defaultObservable => defaultObservable.Value));

    internal override void Load()
    {
        base.Load();

        Attribute.CollectionChanged += (_, e) => subscribeToNewItems(e);
        return;

        void subscribeToNewItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null) return;

            foreach (Observable<T> newItem in e.NewItems)
            {
                newItem.Subscribe(_ => RequestSerialisation?.Invoke());
            }
        }
    }

    protected override Observable<T> CloneValue(Observable<T> value) => new(value.Value!);
    protected override Observable<T> ConstructValue(JToken token) => new(token.Value<T>()!);

    protected ValueListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<Observable<T>> defaultValues)
        : base(metadata, defaultValues)
    {
    }
}

public class StringListModuleSetting : ValueListModuleSetting<string>
{
    public StringListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<string> defaultValues)
        : base(metadata, defaultValues.Select(value => new Observable<string>(value)))
    {
    }

    protected override Observable<string> CreateNewItem() => new(string.Empty);
}
