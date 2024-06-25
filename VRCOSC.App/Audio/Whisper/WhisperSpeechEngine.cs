﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public partial class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;
    private string previousText;
    private bool triggeredFinal;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID);
        var captureDevice = AudioDeviceHelper.GetDeviceByID(captureDeviceId);

        if (captureDevice is null) throw new InvalidOperationException();

        audioProcessor = new AudioProcessor(captureDevice);
        audioProcessor.Start();

        repeater = new Repeater(processResult);
        repeater.Start(TimeSpan.FromSeconds(1));

        previousText = string.Empty;
        triggeredFinal = false;
    }

    private async void processResult()
    {
        Debug.Assert(audioProcessor is not null);

        var result = await audioProcessor.GetResult();
        if (result is null) return;

        // TODO: This is working perfectly, it's just not triggering the final result when it should be. All the other recognition is being filtered correctly though
        // TODO: It might also be the case that if a user doesn't talk for a while the buffer becomes too big it takes ages to process so it's probably a good idea to periodically clear the buffer if nothing is heard

        var isBlankAudio = ((result.Text.StartsWith('[') || result.Text.StartsWith('{') || result.Text.StartsWith('(')) && (result.Text.EndsWith(']') || result.Text.EndsWith('}') || result.Text.EndsWith(')'))) || result.Text == "*" || result.Text == "(";

        if (!isBlankAudio && result.Confidence < 0.5f && !triggeredFinal)
        {
            OnFinalResult?.Invoke(new SpeechResult(previousText, result.Confidence));
            audioProcessor?.ClearBuffer();
            triggeredFinal = true;
        }

        if (!isBlankAudio && result.Confidence >= 0.35f)
        {
            var text = result.Text;
            OnPartialResult?.Invoke(new SpeechResult(text, result.Confidence));
            previousText = text;
            triggeredFinal = false;
        }
    }

    public override void Teardown()
    {
        audioProcessor?.Stop();
    }
}