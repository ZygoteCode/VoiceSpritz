using System;

public class VoiceSpritzFlanger : VoiceSpritzEffect
{
    float odelay;
    float delaylen;
    float wetmix;
    float wetmix2;
    float drymix2;
    float dppossc;
    float dpbacksc;
    float dppos;
    float dpback;
    float dpint;
    float delaypos;
    float[] buffer = new float[500000];


    public VoiceSpritzFlanger(float lengthMs = 6, float feedbackDb = -120, float wetMixDb = -6, float dryMixDb = -6, float rateHz = 0.6F)
    {
        // Length (ms) => Default (6), Minimum (0), Maximum (200)
        // Feedback (dB) => Default (-120), Minimum (-120), Maximum (6)
        // Wet mix (dB) => Default (-6), Minimum (-120), Maximum (6)
        // Dry mix (dB) => Default (-6), Minimum (-120), Maximum (6)
        // Rate (hz) => Default (0.6), Minimum (0.001), Maximum (100)

        delaypos = 0;
        odelay = delaylen;
        delaylen = Math.Min(lengthMs * 44100 / 1000, 500000);
        //if (odelay != delaylen) freembuf(delaylen*2);

        wetmix = (float)Math.Pow(2, feedbackDb / 6);
        wetmix2 = (float)Math.Pow(2, wetMixDb / 6);
        drymix2 = (float)Math.Pow(2, dryMixDb / 6);
        dppossc = 2 * (float)Math.PI * rateHz / 44100;
        dpbacksc = delaylen * 0.5f - 1;
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            dppos = dppos + dppossc;
            dpback = ((float)Math.Sin(dppos) + 1) * dpbacksc;
            dpint = delaypos - dpback - 1;
            if (dpint < 0) dpint += delaylen;

            dpint *= 2;

            float os1 = buffer[(int)dpint + 0];
            float os2 = buffer[(int)dpint + 1];

            dpint = delaypos * 2;

            buffer[(int)dpint + 0] = spl0 + os1 * wetmix;
            buffer[(int)dpint + 1] = spl1 + os2 * wetmix;
            delaypos += 1;
            if (delaypos >= delaylen) delaypos = 0;

            spl0 = spl0 * drymix2 + os1 * wetmix2;
            spl1 = spl1 * drymix2 + os2 * wetmix2;

            theBuffer[j] = spl0;
        }
    }
}