using System.Collections.Generic;
using System.Diagnostics;
using System;

public class VoiceSpritzNewPitchShifter
{
    protected float detectedPitch; 
    protected float shiftedPitch;
    int numshifts;
    Queue<VoiceSpritzPitchShift> shifts;
    protected int currPitch;
    protected int attack;
    int numElapsed;
    protected double vibRate;
    protected double vibDepth;
    double g_time;
    protected float sampleRate;
    protected VoiceSpritzAutoTuneSettings settings;

    public VoiceSpritzNewPitchShifter(VoiceSpritzAutoTuneSettings settings, float sampleRate)
    {
        this.settings = settings;
        this.sampleRate = sampleRate;
        numshifts = 5000;
        shifts = new Queue<VoiceSpritzPitchShift>(numshifts);

        currPitch = 0;
        attack = 0;
        numElapsed = 0;
        vibRate = 4.0;
        vibDepth = 0.00;
        g_time = 0.0;
    }

    protected float SnapFactor(float freq)
    {
        float previousFrequency = 0.0f;
        float correctedFrequency = 0.0f;
        int previousNote = 0;
        int correctedNote = 0;

        for (int i = 1; i < 120; i++)
        {
            bool endLoop = false;

            foreach (int note in this.settings.AutoPitches)
            {
                if (i % 12 == note)
                {
                    previousFrequency = correctedFrequency;
                    previousNote = correctedNote;
                    correctedFrequency = (float)(8.175 * Math.Pow(1.05946309, (float)i));
                    correctedNote = i;

                    if (correctedFrequency > freq)
                    {
                        endLoop = true;
                    }

                    break;
                }
            }

            if (endLoop)
            {
                break;
            }
        }

        if (correctedFrequency == 0.0)
        {
            return 1.0f;
        }

        int destinationNote = 0;
        double destinationFrequency = 0.0;

        if (correctedFrequency - freq > freq - previousFrequency)
        {
            destinationNote = previousNote;
            destinationFrequency = previousFrequency;
        }
        else
        {
            destinationNote = correctedNote;
            destinationFrequency = correctedFrequency;
        }

        if (destinationNote != currPitch)
        {
            numElapsed = 0;
            currPitch = destinationNote;
        }

        if (attack > numElapsed)
        {
            double n = (destinationFrequency - freq) / attack * numElapsed;
            destinationFrequency = freq + n;
        }

        numElapsed++;
        return (float)(destinationFrequency / freq);
    }

    protected void UpdateShifts(float detected, float shifted, int targetNote)
    {
        if (shifts.Count >= numshifts)
        {
            shifts.Dequeue();
        }

        VoiceSpritzPitchShift shift = new VoiceSpritzPitchShift(detected, shifted, targetNote);
        Debug.WriteLine(shift);
        shifts.Enqueue(shift);
    }

    protected float AddVibrato(int nFrames)
    {
        g_time += nFrames;
        float d = (float)(Math.Sin(2 * 3.14159265358979 * vibRate * g_time / sampleRate) * vibDepth);
        return d;
    }
}

class VoiceSpritzSmbPitchShifter : VoiceSpritzNewPitchShifter
{
    public VoiceSpritzSmbPitchShifter(VoiceSpritzAutoTuneSettings settings, float sampleRate) : base(settings, sampleRate) { }

    public void ShiftPitch(float[] inputBuff, float inputPitch, float targetPitch, float[] outputBuff, int nFrames)
    {
        UpdateSettings();
        detectedPitch = inputPitch;
        float shiftFactor = 1.0f;

        if (settings.SnapMode)
        {
            if (inputPitch > 0)
            {
                shiftFactor = SnapFactor(inputPitch);
                shiftFactor += AddVibrato(nFrames);
            }
        }
        else
        {
            shiftFactor = 1.0f;

            if (inputPitch > 0 && targetPitch > 0)
            {
                shiftFactor = targetPitch / inputPitch;
            }
        }

        if (shiftFactor > 2.0)
        {
            shiftFactor = 2.0f;
        }

        if (shiftFactor < 0.5)
        {
            shiftFactor = 0.5f;
        }

        VoiceSpritzSmbPitchShift.smbPitchShift(shiftFactor, nFrames, 2048, 8, sampleRate, inputBuff, outputBuff);
        shiftedPitch = inputPitch * shiftFactor;
        UpdateShifts(detectedPitch, shiftedPitch, currPitch);
    }

    private void UpdateSettings()
    {
        vibRate = settings.VibratoRate;
        vibDepth = settings.VibratoDepth;
        attack = (int)((settings.AttackTimeMilliseconds * 441) / 1024.0);
    }
}

class VoiceSpritzPitchShift
{
    public float DetectedPitch { get; private set; }
    public float ShiftedPitch { get; private set; }
    public int DestinationNote { get; private set; }

    public VoiceSpritzPitchShift(float detected, float shifted, int destNote)
    {
        DetectedPitch = detected;
        ShiftedPitch = shifted;
        DestinationNote = destNote;
    }

    public override string ToString()
    {
        return String.Format("Detected {0:f2}Hz, shifted to {1:f2}Hz, {2}{3} ", DetectedPitch, ShiftedPitch, (VoiceSpritzNote)(DestinationNote % 12), DestinationNote / 12);
    }
}