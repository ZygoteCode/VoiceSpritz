using NAudio.Dsp;

public class VoiceSpritzEqualizer : VoiceSpritzEffect
{
    private BiQuadFilter[] _filters;
    private int _bandCount;

    public VoiceSpritzEqualizer(VoiceSpritzEqualizerBand[] bands)
    {
        _bandCount = bands.Length;
        _filters = new BiQuadFilter[bands.Length];

        for (int bandIndex = 0; bandIndex < _bandCount; bandIndex++)
        {
            var band = bands[bandIndex];
            _filters[bandIndex] = BiQuadFilter.PeakingEQ(44100, band.Frequency, band.Bandwidth, band.Gain);
        }
    }

    public VoiceSpritzEqualizer(float frequency, float gain, float bandwidth = 1.0F)
    {
        VoiceSpritzEqualizerBand[] bands = new VoiceSpritzEqualizerBand[1] { new VoiceSpritzEqualizerBand(frequency, gain, bandwidth) };
        _bandCount = bands.Length;
        _filters = new BiQuadFilter[bands.Length];

        for (int bandIndex = 0; bandIndex < _bandCount; bandIndex++)
        {
            var band = bands[bandIndex];
            _filters[bandIndex] = BiQuadFilter.PeakingEQ(44100, band.Frequency, band.Bandwidth, band.Gain);
        }
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            for (int band = 0; band < _bandCount; band++)
            {
                buffer[i] = _filters[band].Transform(buffer[i]);
            }
        }
    }
}