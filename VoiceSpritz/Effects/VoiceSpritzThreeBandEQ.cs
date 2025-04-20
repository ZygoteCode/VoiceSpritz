using System;

public class VoiceSpritzThreeBandEQ : VoiceSpritzEffect
{
    float db2log;
    float pi;
    float halfpi;
    float halfpiscaled;
    float mixl;
    float mixm;
    float mixh;
    float al;
    float ah;
    float mixl1;
    float mixm1;
    float mixh1;
    float gainl;
    float gainm;
    float gainh;
    float mixlg;
    float mixmg;
    float mixhg;
    float mixlg1;
    float mixmg1;
    float mixhg1;
    float lfl;
    float lfh;
    float rfh;
    float rfl;

    public VoiceSpritzThreeBandEQ(float lowFrequenciesDrivePercent = 0, float lowFrequenciesGainDb = 0, float middleFrequenciesDrivePercent = 0, float middleFrequenciesGainDb = 0, float highFrequenciesDrivePercent = 0, float highFrequenciesGainDb = 0, float lowMiddleFrequencyHz = 240, float middleHighFrequencyHz = 2400)
    {
        // Low frequencies drive (%) => Default (0), Minimum (0), Maximum (100)
        // Low frequencies gain (dB) => Default (0), Minimum (-12), Maximum (12)
        // Middle frequencies drive (%) => Default (0), Minimum (0), Maximum (100)
        // Middle frequencies gain (dB) => Default (0), Minimum (-12), Maximum (12)
        // High frequencies drive (%) => Default (0), Minimum (0), Maximum (100)
        // High frequencies gain (dB) => Default (0), Minimum (-12), Maximum (12)
        // Low-middle frequency (Hz) => Default (240), Minimum (60), Maximum (680)
        // Middle-high frequency (Hz) => Default (2400), Minimum (720), Maximum (12000)

        db2log = 0.11512925464970228420089957273422f; // ln(10) / 20 
        pi = 3.1415926535f;
        halfpi = pi / 2;
        halfpiscaled = halfpi * 1.41254f;
        mixl = lowFrequenciesDrivePercent / 100;
        mixm = middleFrequenciesDrivePercent / 100;
        mixh = highFrequenciesDrivePercent / 100;
        al = Math.Min(lowMiddleFrequencyHz, 44100) / 44100;
        ah = Math.Max(Math.Min(middleHighFrequencyHz, 44100) / 44100, al);
        mixl1 = 1 - mixl;
        mixm1 = 1 - mixm;
        mixh1 = 1 - mixh;
        gainl = (float)Math.Exp(lowFrequenciesGainDb * db2log);
        gainm = (float)Math.Exp(middleFrequenciesGainDb * db2log);
        gainh = (float)Math.Exp(highFrequenciesGainDb * db2log);
        mixlg = mixl * gainl;
        mixmg = mixm * gainm;
        mixhg = mixh * gainh;
        mixlg1 = mixl1 * gainl;
        mixmg1 = mixm1 * gainm;
        mixhg1 = mixh1 * gainh;
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            float dry0 = spl0;
            float dry1 = spl1;

            float lf1h = lfh;
            lfh = dry0 + lfh - ah * lf1h;
            float high_l = dry0 - lfh * ah;

            float lf1l = lfl;
            lfl = dry0 + lfl - al * lf1l;
            float low_l = lfl * al;

            float mid_l = dry0 - low_l - high_l;

            float rf1h = rfh;
            rfh = dry1 + rfh - ah * rf1h;
            float high_r = dry1 - rfh * ah;

            float rf1l = rfl;
            rfl = dry1 + rfl - al * rf1l;
            float low_r = rfl * al;

            float mid_r = dry1 - low_r - high_r;

            float wet0_l = mixlg * (float)Math.Sin(low_l * halfpiscaled);
            float wet0_m = mixmg * (float)Math.Sin(mid_l * halfpiscaled);
            float wet0_h = mixhg * (float)Math.Sin(high_l * halfpiscaled);
            float wet0 = (wet0_l + wet0_m + wet0_h);

            float dry0_l = low_l * mixlg1;
            float dry0_m = mid_l * mixmg1;
            float dry0_h = high_l * mixhg1;
            dry0 = (dry0_l + dry0_m + dry0_h);

            float wet1_l = mixlg * (float)Math.Sin(low_r * halfpiscaled);
            float wet1_m = mixmg * (float)Math.Sin(mid_r * halfpiscaled);
            float wet1_h = mixhg * (float)Math.Sin(high_r * halfpiscaled);
            float wet1 = (wet1_l + wet1_m + wet1_h);

            float dry1_l = low_r * mixlg1;
            float dry1_m = mid_r * mixmg1;
            float dry1_h = high_r * mixhg1;
            dry1 = (dry1_l + dry1_m + dry1_h);

            spl0 = dry0 + wet0;
            spl1 = dry1 + wet1;

            theBuffer[j] = spl0;
        }
    }
}