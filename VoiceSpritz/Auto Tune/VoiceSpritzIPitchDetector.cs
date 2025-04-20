public interface VoiceSpritzIPitchDetector
{
    float DetectPitch(float[] buffer, int frames);
}