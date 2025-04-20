using System;

public class VoiceSpritzWaveShaper : VoiceSpritzEffect
{
    float log2db;
    float db2log;
    float pi;
    float halfpi;
    float pt;
    float nt;
    float pl;
    float nl;
    float mixa;
    float mixb;
    float drivea;
    float mixa1;
    float drivea1;
    float drivea2;
    float mixb1;
    float pts;
    float nts;
    float ptt;
    float ntt;
    float ptsv;
    float ntsv;
    float drive = 0; // never assigned to
    float wet0;
    float wet1;
    float diff;
    float mult;


    public VoiceSpritzWaveShaper(float positiveThresholdDb = 0, float negativeThresholdDb = 0, float positiveNonlinearity = 1, float negativeNonlinearity = 1, float positiveKnee = 0, float negativeKnee = 0, float modA = 0, float modB = 0)
    {
        // public Slider AddSlider(float defaultValue, float minimum, float maximum, float increment, string description)
        // Positive threshold (dB) => Default (0), Minimum (-60), Maximum (0)
        // Negative threshold (dB) => Default (0), Minimum (-60), Maximum (0)
        // Positive nonlinearity => Default (1), Minimum (1), Maximum (2)
        // Negative nonlinearity => Default (1), Minimum (1), Maximum (2)
        // Positive knee => Default (0), Minimum (0), Maximum (6)
        // Negative knee => Default (0), Minimum (0), Maximum (6)
        // Mod A => Default (0), Minimum (0), Maximum (100)
        // Mod B => Default (0), Minimum (0), Maximum (100)

        log2db = 8.6858896380650365530225783783321f; // 20 / ln(10)
        db2log = 0.11512925464970228420089957273422f; // ln(10) / 20 
        pi = 3.1415926535f;
        halfpi = pi / 2;
        pt = positiveThresholdDb;
        nt = negativeThresholdDb;
        pl = positiveNonlinearity - 1;
        nl = negativeNonlinearity - 1;
        mixa = modA / 100;
        mixb = modB / 100;
        drivea = 1;
        mixa1 = 1 - mixa;
        drivea1 = 1 / (1 - (drivea / 2));
        drivea2 = drive / 2;
        mixb1 = 1 - mixb;
        pts = positiveKnee;
        nts = negativeKnee;
        ptt = pt - pts;
        ntt = nt - nts;

        ptsv = (float)Math.Exp(ptt * db2log);
        ntsv = (float)-Math.Exp(ntt * db2log);
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            if (mixa > 0)
            {
                wet0 = drivea1 * spl0 * (1 - Math.Abs(spl0 * drivea2));
                wet1 = drivea1 * spl1 * (1 - Math.Abs(spl1 * drivea2));
                spl0 = mixa1 * spl0 + (mixa) * wet0;
                spl1 = mixa1 * spl1 + (mixa) * wet1;
            }

            if (mixb > 0)
            {
                wet0 = (float)Math.Sin(spl0 * halfpi);
                wet1 = (float)Math.Sin(spl1 * halfpi);
                spl0 = mixb1 * spl0 + (mixb) * wet0;
                spl1 = mixb1 * spl1 + (mixb) * wet1;
            }

            float db0 = (float)Math.Log(Math.Abs(spl0)) * log2db;
            float db1 = (float)Math.Log(Math.Abs(spl1)) * log2db;

            if (spl0 > ptsv)
            {
                diff = Math.Max(Math.Min((db0 - ptt), 0), pts);
                if (pts == 0) mult = 0; else mult = diff / pts;
                spl0 = ptsv + ((spl0 - ptsv) / (1 + (pl * mult)));
            }
            if (spl0 < ntsv)
            {
                diff = Math.Max(Math.Min((db0 - ntt), 0), nts);
                if (nts == 0) mult = 0; else mult = diff / nts;
                spl0 = ntsv + ((spl0 - ntsv) / (1 + (nl * mult)));
            }
            if (spl1 > ptsv)
            {
                diff = Math.Max(Math.Min((db1 - ptt), 0), pts);
                if (pts == 0) mult = 0; else mult = diff / pts;
                spl1 = ptsv + ((spl1 - ptsv) / (1 + (pl * mult)));
            }
            if (spl1 < ntsv)
            {
                diff = Math.Max(Math.Min((db1 - ntt), 0), nts);
                if (nts == 0) mult = 0; else mult = diff / nts;
                spl1 = ntsv + ((spl1 - ntsv) / (1 + (nl * mult)));
            }

            theBuffer[j] = spl0;
        }
    }
}