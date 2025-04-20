using System;

public class VoiceSpritzCompressor : VoiceSpritzEffect
{
    float log2db;
    float db2log;
    float attime;
    float reltime;
    float ratio;
    float cratio;
    float rundb;
    float overdb;
    float ratatcoef;
    float ratrelcoef;
    float atcoef;
    float relcoef;
    float mix;
    float gr_meter_decay;
    float thresh;
    float threshv;
    float allin;
    float cthresh;
    float cthreshv;
    float softknee = 0; // never assigned to
    float makeup;
    float makeupv;
    float autogain = 0; // never assigned to
    float rmscoef = 0; // never assigned to
    float averatio;
    float runratio;
    float maxover;
    float gr_meter;
    float runmax;
    float runave;

    public VoiceSpritzCompressor(float thresholdDb = 0, float ratioValue = 1, float gainDb = 0, float attackTimeMs = 20, float releaseTimeMs = 250, float mixPercentage = 100)
    {
        // Threshold (dB) => Default (0), Minimum (-60), Maximum (0)
        // Ratio => Default (1), Minimum (0), Maximum (3)
        // Gain (dB) => Default (0), Minimum (-20), Maximum (20)
        // Attack time (ms) => Default (20), Minimum (20), Maximum (2000)
        // Release time (ms) => Default (250), Minimum (20), Maximum (1000)
        // Mix (%) => Default (100), Minimum (0), Maximum (100)

        log2db = 8.6858896380650365530225783783321f; // 20 / ln(10)
        db2log = 0.11512925464970228420089957273422f; // ln(10) / 20 
        attime = 0.010f;
        reltime = 0.100f;
        ratio = 0;
        cratio = 0;
        rundb = 0;
        overdb = 0;
        ratatcoef = (float)Math.Exp(-1 / (0.00001f * 44100));
        ratrelcoef = (float)Math.Exp(-1 / (0.5f * 44100));
        atcoef = (float)Math.Exp(-1 / (attime * 44100));
        relcoef = (float)Math.Exp(-1 / (reltime * 44100));
        mix = 1;
        gr_meter = 1;
        gr_meter_decay = (float)Math.Exp(1 / (1 * 44100));
        thresh = thresholdDb;
        threshv = (float)Math.Exp(thresh * db2log);
        ratio = (ratioValue == 0 ? 4 : (ratioValue == 1 ? 8 : (ratioValue == 2 ? 12 : (ratioValue == 3 ? 20 : 20))));
        if (ratioValue == 4) { allin = 1; cratio = 20; } else { allin = 0; cratio = ratio; }
        cthresh = (softknee != 0) ? (thresh - 3) : thresh;
        cthreshv = (float)Math.Exp(cthresh * db2log);
        makeup = gainDb;
        makeupv = (float)Math.Exp((makeup + autogain) * db2log);
        attime = attackTimeMs / 1000000;
        reltime = releaseTimeMs / 1000;
        atcoef = (float)Math.Exp(-1 / (attime * 44100));
        relcoef = (float)Math.Exp(-1 / (reltime * 44100));
        mix = mixPercentage / 100;
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            float ospl0 = spl0;
            float ospl1 = spl1;
            float aspl0 = Math.Abs(spl0);
            float aspl1 = Math.Abs(spl1);
            float maxspl = Math.Max(aspl0, aspl1);
            maxspl = maxspl * maxspl;
            runave = maxspl + rmscoef * (runave - maxspl);
            float det = (float) Math.Sqrt(Math.Max(0, runave));

            overdb = 2.08136898f * (float)Math.Log(det / cthreshv) * log2db;
            overdb = Math.Max(0, overdb);

            if (overdb - rundb > 5) averatio = 4;

            if (overdb > rundb)
            {
                rundb = overdb + atcoef * (rundb - overdb);
                runratio = averatio + ratatcoef * (runratio - averatio);
            }
            else
            {
                rundb = overdb + relcoef * (rundb - overdb);
                runratio = averatio + ratrelcoef * (runratio - averatio);
            }
            overdb = rundb;
            averatio = runratio;

            if (allin != 0)
            {
                cratio = 12 + averatio;
            }
            else
            {
                cratio = ratio;
            }

            float gr = -overdb * (cratio - 1) / cratio;
            float grv = (float) Math.Exp(gr * db2log);

            runmax = maxover + relcoef * (runmax - maxover);  // highest peak for setting att/rel decays in reltime
            maxover = runmax;

            if (grv < gr_meter) gr_meter = grv; else { gr_meter *= gr_meter_decay; if (gr_meter > 1) gr_meter = 1; };

            spl0 *= grv * makeupv * mix;
            spl1 *= grv * makeupv * mix;

            spl0 += ospl0 * (1 - mix);
            spl1 += ospl1 * (1 - mix);

            theBuffer[j] = spl0;
        }
    }
}