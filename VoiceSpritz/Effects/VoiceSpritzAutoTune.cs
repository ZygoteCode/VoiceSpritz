using System;

public class VoiceSpritzAutoTune : VoiceSpritzEffect
{
    private float previousPitch;
    private int release;

    private VoiceSpritzSmbPitchShifter pitchShifter;
    private VoiceSpritzIPitchDetector pitchDetector;
    private VoiceSpritzAutoTuneSettings autoTuneSettings;

    public VoiceSpritzAutoTune(VoiceSpritzAutoTuneSettings newSettings)
    {
        autoTuneSettings = newSettings;
        pitchShifter = new VoiceSpritzSmbPitchShifter(autoTuneSettings, 44100);
        pitchDetector = new VoiceSpritzAutoCorrelator(44100);
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        float pitch = pitchDetector.DetectPitch(buffer, samplesRead);

        if (pitch == 0f && release < 1)
        {
            pitch = previousPitch;
            release++;
        }
        else
        {
            previousPitch = pitch;
            release = 0;
        }

        int midiNoteNumber = 40;
        float targetPitch = (float)(8.175 * Math.Pow(1.05946309, midiNoteNumber));
        pitchShifter.ShiftPitch(buffer, pitch, targetPitch, buffer, samplesRead);
    }
}