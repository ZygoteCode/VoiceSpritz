using System;

public class VoiceSpritzTremolo : VoiceSpritzEffect
{
    float adv;
    float sep;
    float amount;
    float sc;
    float pos;

    public VoiceSpritzTremolo(float frequencyHz = 4, float amountDb = -6)
    {
        // public Slider AddSlider(float defaultValue, float minimum, float maximum, float increment, string description)
        // Frequency (hz) => Default (4), Minimum (0), Maximum (100)
        // Amount (dB) => Default (-6), Minimum (-60), Maximum (0)

        adv = (float) Math.PI * 2 * frequencyHz / 44100;
        sep = 0 * (float) Math.PI;
        amount = (float) Math.Pow(2, amountDb / 6);
        sc = 0.5f * amount; amount = 1 - amount;
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            spl0 = spl0 * (((float)Math.Cos(pos) + 1) * sc + amount);
            spl1 = spl1 * (((float)Math.Cos(pos + sep) + 1) * sc + amount);
            pos += adv;

            theBuffer[j] = spl0;
        }
    }
}