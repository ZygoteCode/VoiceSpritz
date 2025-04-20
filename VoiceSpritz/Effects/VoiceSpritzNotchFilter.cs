using NAudio.Dsp;

public class VoiceSpritzNotchFilter : VoiceSpritzEffect
{
    private BiQuadFilter _filter;

    public VoiceSpritzNotchFilter(float frequency, float bandwidth = 1.0F)
    {
        _filter = BiQuadFilter.NotchFilter(44100, frequency, bandwidth);
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] = _filter.Transform(buffer[i]);
        }
    }
}