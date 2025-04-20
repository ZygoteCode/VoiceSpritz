using NAudio.Wave;
using System;
using System.Collections.Generic;

public class VoiceSpritzWaveProvider : ISampleProvider
{
    private readonly ISampleProvider sourceProvider;
    private readonly List<VoiceSpritzEffect> effects = new List<VoiceSpritzEffect>();
    public WaveFormat WaveFormat => sourceProvider.WaveFormat;

    public VoiceSpritzWaveProvider(ISampleProvider sourceProvider)
    {
        this.sourceProvider = sourceProvider;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = sourceProvider.Read(buffer, offset, count);

        if (effects.Count > 0)
        {
            if (GlobalVariables.ShowDB)
            {
                double sum = 0;

                for (var i = 0; i < samplesRead; i++)
                {
                    double sample = buffer[i] / 32768.0;
                    sum += (sample * sample);
                }

                GlobalVariables.Decibels = (20 * Math.Log10(Math.Sqrt(sum / (samplesRead / 2)))) + 185;
            }
            else
            {
                foreach (VoiceSpritzEffect effect in effects)
                {
                    if (effect.GetType() == typeof(VoiceSpritzNoiseGate) || effect.GetType() == typeof(VoiceSpritzSuppression))
                    {
                        double sum = 0;

                        for (var i = 0; i < samplesRead; i++)
                        {
                            double sample = buffer[i] / 32768.0;
                            sum += (sample * sample);
                        }

                        GlobalVariables.Decibels = (20 * Math.Log10(Math.Sqrt(sum / (samplesRead / 2)))) + 185;
                        break;
                    }
                }
            }

            foreach (VoiceSpritzEffect effect in effects)
            {
                effect.Process(buffer, samplesRead);
            }
        }

        return samplesRead;
    }

    public void AddEffect(VoiceSpritzEffect effect)
    {
        effects.Add(effect);
    }
}