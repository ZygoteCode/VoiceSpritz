using NAudio.Dsp;
using System.Windows.Forms;

public class VoiceSpritzHighPassFilter : VoiceSpritzEffect
{
    private BiQuadFilter _filter;

    public VoiceSpritzHighPassFilter(float cutoffFrequency, float bandwidth = 1.0F)
    {
        _filter = BiQuadFilter.HighPassFilter(44100, cutoffFrequency, bandwidth);
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] = _filter.Transform(buffer[i]);
        }
    }
}