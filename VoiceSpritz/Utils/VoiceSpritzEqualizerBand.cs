public class VoiceSpritzEqualizerBand
{
    public float Frequency { get; set; }
    public float Gain { get; set; }
    public float Bandwidth { get; set; }

    public VoiceSpritzEqualizerBand(float frequency, float gain, float bandwidth = 1.0F)
    {
        Frequency = frequency;
        Gain = gain;
        Bandwidth = bandwidth;
    }
}