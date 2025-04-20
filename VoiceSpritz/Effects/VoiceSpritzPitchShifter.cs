using NAudio.Dsp;
using System;

public class VoiceSpritzPitchShifter : VoiceSpritzEffect
{
    private float pitch = 1f;
    private readonly int fftSize;
    private readonly long osamp;
    private SmbPitchShifter pitchShifter = new SmbPitchShifter();
    const float LIM_THRESH = 0.95f;
    const float LIM_RANGE = (1f - LIM_THRESH);
    const float M_PI_2 = (float)(Math.PI / 2);

    private float Limiter(float sample)
    {
        float res;

        if ((LIM_THRESH < sample))
        {
            res = (sample - LIM_THRESH) / LIM_RANGE;
            res = (float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        }
        else if ((sample < -LIM_THRESH))
        {
            res = -(sample + LIM_THRESH) / LIM_RANGE;
            res = -(float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        }
        else
        {
            res = sample;
        }

        return res;
    }

    public VoiceSpritzPitchShifter() : this(4096, 4L, 1F)
    {

    }

    public VoiceSpritzPitchShifter(float pitch) : this(4096, 4L, pitch)
    {

    }

    public VoiceSpritzPitchShifter(int fftSize, long osamp, float initialPitch)
    {
        this.fftSize = fftSize;
        this.osamp = osamp;
        pitch = initialPitch;
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        if (pitch == 1F)
        {
            return;
        }

        var mono = new float[samplesRead];
        var index = 0;

        for (var sample = 0; sample <= samplesRead + 0 - 1; sample++)
        {
            mono[index] = buffer[sample];
            index += 1;
        }

        pitchShifter.PitchShift(pitch, samplesRead, fftSize, osamp, 44100, mono);
        index = 0;

        for (var sample = 0; sample <= samplesRead + 0 - 1; sample++)
        {
            buffer[sample] = Limiter(mono[index]);
            index += 1;
        }
    }
}