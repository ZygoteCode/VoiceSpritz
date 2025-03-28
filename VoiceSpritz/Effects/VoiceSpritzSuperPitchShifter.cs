﻿using System;

public class VoiceSpritzSuperPitchShifter : VoiceSpritzEffect
{
    int bufsize;
    float xfade;
    int bufloc0;
    int bufloc1;
    int buffer0;
    int buffer1;
    int bufdiff;
    float pitch;

    float denorm;
    bool filter;
    float v0;
    float h01, h02, h03, h04;
    float h11, h12, h13, h14;
    float a1, a2, a3, b1, b2;
    float t0, t1;
    float drymix;
    float wetmix;
    float[] buffer = new float[64000];


    public VoiceSpritzSuperPitchShifter(float pitchAdjustCents = 0, float pitchAdjustSemitones = 5, float pitchAdjustOctaves = 0, float windowSizeMs = 50, float overlapSizeMs = 20, float wetMixDb = 0, float dryMixDb = -120, bool filterValue = true)
    {
        // Pitch adjust (cents) => Default (0), Minimum (-100), Maximum (100)
        // Pitch adjust (semitones) => Default (5), Minimum (-12), Maximum (12)
        // Pitch adjust (octaves) => Default (0), Minimum (-8), Maximum (8)
        // Window size (ms) => Default (50), Minimum (1), Maximum (200)
        // Overlap size (ms) => Default (20), Minimum (0.05), Maximum (50)
        // Wet mix (dB) => Default (0), Minimum (-120), Maximum (6)
        // Dry mix (dB) => Default (-120), Minimum (-120), Maximum (6)
        // Filter => Default (true)

        bufsize = (int)44100; // srate|0;
        xfade = 100;
        bufloc0 = 10000;
        bufloc1 = bufloc0 + bufsize + 1000;

        buffer0 = bufloc0;
        buffer1 = bufloc1;
        bufdiff = bufloc1 - bufloc0;
        pitch = 1.0f;
        filter = filterValue == true;
        int bsnew = (int)(Math.Min(windowSizeMs, 1000) * 0.001 * 44100);
        //   bsnew=(min(slider4,1000)*0.001*srate)|0;
        if (bsnew != bufsize)
        {
            bufsize = bsnew;
            v0 = buffer0 + bufsize * 0.5f;
            if (v0 > bufloc0 + bufsize)
            {
                v0 -= bufsize;
            }
        }

        xfade = (int)(overlapSizeMs * 0.001 * 44100);
        if (xfade > bsnew * 0.5)
        {
            xfade = bsnew * 0.5f;
        }

        float npitch = (float) Math.Pow(2, ((pitchAdjustSemitones + pitchAdjustCents * 0.01f) / 12 + pitchAdjustOctaves));
        if (pitch != npitch)
        {
            pitch = npitch;
            float lppos = (pitch > 1.0f) ? 1.0f / pitch : pitch;
            if (lppos < (0.1f / 44100))
            {
                lppos = 0.1f / 44100;
            }
            float r = 1.0f;
            float c = 1.0f / (float) Math.Tan(Math.PI * lppos * 0.5f);
            a1 = 1.0f / (1.0f + r * c + c * c);
            a2 = 2 * a1;
            a3 = a1;
            b1 = 2.0f * (1.0f - c * c) * a1;
            b2 = (1.0f - r * c + c * c) * a1;
            h01 = h02 = h03 = h04 = 0;
            h11 = h12 = h13 = h14 = 0;
        }

        drymix = (float)Math.Pow(2, (dryMixDb / 6));
        wetmix = (float)Math.Pow(2, (wetMixDb / 6));
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            int iv0 = (int)(v0);
            float frac0 = v0 - iv0;
            int iv02 = (iv0 >= (bufloc0 + bufsize - 1)) ? iv0 - bufsize + 1 : iv0 + 1;

            float ren0 = (buffer[iv0 + 0] * (1 - frac0) + buffer[iv02 + 0] * frac0);
            float ren1 = (buffer[iv0 + bufdiff] * (1 - frac0) + buffer[iv02 + bufdiff] * frac0);
            float vr = pitch;
            float tv, frac, tmp, tmp2;
            if (vr >= 1.0)
            {
                tv = v0;
                if (tv > buffer0) tv -= bufsize;
                if (tv >= buffer0 - xfade && tv < buffer0)
                {
                    // xfade
                    frac = (buffer0 - tv) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr > buffer0 + 1) v0 += xfade;
                }
            }
            else
            {// read pointer moving slower than write pointer
                tv = v0;
                if (tv < buffer0) tv += bufsize;
                if (tv >= buffer0 && tv < buffer0 + xfade)
                {
                    // xfade
                    frac = (tv - buffer0) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr < buffer0 + 1) v0 += xfade;
                }
            }


            if ((v0 += vr) >= (bufloc0 + bufsize)) v0 -= bufsize;

            float os0 = spl0;
            float os1 = spl1;
            if (filter && pitch > 1.0)
            {

                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }


            buffer[buffer0 + 0] = spl0; // write after reading it to avoid clicks
            buffer[buffer0 + bufdiff] = spl1;

            spl0 = ren0 * wetmix;
            spl1 = ren1 * wetmix;

            if (filter && pitch < 1.0)
            {
                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }

            spl0 += os0 * drymix;
            spl1 += os1 * drymix;

            if ((buffer0 += 1) >= (bufloc0 + bufsize)) buffer0 -= bufsize;

            theBuffer[j] = spl0;
        }
    }
}