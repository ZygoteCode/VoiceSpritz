using System;

public class VoiceSpritzDistortion : VoiceSpritzEffect
{
    private float _gainDb;
    private VoiceSpritzDistortionMethod _method;

    public VoiceSpritzDistortion(float gainDb = 1.0F, VoiceSpritzDistortionMethod method = VoiceSpritzDistortionMethod.Method1)
    {
        _gainDb = gainDb;
        _method = method;
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        if (_method.Equals(VoiceSpritzDistortionMethod.Method1))
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i] = (float) Math.Tanh(_gainDb * buffer[i]);
            }
        }
        else if (_method.Equals(VoiceSpritzDistortionMethod.Method2))
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i] = Mod(_gainDb * buffer[i] + 1, 2) - 1;
            }
        }
        else if (_method.Equals(VoiceSpritzDistortionMethod.Method3))
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i] = Math.Abs(Mod(2 * _gainDb * buffer[i] + 2, 4) - 2) - 1;
            }
        }
        else if (_method.Equals(VoiceSpritzDistortionMethod.Method4))
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i] = (float) Math.Sin(_gainDb * buffer[i]);
            }
        }
    }

    private float Mod(float a, float b)
    {
        a = (float) Math.IEEERemainder(a, b);

        if (a < 0)
        {
            a += b;
        }

        return a;
    }
}

public enum VoiceSpritzDistortionMethod
{
    Method1,
    Method2,
    Method3,
    Method4
}