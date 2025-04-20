using System;

public class VoiceSpritzDelayPong : VoiceSpritzEffect
{
    int delaypos;
    float pongloc;
    float delaylen;
    float odelay;
    float beat;
    float wetmix;
    float drymix;
    float wetmix2;
    float drymix2;
    float pongwidth;
    float pongpan;
    float[] buffer = new float[1000000];
    float sw;

    public VoiceSpritzDelayPong(float tempo = 120, float delayMs = 0, float feedbackDb = -5, float mixInDb = 0, float outputWetDb = -6, float outputDryDb = 0, float pingPongWidthPercent = 0, float beatSyncFractionWholeNote = 0.25F)
    {
        // Delay [0 for beatsync] (ms) => Default (0), Minimum (0), Maximum (13000)
        // Feedback (dB) => Default (-5), Minimum (-120), Maximum (6)
        // Mix in (dB) => Default (0), Minimum (-120), Maximum (6)
        // Output wet (dB) => Default (-6), Minimum (-120), Maximum (6)
        // Output dry (dB) => Default (0), Minimum (-120), Maximum (6)
        // Ping-pong width (%) => Default (0), Minimum (0), Maximum (100)
        // Beatsync - Fraction of whole note => Default (0.25), Minimum (0.0625), Maximum (4)

        delaypos = 0;
        pongloc = 0;
        odelay = delaylen;
        beat = 240 * beatSyncFractionWholeNote;
        wetmix = (float)Math.Pow(2, feedbackDb / 6);
        drymix = (float)Math.Pow(2, mixInDb / 6);
        wetmix2 = (float)Math.Pow(2, outputWetDb / 6);
        drymix2 = (float)Math.Pow(2, outputDryDb / 6);
        pongwidth = pingPongWidthPercent / 100;
        pongpan = (1 - pongwidth) / 2;

        if (delayMs == 0)
        {
            delaylen = Math.Min((beat / tempo) * 44100, 500000);
        }
        else
        {
            delaylen = Math.Min(delayMs * 44100 / 1000, 500000);
        }
    }

    public override void Process(float[] theBuffer, int samplesRead)
    {
        for (int j = 0; j < samplesRead; j++)
        {
            float spl0 = theBuffer[j];
            float spl1 = theBuffer[j];

            int dpint = delaypos * 2;
            float os1 = buffer[dpint + 0];
            float os2 = buffer[dpint + 1];

            buffer[dpint + 0] = Math.Min(Math.Max(spl0 * drymix + os1 * wetmix, -4), 4);
            buffer[dpint + 1] = Math.Min(Math.Max(spl1 * drymix + os2 * wetmix, -4), 4);

            //float switching = 0;

            if (Math.Abs(delaypos) < 400)
            {
                sw = (pongloc != 0) ? Math.Abs(delaypos) / 400 : ((400 - Math.Abs(delaypos)) / 400);
            }

            if ((delaypos += 1) >= delaylen)
            {
                delaypos = 0;
                pongloc = (pongloc * -1) + 1;
            }

            float os = (os1 + os2) / 2;
            float panloc = pongpan + pongwidth * sw;

            spl0 = spl0 * drymix2 + os * wetmix2 * (panloc);
            spl1 = spl1 * drymix2 + os * wetmix2 * (1 - panloc);

            theBuffer[j] = spl0;
        }
    }
}