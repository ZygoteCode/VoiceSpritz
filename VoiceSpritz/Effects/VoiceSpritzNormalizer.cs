using System;

public class VoiceSpritzNormalizer : VoiceSpritzEffect
{
    public override void Process(float[] buffer, int samplesRead)
    {
        float max = 0;

        for (int i = 0; i < samplesRead; i++)
        {
            var abs = Math.Abs(buffer[i]);

            if (abs > max)
            {
                max = abs;
            }
        }

        if (max == 0 || max > 1.0F)
        {
            return;
        }

        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] *= max;
        }
    }
}