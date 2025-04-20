using System;

public class VoiceSpritzChorus : VoiceSpritzEffect
{
    int bpos;
    float numvoices;
    float choruslen;
    float rateadj;
    float csize;
    int bufofs;
    float wetmix;
    float drymix;
    float[] buffer = new float[1000000];

    public VoiceSpritzChorus(float chorusLengthMs = 15, float numberOfVoices = 1, float rateHz = 0.5F, float pitchFudgeFactor = 0.7F, float wetMixDb = -6, float dryMixDb = -6)
    {
        // Chorus Length (ms) => Default (15), Minimum (1), Maximum (250)
        // Number of voices => Default (1), Minimum (1), Maximum (8)
        // Rate (hz) => Default (0.5), Minimum (0.1), Maximum (16.0)
        // Pitch fudge factor => Default (0.7), Minimum (0.1), Maximum (1.0)
        // Wet mix (dB) => Default (-6), Minimum (-100), Maximum (12)
        // Dry mix (dB) => Default (-6), Minimum (-100), Maximum (12)

        bpos = 0;
        numvoices = Math.Min(16, Math.Max(numberOfVoices, 1));
        choruslen = chorusLengthMs * 44100 * 0.001f;

        for (int i = 0; i < numvoices; i++)
        {
            buffer[i] = (i + 1) / numvoices * (float) Math.PI;
        }

        bufofs = 16384;

        csize = choruslen / numvoices * pitchFudgeFactor;

        rateadj = rateHz * 2 * (float) Math.PI / 44100;
        wetmix = (float) Math.Pow(2, wetMixDb / 6);
        drymix = (float) Math.Pow(2, dryMixDb / 6);
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            if (bpos >= choruslen)
            {
                bpos = 0;
            }
            float os0 = spl0;

            // calculate new sample based on numvoices
            spl0 = spl0 * drymix;
            float vol = wetmix / numvoices;

            for (int i = 0; i < numvoices; i++)
            {
                float tpos = bpos - (0.5f + 0.49f * (float) (Math.Sin(Math.PI * (buffer[i] += rateadj)) / (Math.PI * buffer[i]))) * (i + 1) * csize;

                if (tpos < 0) tpos += choruslen;
                if (tpos > choruslen) tpos -= choruslen;
                float frac = tpos - (int)tpos;
                float tpos2 = (tpos >= choruslen - 1) ? 0 : tpos + 1;

                spl0 += (buffer[bufofs + (int)tpos] * (1 - frac) + buffer[bufofs + (int)tpos2] * frac) * vol;
            }

            buffer[bufofs + bpos] = os0;
            bpos += 1;

            spl1 = spl0;

            theBuffer[j] = spl0;
        }
    }
}