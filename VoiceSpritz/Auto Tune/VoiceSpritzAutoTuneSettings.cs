using System.Collections.Generic;

public class VoiceSpritzAutoTuneSettings
{
    public VoiceSpritzAutoTuneSettings()
    {
        SnapMode = true;
        PluggedIn = true;
        AutoPitches = new HashSet<VoiceSpritzNote>();
        AutoPitches.Add(VoiceSpritzNote.C);
        AutoPitches.Add(VoiceSpritzNote.CSharp);
        AutoPitches.Add(VoiceSpritzNote.D);
        AutoPitches.Add(VoiceSpritzNote.DSharp);
        AutoPitches.Add(VoiceSpritzNote.E);
        AutoPitches.Add(VoiceSpritzNote.F);
        AutoPitches.Add(VoiceSpritzNote.FSharp);
        AutoPitches.Add(VoiceSpritzNote.G);
        AutoPitches.Add(VoiceSpritzNote.GSharp);
        AutoPitches.Add(VoiceSpritzNote.A);
        AutoPitches.Add(VoiceSpritzNote.ASharp);
        AutoPitches.Add(VoiceSpritzNote.B);
        VibratoDepth = 0.0;
        VibratoRate = 4.0;
        AttackTimeMilliseconds = 0.0;
    }

    public bool Enabled { get; set; }
    public bool SnapMode { get; set; }
    public double AttackTimeMilliseconds { get; set; }
    public HashSet<VoiceSpritzNote> AutoPitches { get; private set; }
    public bool PluggedIn { get; set; }
    public double VibratoRate { get; set; }
    public double VibratoDepth { get; set; }
}

public enum VoiceSpritzNote
{
    C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
}