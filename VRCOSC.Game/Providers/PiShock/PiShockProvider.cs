﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VRCOSC.Game.Providers.PiShock;

public class PiShockProvider
{
    private const string app_name = "VRCOSC";
    private const string api_url = "https://do.pishock.com/api/apioperate";

    private readonly HttpClient client = new();
    private readonly string apiKey;

    public PiShockProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> Execute(string username, string sharecode, PiShockMode mode, int duration, int intensity)
    {
        if (duration is < 1 or > 15) throw new InvalidOperationException($"{nameof(duration)} must be between 1 and 15");
        if (intensity is < 1 or > 100) throw new InvalidOperationException($"{nameof(intensity)} must be between 1 and 100");

        var request = getRequestForMode(mode, duration, intensity);
        request.AppName = app_name;
        request.APIKey = apiKey;
        request.Username = username;
        request.ShareCode = sharecode;

        var response = await client.PostAsync(api_url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
        return await response.Content.ReadAsStringAsync();
    }

    private static BasePiShockRequest getRequestForMode(PiShockMode mode, int duration, int intensity) => mode switch
    {
        PiShockMode.Shock => new ShockPiShockRequest
        {
            Duration = duration.ToString(),
            Intensity = intensity.ToString()
        },
        PiShockMode.Vibrate => new VibratePiShockRequest
        {
            Duration = duration.ToString(),
            Intensity = intensity.ToString()
        },
        PiShockMode.Beep => new BeepPiShockRequest
        {
            Duration = duration.ToString()
        },
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}

public enum PiShockMode
{
    Shock,
    Vibrate,
    Beep
}
