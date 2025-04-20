public class VoiceSpritzSuppression : VoiceSpritzEffect
{
    private double _decibels;

    public VoiceSpritzSuppression(double decibels)
    {
        _decibels = decibels;
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        if (GlobalVariables.Decibels > _decibels)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i] = 0F;
            }
        }
    }
}