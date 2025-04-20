using MetroSuite;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

public partial class MainForm : MetroForm
{
    [DllImport("psapi.dll")]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessWorkingSetSize(IntPtr process, UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);

    private int devicesNumber = -1;
    
    private WaveIn waveMicrophoneInput;
    private WaveOut waveHeadphonesOutput;
    private WaveOut waveCableMicrophoneOutput;

    private BufferedWaveProvider headphonesWaveProvider;
    private BufferedWaveProvider cableWaveProvider;

    private VoiceSpritzWaveProvider spritzHeadPhonesWaveProvider;
    private VoiceSpritzWaveProvider spritzCableWaveProvider;

    public MainForm()
    {
        InitializeComponent();
        GlobalVariables.ShowDB = true;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        CheckForIllegalCrossThreadCalls = false;

        Thread clearRamThread = new Thread(ClearRam);
        clearRamThread.Priority = ThreadPriority.Highest;
        clearRamThread.Start();

        Thread checkDevicesThread = new Thread(CheckDevices);
        checkDevicesThread.Priority = ThreadPriority.Highest;
        checkDevicesThread.Start();

        Thread updateDbThread = new Thread(UpdateDb);
        updateDbThread.Priority = ThreadPriority.Highest;
        updateDbThread.Start();

        foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
        {
            thread.PriorityLevel = ThreadPriorityLevel.Highest;
        }

        if (!RefreshDevices())
        {
            return;
        }

        RefreshLiveMode();
    }

    public void UpdateDb()
    {
        while (true)
        {
            Thread.Sleep(1);
            
            if (GlobalVariables.ShowDB)
            {
                metroLabel3.Text = $"Current dB value: {GlobalVariables.Decibels.ToString().Replace(",", ".")} dB.";
            }
            else
            {
                metroLabel3.Text = $"Current dB value: 0.0 dB.";
            }
        }
    }

    public void RefreshLiveMode()
    {
        if (guna2ComboBox1.SelectedItem == null || guna2ComboBox2.SelectedItem == null || guna2ComboBox4.SelectedItem == null || guna2ComboBox2.SelectedItem.ToString() == guna2ComboBox4.SelectedItem.ToString())
        {
            return;
        }

        if (waveMicrophoneInput != null)
        {
            waveMicrophoneInput.StopRecording();
            waveMicrophoneInput.Dispose();
            waveHeadphonesOutput.Stop();
            waveHeadphonesOutput.Dispose();
            waveCableMicrophoneOutput.Stop();
            waveCableMicrophoneOutput.Dispose();
            headphonesWaveProvider.ClearBuffer();
            cableWaveProvider.ClearBuffer();
        }

        for (int waveOutDevice = 0; waveOutDevice < WaveOut.DeviceCount; waveOutDevice++)
        {
            WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);

            string productName = deviceInfo.ProductName.ToLower();
            string realProductName = guna2ComboBox2.SelectedItem.ToString().ToLower();
            string cableRealProductName = guna2ComboBox4.SelectedItem.ToString().ToLower();

            if (productName.StartsWith(realProductName) || realProductName.StartsWith(productName))
            {
                waveHeadphonesOutput = new WaveOut();
                waveHeadphonesOutput.DeviceNumber = waveOutDevice;
            }
            else if (productName.StartsWith(cableRealProductName) || cableRealProductName.StartsWith(productName))
            {
                waveCableMicrophoneOutput = new WaveOut();
                waveCableMicrophoneOutput.DeviceNumber = waveOutDevice;
            }
        }

        for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
        {
            WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);

            string productName = deviceInfo.ProductName.ToLower();
            string realProductName = guna2ComboBox1.SelectedItem.ToString().ToLower();

            if (productName.StartsWith(realProductName) || realProductName.StartsWith(productName))
            {
                waveMicrophoneInput = new WaveIn();
                waveMicrophoneInput.WaveFormat = new WaveFormat(44100, 16, 1);
                waveMicrophoneInput.BufferMilliseconds = 100;
                waveMicrophoneInput.DeviceNumber = waveInDevice;
                waveMicrophoneInput.DataAvailable += WaveMicrophoneInput_DataAvailable;

                headphonesWaveProvider = new BufferedWaveProvider(waveMicrophoneInput.WaveFormat);
                spritzHeadPhonesWaveProvider = new VoiceSpritzWaveProvider(headphonesWaveProvider.ToSampleProvider());

                /*spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzNormalizer());
                spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzVolumeAmplification(6.0F));
                spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzSuperEqualizer(lowFrequenciesGainDb: 6, middleFrequenciesGainDb: 2.5F, highFrequenciesGainDb: 3.0F));
                spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzCompressor());
                spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzAutoTune(new VoiceSpritzAutoTuneSettings()));*/
                //spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzFlanger());

                waveHeadphonesOutput.Init(spritzHeadPhonesWaveProvider);
                waveHeadphonesOutput.Play();

                cableWaveProvider = new BufferedWaveProvider(waveMicrophoneInput.WaveFormat);
                spritzCableWaveProvider = new VoiceSpritzWaveProvider(cableWaveProvider.ToSampleProvider());

                /*spritzCableWaveProvider.AddEffect(new VoiceSpritzNormalizer());
                spritzCableWaveProvider.AddEffect(new VoiceSpritzVolumeAmplification(6.0F));
                spritzCableWaveProvider.AddEffect(new VoiceSpritzSuperEqualizer(lowFrequenciesGainDb: 6, middleFrequenciesGainDb: 2.5F, highFrequenciesGainDb: 3.0F));
                spritzCableWaveProvider.AddEffect(new VoiceSpritzCompressor());
                spritzCableWaveProvider.AddEffect(new VoiceSpritzAutoTune(new VoiceSpritzAutoTuneSettings()));*/
                //spritzHeadPhonesWaveProvider.AddEffect(new VoiceSpritzFlanger());

                waveCableMicrophoneOutput.Init(spritzCableWaveProvider);
                waveCableMicrophoneOutput.Play();

                waveMicrophoneInput.StartRecording();
            }
        }
    }

    private void WaveMicrophoneInput_DataAvailable(object sender, WaveInEventArgs e)
    {
        if (!guna2CheckBox1.Checked || (!guna2CheckBox2.Checked && !guna2CheckBox3.Checked))
        {
            return;
        }

        if (guna2CheckBox2.Checked)
        {
            headphonesWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        if (guna2CheckBox3.Checked)
        {
            cableWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
    }

    public void ClearRam()
    {
        while (true)
        {
            Thread.Sleep(500);
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
        }
    }

    public void CheckDevices()
    {
        while (true)
        {
            Thread.Sleep(100);
            MMDeviceCollection allDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

            if (devicesNumber == -1)
            {
                devicesNumber = allDevices.Count;
            }
            else
            {
                if (allDevices.Count != devicesNumber)
                {
                    devicesNumber = allDevices.Count;
                    string currentInputDevice = guna2ComboBox1.SelectedItem.ToString();
                    string currentOutputDevice = guna2ComboBox2.SelectedItem.ToString();
                    string currentVacOutputDevice = guna2ComboBox4.SelectedItem.ToString();

                    if (!RefreshDevices())
                    {
                        return;
                    }

                    if (!guna2ComboBox1.Items.Contains(currentInputDevice) || !guna2ComboBox2.Items.Contains(currentOutputDevice) || !guna2ComboBox4.Items.Contains(currentVacOutputDevice))
                    {
                        Process.GetCurrentProcess().Kill();
                        return;
                    }

                    guna2ComboBox1.SelectedItem = currentInputDevice;
                    guna2ComboBox2.SelectedItem = currentOutputDevice;
                    guna2ComboBox4.SelectedItem = currentVacOutputDevice;
                }
            }
        }
    }

    public bool RefreshDevices()
    {
        guna2ComboBox1.Items.Clear();
        guna2ComboBox2.Items.Clear();
        guna2ComboBox4.Items.Clear();
        MMDeviceCollection inputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

        foreach (MMDevice inputDevice in inputDevices)
        {
            string deviceName = inputDevice.FriendlyName;
            guna2ComboBox1.Items.Add(deviceName);
        }

        MMDeviceCollection outputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (MMDevice outputDevice in outputDevices)
        {
            string deviceName = outputDevice.FriendlyName;
            guna2ComboBox2.Items.Add(deviceName);

            if (deviceName.ToLower().Contains("virtual") || deviceName.ToLower().Contains("cable"))
            {
                guna2ComboBox4.Items.Add(deviceName);
            }
        }

        if (guna2ComboBox1.Items.Count == 0 || guna2ComboBox2.Items.Count == 0 || guna2ComboBox4.Items.Count == 0)
        {
            Process.GetCurrentProcess().Kill();
            return false;
        }

        guna2ComboBox1.SelectedIndex = 0;
        guna2ComboBox2.SelectedIndex = 0;
        guna2ComboBox4.SelectedIndex = 0;

        return true;
    }

    private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
    {
        Process.GetCurrentProcess().Kill();
    }

    private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshLiveMode();
    }

    private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshLiveMode();
    }

    private void guna2ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshLiveMode();
    }

    private void guna2CheckBox4_CheckedChanged(object sender, EventArgs e)
    {
        GlobalVariables.ShowDB = guna2CheckBox4.Checked;
    }
}