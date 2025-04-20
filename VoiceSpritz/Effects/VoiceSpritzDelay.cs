using System;

public class VoiceSpritzDelay : VoiceSpritzEffect
{
    float delaypos;
    float odelay;
    float delaylen;
    float wetmix;
    float drymix;
    float wetmix2;
    float drymix2;
    float rspos;
    int rspos2;
    float drspos;
    int tpos;
    float[] buffer = new float[500000];

    public VoiceSpritzDelay(float delayMs = 300, float feedbackDb = -5, float mixInDb = 0, float outputWetDb = -6, float outputDryDb = 0)
    {
        // Delay (ms) => Default (300), Minimum (0), Maximum (4000)
        // Feedback (dB) => Default (-5), Minimum (-120), Maximum (6)
        // Mix in (dB) => Default (0), Minimum (-120), Maximum (6)
        // Output wet (dB) => Default (-6), Minimum (-120), Maximum (6)
        // Output dry (dB) => Default (0), Minimum (-120), Maximum (6)

        delaypos = 0;
        odelay = delaylen;
        delaylen = Math.Min(delayMs * 44100 / 1000, 500000);
        wetmix = (float)Math.Pow(2, (feedbackDb / 6));
        drymix = (float)Math.Pow(2, (mixInDb / 6));
        wetmix2 = (float)Math.Pow(2, (outputWetDb / 6));
        drymix2 = (float)Math.Pow(2, (outputDryDb / 6));
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            int dpint = (int)delaypos * 2;
            float os1 = buffer[dpint + 0];
            float os2 = buffer[dpint + 1];

            buffer[dpint + 0] = Math.Min(Math.Max(spl0 * drymix + os1 * wetmix, -4), 4);
            buffer[dpint + 1] = Math.Min(Math.Max(spl1 * drymix + os2 * wetmix, -4), 4);

            if ((delaypos += 1) >= delaylen)
            {
                delaypos = 0;
            }

            spl0 = spl0 * drymix2 + os1 * wetmix2;
            spl1 = spl1 * drymix2 + os2 * wetmix2;

            theBuffer[j] = spl0;
        }
    }
}