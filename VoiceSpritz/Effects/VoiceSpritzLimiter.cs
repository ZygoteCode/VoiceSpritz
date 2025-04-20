using System;

public class VoiceSpritzLimiter : VoiceSpritzEffect
{
    float log2db;
    float db2log;
    float thresh;
    float threshdb;
    float ceiling;
    float ceildb;
    float makeup;
    float makeupdb;
    float sc;
    float scv;
    float sccomp;
    float peakdb;
    float peaklvl;
    float scratio;
    float scmult;

    public VoiceSpritzLimiter(float thresholdDb = 0, float ceilingDb = -0.1F, float softClipDb = 2.0F, float softClipRatio = 10)
    {
        // public Slider AddSlider(float defaultValue, float minimum, float maximum, float increment, string description)
        // Threshold (dB) => Default (0), Minimum (-30), Maximum (0)
        // Ceiling (dB) => Default (-0.1), Minimum (-20.0), Maximum (0)
        // Soft clip (dB) => Default (2.0), Minimum (0), Maximum (6.0)
        // Soft clip ratio => Default (10), Minimum (3), Maximum (20)

        // pi = 3.1415926535;
        log2db = 8.6858896380650365530225783783321f; // 20 / ln(10)
        db2log = 0.11512925464970228420089957273422f; // ln(10) / 20 
        thresh = (float)Math.Exp(thresholdDb * db2log);
        threshdb = thresholdDb;
        ceiling = (float)Math.Exp(ceilingDb * db2log);
        ceildb = ceilingDb;
        makeup = (float)Math.Exp((ceildb - threshdb) * db2log);
        makeupdb = ceildb - threshdb;
        sc = -softClipDb;
        scv = (float)Math.Exp(sc * db2log);
        sccomp = (float)Math.Exp(-sc * db2log);
        peakdb = ceildb + 25;
        peaklvl = (float)Math.Exp(peakdb * db2log);
        scratio = softClipRatio;
        scmult = Math.Abs((ceildb - sc) / (peakdb - sc));
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            float peak = Math.Max(Math.Abs(spl0), Math.Abs(spl1));
            spl0 = spl0 * makeup;
            spl1 = spl1 * makeup;
            float sign0 = Math.Sign(spl0);
            float sign1 = Math.Sign(spl1);
            float abs0 = Math.Abs(spl0);
            float abs1 = Math.Abs(spl1);
            float overdb0 = 2.08136898f * (float)Math.Log(abs0) * log2db - ceildb;
            float overdb1 = 2.08136898f * (float)Math.Log(abs1) * log2db - ceildb;
            if (abs0 > scv)
            {
                spl0 = sign0 * (scv + (float)Math.Exp(overdb0 * scmult) * db2log);
            }
            if (abs1 > scv)
            {
                spl1 = sign1 * (scv + (float)Math.Exp(overdb1 * scmult) * db2log);
            }

            spl0 = Math.Min(ceiling, Math.Abs(spl0)) * Math.Sign(spl0);
            spl1 = Math.Min(ceiling, Math.Abs(spl1)) * Math.Sign(spl1);

            theBuffer[j] = spl0;
        }
    }
}