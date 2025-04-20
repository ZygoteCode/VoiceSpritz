using System;

public class VoiceSpritzSuperCompressor : VoiceSpritzEffect
{
    float log2db;
    float db2log;
    //float i;
    float attime;
    float reltime;
    float rmstime;
    //float maxover;
    float ratio;
    float cratio;
    float rundb;
    float overdb;
    float atcoef;
    float relcoef;
    float rmscoef;
    float leftright;
    //float latvert;
    float thresh;
    float threshv;
    float bias;
    float cthresh;
    float cthreshv;
    float makeup;
    float makeupv;
    float timeconstant;
    float agc;
    float aspl0;
    float aspl1;
    float runave;
    float dcoffset = 0; // never assigned to

    public VoiceSpritzSuperCompressor(float thresholdDb = 0, float biasValue = 70, float makeupGainDb = 0, VoiceSpritzSuperCompressorAGC agcValue = VoiceSpritzSuperCompressorAGC.LeftRight, float timeConstantValue = 1, float levelDetectorRMSWindow = 100)
    {
        // Threshold (dB) => Default (0), Minimum (-60), Maximum (0)
        // Bias => Default (70), Minimum (0.1), Maximum (100)
        // Makeup gain (dB) => Default (0), Minimum (-30), Maximum (30)
        // AGC => LeftRight, LateralVertical
        // Time constant => Default (1), Minimum (1), Maximum (6)
        // Level detector RMS window => Default (100), Minimum (1), Maximum (10000)

        log2db = 8.6858896380650365530225783783321f; // 20 / ln(10)
        db2log = 0.11512925464970228420089957273422f; // ln(10) / 20 
                                                      //i = 0;
        attime = 0.0002f; //200us
        reltime = 0.300f; //300ms
        rmstime = 0.000050f; //50us
                             //maxover = 0;
        ratio = 0;
        cratio = 0;
        rundb = 0;
        overdb = 0;
        atcoef = (float)Math.Exp(-1 / (attime * 44100));
        relcoef = (float)Math.Exp(-1 / (reltime * 44100));
        rmscoef = (float)Math.Exp(-1 / (rmstime * 44100));
        leftright = 0;
        //latvert = 1;
        thresh = thresholdDb;
        threshv = (float)Math.Exp(thresh * db2log);
        ratio = 20;
        bias = 80 * biasValue / 100;
        cthresh = thresh - bias;
        cthreshv = (float)Math.Exp(cthresh * db2log);
        makeup = makeupGainDb;
        makeupv = (float)Math.Exp(makeup * db2log);
        agc = agcValue == VoiceSpritzSuperCompressorAGC.LeftRight ? 0 : 1;
        timeconstant = timeConstantValue;
        if (timeconstant == 1)
        {
            attime = 0.0002f;
            reltime = 0.300f;
        }
        if (timeconstant == 2)
        {
            attime = 0.0002f;
            reltime = 0.800f;
        }
        if (timeconstant == 3)
        {
            attime = 0.0004f;
            reltime = 2.000f;
        }
        if (timeconstant == 4)
        {
            attime = 0.0008f;
            reltime = 5.000f;
        }
        if (timeconstant == 5)
        {
            attime = 0.0002f;
            reltime = 10.000f;
        }
        if (timeconstant == 6)
        {
            attime = 0.0004f;
            reltime = 25.000f;
        }
        atcoef = (float)Math.Exp(-1 / (attime * 44100));
        relcoef = (float)Math.Exp(-1 / (reltime * 44100));

        rmstime = levelDetectorRMSWindow / 1000000;
        rmscoef = (float)Math.Exp(-1 / (rmstime * 44100));
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            if (agc == leftright)
            {
                aspl0 = Math.Abs(spl0);
                aspl1 = Math.Abs(spl1);
            }
            else
            {
                aspl0 = Math.Abs(spl0 + spl1) / 2;
                aspl1 = Math.Abs(spl0 - spl1) / 2;
            }

            float maxspl = Math.Max(aspl0, aspl1);
            maxspl = maxspl * maxspl;

            runave = maxspl + rmscoef * (runave - maxspl);
            float det = (float) Math.Sqrt(Math.Max(0, runave));

            overdb = 2.08136898f * (float) Math.Log(det / threshv) * log2db;
            overdb = Math.Max(0, overdb);

            if (overdb > rundb)
            {
                rundb = overdb + atcoef * (rundb - overdb);
            }
            else
            {
                rundb = overdb + relcoef * (rundb - overdb);
            }
            overdb = Math.Max(rundb, 0);

            if (bias == 0)
            {
                cratio = ratio;
            }
            else
            {
                cratio = 1 + (ratio - 1) * (float) Math.Sqrt((overdb + dcoffset) / (bias + dcoffset));
            }
            //slider7 = cratio;

            float gr = -overdb * (cratio - 1) / cratio;
            //slider8 = -gr;
            float grv = (float) Math.Exp(gr * db2log);

            if (agc == leftright)
            {
                spl0 *= grv * makeupv;
                spl1 *= grv * makeupv;
            }
            else
            {
                float sav0 = (spl0 + spl1) * grv;
                float sav1 = (spl0 - spl1) * grv;
                spl0 = makeupv * (sav0 + sav1) * 0.5f;
                spl1 = makeupv * (sav0 - sav1) * 0.5f;
            }

            theBuffer[j] = spl0;
        }
    }
}

public enum VoiceSpritzSuperCompressorAGC
{
    LeftRight,
    LateralVertical
}