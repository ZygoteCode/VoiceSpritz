using System;

public class VoiceSpritzFftPitchDetector : VoiceSpritzIPitchDetector
{
    private float sampleRate;
    private float[] fftBuffer;
    private float[] prevBuffer;

    public VoiceSpritzFftPitchDetector(float sampleRate)
    {
        this.sampleRate = sampleRate;
    }

    private float HammingWindow(int n, int N)
    {
        return 0.54f - 0.46f * (float)Math.Cos((2 * Math.PI * n) / (N - 1));
    }

    public float DetectPitch(float[] buffer, int inFrames)
    {
        Func<int, int, float> window = HammingWindow;

        if (prevBuffer == null)
        {
            prevBuffer = new float[inFrames];
        }

        int frames = inFrames * 2;

        if (fftBuffer == null)
        {
            fftBuffer = new float[frames * 2];
        }

        for (int n = 0; n < frames; n++)
        {
            if (n < inFrames)
            {
                fftBuffer[n * 2] = prevBuffer[n] * window(n, frames);
                fftBuffer[n * 2 + 1] = 0;
            }
            else
            {
                fftBuffer[n * 2] = buffer[n - inFrames] * window(n, frames);
                fftBuffer[n * 2 + 1] = 0;
            }
        }

        VoiceSpritzSmbPitchShift.smbFft(fftBuffer, frames, -1);

        float binSize = sampleRate / frames;
        int minBin = (int)(85 / binSize);
        int maxBin = (int)(300 / binSize);
        float maxIntensity = 0f;
        int maxBinIndex = 0;

        for (int bin = minBin; bin <= maxBin; bin++)
        {
            float real = fftBuffer[bin * 2];
            float imaginary = fftBuffer[bin * 2 + 1];
            float intensity = real * real + imaginary * imaginary;

            if (intensity > maxIntensity)
            {
                maxIntensity = intensity;
                maxBinIndex = bin;
            }
        }

        return binSize * maxBinIndex;
    }
}