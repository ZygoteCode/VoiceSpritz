using System;

public class VoiceSpritzSmbPitchShift
{
    public const double M_PI_VAL = 3.14159265358979323846;

    static float[] gInFIFO = new float[8192];
    static float[] gOutFIFO = new float[8192];
    static float[] gFFTworksp = new float[2 * 8192];
    static float[] gLastPhase = new float[8192 / 2 + 1];
    static float[] gSumPhase = new float[8192 / 2 + 1];
    static float[] gOutputAccum = new float[2 * 8192];
    static float[] gAnaFreq = new float[8192];
    static float[] gAnaMagn = new float[8192];
    static float[] gSynFreq = new float[8192];
    static float[] gSynMagn = new float[8192];
    static int gRover = 0;

    public static void smbPitchShift(float pitchShift, int numSampsToProcess, int fftFrameSize, int osamp, float sampleRate, float[] indata, float[] outdata)
    {
        double magn, phase, tmp, window, real, imag;
        double freqPerBin, expct;
        int i, k, qpd, index, inFifoLatency, stepSize, fftFrameSize2;
        fftFrameSize2 = fftFrameSize / 2;
        stepSize = fftFrameSize / osamp;
        freqPerBin = sampleRate / (double)fftFrameSize;
        expct = 2.0 * M_PI_VAL * (double)stepSize / (double)fftFrameSize;
        inFifoLatency = fftFrameSize - stepSize;

        if (gRover == 0)
        {
            gRover = inFifoLatency;
        }

        for (i = 0; i < numSampsToProcess; i++)
        {
            gInFIFO[gRover] = indata[i];
            outdata[i] = gOutFIFO[gRover - inFifoLatency];
            gRover++;

            if (gRover >= fftFrameSize)
            {
                gRover = inFifoLatency;

                for (k = 0; k < fftFrameSize; k++)
                {
                    window = -.5 * Math.Cos(2.0 * M_PI_VAL * k / fftFrameSize) + .5;
                    gFFTworksp[2 * k] = (float)(gInFIFO[k] * window);
                    gFFTworksp[2 * k + 1] = 0.0f;
                }

                smbFft(gFFTworksp, fftFrameSize, -1);

                for (k = 0; k <= fftFrameSize2; k++)
                {
                    real = gFFTworksp[2 * k];
                    imag = gFFTworksp[2 * k + 1];
                    magn = 2.0 * Math.Sqrt(real * real + imag * imag);
                    phase = Math.Atan2(imag, real);
                    tmp = phase - gLastPhase[k];
                    gLastPhase[k] = (float)phase;
                    tmp -= k * expct;
                    qpd = (int)(tmp / M_PI_VAL);
                    if (qpd >= 0) qpd += (qpd & 1);
                    else qpd -= (qpd & 1);
                    tmp -= M_PI_VAL * (double)qpd;
                    tmp = osamp * tmp / (2.0 * M_PI_VAL);
                    tmp = k * freqPerBin + tmp * freqPerBin;
                    gAnaMagn[k] = (float)magn;
                    gAnaFreq[k] = (float)tmp;

                }

                Array.Clear(gSynMagn, 0, fftFrameSize);
                Array.Clear(gSynFreq, 0, fftFrameSize);

                for (k = 0; k <= fftFrameSize2; k++)
                {
                    index = (int)(k * pitchShift);

                    if (index <= fftFrameSize2)
                    {
                        gSynMagn[index] += gAnaMagn[k];
                        gSynFreq[index] = gAnaFreq[k] * pitchShift;
                    }
                }

                for (k = 0; k <= fftFrameSize2; k++)
                {
                    magn = gSynMagn[k];
                    tmp = gSynFreq[k];
                    tmp -= k * freqPerBin;
                    tmp /= freqPerBin;
                    tmp = 2.0 * M_PI_VAL * tmp / osamp;
                    tmp += k * expct;
                    gSumPhase[k] += (float)tmp;
                    phase = gSumPhase[k];
                    gFFTworksp[2 * k] = (float)(magn * Math.Cos(phase));
                    gFFTworksp[2 * k + 1] = (float)(magn * Math.Sin(phase));
                }

                for (k = fftFrameSize + 2; k < 2 * fftFrameSize; k++)
                {
                    gFFTworksp[k] = 0.0f;
                }

                smbFft(gFFTworksp, fftFrameSize, 1);

                for (k = 0; k < fftFrameSize; k++)
                {
                    window = -.5 * Math.Cos(2.0 * M_PI_VAL * (double)k / (double)fftFrameSize) + .5;
                    gOutputAccum[k] += (float)(2.0 * window * gFFTworksp[2 * k] / (fftFrameSize2 * osamp));
                }

                for (k = 0; k < stepSize; k++)
                {
                    gOutFIFO[k] = gOutputAccum[k];
                }

                int destOffset = 0;
                int sourceOffset = stepSize;
                Array.Copy(gOutputAccum, sourceOffset, gOutputAccum, destOffset, fftFrameSize);

                for (k = 0; k < inFifoLatency; k++)
                {
                    gInFIFO[k] = gInFIFO[k + stepSize];
                }
            }
        }
    }

    public static void smbFft(float[] fftBuffer, int fftFrameSize, int sign)
    {
        float wr, wi, arg, temp;
        int p1, p2;
        float tr, ti, ur, ui;
        int p1r, p1i, p2r, p2i;
        int i, bitm, j, le, le2, k;
        int fftFrameSize2 = fftFrameSize * 2;

        for (i = 2; i < fftFrameSize2 - 2; i += 2)
        {
            for (bitm = 2, j = 0; bitm < fftFrameSize2; bitm <<= 1)
            {
                if ((i & bitm) != 0)
                {
                    j++;
                }

                j <<= 1;
            }

            if (i < j)
            {
                p1 = i; p2 = j;
                temp = fftBuffer[p1];
                fftBuffer[p1++] = fftBuffer[p2];
                fftBuffer[p2++] = temp;
                temp = fftBuffer[p1];
                fftBuffer[p1] = fftBuffer[p2];
                fftBuffer[p2] = temp;
            }
        }

        int kmax = (int)(Math.Log(fftFrameSize) / Math.Log(2.0) + 0.5);

        for (k = 0, le = 2; k < kmax; k++)
        {
            le <<= 1;
            le2 = le >> 1;
            ur = 1.0f;
            ui = 0.0f;
            arg = (float)(M_PI_VAL / (le2 >> 1));
            wr = (float)Math.Cos(arg);
            wi = (float)(sign * Math.Sin(arg));

            for (j = 0; j < le2; j += 2)
            {
                p1r = j; p1i = p1r + 1;
                p2r = p1r + le2; p2i = p2r + 1;

                for (i = j; i < fftFrameSize2; i += le)
                {
                    float p2rVal = fftBuffer[p2r];
                    float p2iVal = fftBuffer[p2i];
                    tr = p2rVal * ur - p2iVal * ui;
                    ti = p2rVal * ui + p2iVal * ur;
                    fftBuffer[p2r] = fftBuffer[p1r] - tr;
                    fftBuffer[p2i] = fftBuffer[p1i] - ti;
                    fftBuffer[p1r] += tr;
                    fftBuffer[p1i] += ti;
                    p1r += le;
                    p1i += le;
                    p2r += le;
                    p2i += le;
                }

                tr = ur * wr - ui * wi;
                ui = ur * wi + ui * wr;
                ur = tr;
            }
        }
    }
}