using System.Runtime.Remoting.Lifetime;
using System;
using NAudio.Wave;
using NAudio.Utils;

public class VoiceSpritzSimpleCompressor : VoiceSpritzEffect
{
    protected const double DC_OFFSET = 1.0E-25;
    private double envdB; // over-threshold envelope (dB)
    private double MakeUpGain { get; set; }
    private double Threshold { get; set; }
    private double Ratio { get; set; }

    private readonly VoiceSpritzEnvelopeDetector attack;
    private readonly VoiceSpritzEnvelopeDetector release;

    public double Attack
    {
        get => attack.TimeConstant;
        set => attack.TimeConstant = value;
    }

    public double Release
    {
        get => release.TimeConstant;
        set => release.TimeConstant = value;
    }

    public double SampleRate
    {
        get => attack.SampleRate;
        set => attack.SampleRate = release.SampleRate = value;
    }

    public double Run(double inValue, double state)
    {
        return inValue > state ? attack.Run(inValue, state) : release.Run(inValue, state);
    }

    public VoiceSpritzSimpleCompressor(float attackTime = 5.0F, float releaseTime = 10.0F, float makeUpDbGain = 16.0F, float thresholdDb = 16.0F, float ratio = 6.0F)
    {
        attack = new VoiceSpritzEnvelopeDetector(attackTime, 44100);
        release = new VoiceSpritzEnvelopeDetector(releaseTime, 44100);
        this.Threshold = thresholdDb;
        this.Ratio = ratio;
        this.MakeUpGain = makeUpDbGain;
        this.envdB = DC_OFFSET;
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            double in1 = buffer[i];
            double in2 = buffer[i];

            double rect1 = Math.Abs(in1); // n.b. was fabs
            double rect2 = Math.Abs(in2); // n.b. was fabs

            // if desired, one could use another EnvelopeDetector to smooth
            // the rectified signal.

            double link = Math.Max(rect1, rect2);	// link channels with greater of 2

            link += DC_OFFSET; // add DC offset to avoid log( 0 )
            double keydB = Decibels.LinearToDecibels(link); // convert linear -> dB

            // threshold
            double overdB = keydB - Threshold; // delta over threshold
            if (overdB < 0.0)
                overdB = 0.0;

            // attack/release

            overdB += DC_OFFSET; // add DC offset to avoid denormal

            envdB = Run(overdB, envdB); // run attack/release envelope

            overdB = envdB - DC_OFFSET; // subtract DC offset

            // Regarding the DC offset: In this case, since the offset is added before 
            // the attack/release processes, the envelope will never fall below the offset,
            // thereby avoiding denormals. However, to prevent the offset from causing
            // constant gain reduction, we must subtract it from the envelope, yielding
            // a minimum value of 0dB.

            // transfer function
            double gr = overdB * (Ratio - 1.0);	// gain reduction (dB)
            gr = Decibels.DecibelsToLinear(gr) * Decibels.DecibelsToLinear(MakeUpGain); // convert dB -> linear

            // output gain
            in1 *= gr;	// apply gain reduction to input
            in2 *= gr;

            buffer[i] = (float) in1;
        }
    }
}

class VoiceSpritzEnvelopeDetector
{
    private double sampleRate;
    private double ms;
    private double coeff;

    public VoiceSpritzEnvelopeDetector() : this(1.0, 44100.0)
    {
    }

    public VoiceSpritzEnvelopeDetector(double ms, double sampleRate)
    {
        System.Diagnostics.Debug.Assert(sampleRate > 0.0);
        System.Diagnostics.Debug.Assert(ms > 0.0);
        this.sampleRate = sampleRate;
        this.ms = ms;
        SetCoef();
    }

    public double TimeConstant
    {
        get => ms;
        set
        {
            System.Diagnostics.Debug.Assert(value > 0.0);
            this.ms = value;
            SetCoef();
        }
    }

    public double SampleRate
    {
        get => sampleRate;
        set
        {
            System.Diagnostics.Debug.Assert(value > 0.0);
            this.sampleRate = value;
            SetCoef();
        }
    }

    public double Run(double inValue, double state)
    {
        return inValue + coeff * (state - inValue);
    }

    private void SetCoef()
    {
        coeff = Math.Exp(-1.0 / (0.001 * ms * sampleRate));
    }
}