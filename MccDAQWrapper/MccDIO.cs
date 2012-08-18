using System;
using System.Collections.Generic;
using HWGlobals;
using MccDaq;
using System.Threading;

namespace MccDAQWrapper
{
    public class MccDIO : IMultiChannelClock, IDisposable
    {
        #region IMultiChannelClock Members

        public bool Init(Object condata)
        {
            if (IsInit)
                return true;

            try
            {
                int boardnum = (int)condata;
                m_daqBoard = new MccBoard(boardnum);
                InitUL();
                IsInit = InternalDAConfig();
            }
            catch (System.Exception ex)
            {
            }
            return IsInit;
        }

        public bool IsExtTrig { get; set; }

        public bool ConfigFiniteTrig(Trig[] trigs)
        {
            IsFinite = true;
            if (trigs != null && trigs.Length > 0 && trigs.Length <= AO_CHANNELS)
            {
                Triggers = trigs;
                return InternalFiniteDAConfig();
            }
            return false;
        }

        public bool ConfigContTrig(int[] freqs)
        {
            IsFinite = false;
            if (freqs != null && freqs.Length > 0 && freqs.Length <= AO_CHANNELS)
            {
                Frequencies = freqs;
                return InternalDAConfig();
            }
            return false;
        }

        public bool StartAll()
        {
            if (!IsInit)
                return false;

            int CurIndex;
            int CurCount;
            short Status;

            Rate = NumPoints;

            MccDaq.ScanOptions options = MccDaq.ScanOptions.Background | MccDaq.ScanOptions.ScaleData;

            if (IsExtTrig)
            {
                options |= ScanOptions.ExtTrigger;
            }

            if (!IsFinite)
            {
                options |= MccDaq.ScanOptions.Continuous;
            }

            int rate = IsFinite ? FiniteRate : Rate;
            int count = IsFinite ? FiniteCount : Count;

            ULStat = DaqBoard.AOutScan(0, NumChannels - 1, count, ref rate, VoltageGain, DAMemHandle, options);

            ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, MccDaq.FunctionType.AoFunction);

            return (Status == MccDaq.MccBoard.Running);
        }

        public bool IsRunning
        {
            get
            {
                if (!IsInit)
                    return false;

                int CurIndex, CurCount;
                short Status;
                ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, MccDaq.FunctionType.AoFunction);
                return (Status == MccDaq.MccBoard.Running);
            }
        }

        public void StopAll()
        {
            if (DaqBoard != null)
            {
                ULStat = DaqBoard.StopBackground(MccDaq.FunctionType.AoFunction);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            StopAll();
            if (DAMemHandle != IntPtr.Zero)
            {
                MccDaq.MccService.WinBufFreeEx(DAMemHandle);
            }
        }

        #endregion

        #region Private methods

        private void BuildArray()
        { // one array is scanned element-wise for all active channels
            DAData = new double[Count];

            int[] intervals = new int[NumChannels];
            for (int i = 0; i < NumChannels; i++)
            {
                intervals[i] = NumPoints / Frequencies[i];
            }

            for (int i = 0; i < NumPoints; i++)
            {
                for (int j = 0; j < NumChannels; j++)
                {
                    DAData[i * NumChannels + j] = i % intervals[j] < intervals[j] / 2 ? TTL_LEVEL : 0;
                }
            }
        }

        private void BuildFiniteArray()
        { // one array is scanned element-wise for all active channels
            DAData = new double[FiniteCount];
            List<double[]> res = new List<double[]>();
            int[] pulses = new int[NumChannels];
            int[] intervals = new int[NumChannels];

            for (int i = 0; i < NumChannels; i++)
            {
                res.Add(new double[FiniteNumPoint]);
                intervals[i] = FiniteRate / Triggers[i].Frequency;
            }

            for (int i = 0; i < FiniteNumPoint; i++)
            {
                for (int j = 0; j < NumChannels; j++)
                {
                    if (pulses[j] > Triggers[j].Pulses)
                        continue;

                    if (i % intervals[j] < intervals[j] / 2)
                    {   // TTL
                        if (i == 0 || res[j][i - 1] == 0)
                        { // rising edge point
                            if (pulses[j] < Triggers[j].Pulses)
                            { // NO TTL!
                                res[j][i] = Triggers[j].IsHigh ? Trig.TTL_HIGH : TTL_LEVEL;
                            }
                            pulses[j]++;
                        }
                        else
                        {
                            res[j][i] = Triggers[j].IsHigh ? Trig.TTL_HIGH : TTL_LEVEL;
                        }
                    }

                    DAData[i * NumChannels + j] = res[j][i];
                }
            }
        }

        private void InitUL()
        {
            ULStat = MccDaq.MccService.ErrHandling(MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);
        }

        private bool InternalDAConfig()
        {
            if (DAMemHandle != IntPtr.Zero)
            {
                MccDaq.MccService.WinBufFreeEx(DAMemHandle);
            }

            int FirstPoint = 0;
            DAMemHandle = MccDaq.MccService.ScaledWinBufAllocEx(Count); // set aside memory to hold data
            if (DAMemHandle == IntPtr.Zero)
            {
                return false;
            }

            BuildArray();

            ULStat = MccDaq.MccService.ScaledWinArrayToBuf(DAData, DAMemHandle, FirstPoint, Count);
            return ULStat.Value == ErrorInfo.ErrorCode.NoErrors;
        }

        private void GetMaxFreqSamples(out int samples, out int maxFreq)
        {
            double maxTime = 0;
            double time;
            maxFreq = 1;

            foreach (Trig trig in Triggers)
            {
                time = (double)trig.Pulses / trig.Frequency;
                if (time > maxTime)
                {
                    maxTime = time;
                }
                if (trig.Frequency > maxFreq)
                {
                    maxFreq = trig.Frequency;
                }
            }
            samples = (int)(maxTime * maxFreq * 2);
        }

        private bool InternalFiniteDAConfig()
        {
            if (DAMemHandle != IntPtr.Zero)
            {
                MccDaq.MccService.WinBufFreeEx(DAMemHandle);
            }

            int samples, freq;
            GetMaxFreqSamples(out samples, out freq);

            FiniteRate = freq * 2;
            FiniteNumPoint = samples;

            int FirstPoint = 0;
            DAMemHandle = MccDaq.MccService.ScaledWinBufAllocEx(FiniteCount); // set aside memory to hold data
            if (DAMemHandle == IntPtr.Zero)
            {
                return false;
            }

            BuildFiniteArray();

            ULStat = MccDaq.MccService.ScaledWinArrayToBuf(DAData, DAMemHandle, FirstPoint, FiniteCount);
            return ULStat.Value == ErrorInfo.ErrorCode.NoErrors;
        }

        #endregion

        #region private members

        private MccBoard m_daqBoard;
        private MccBoard DaqBoard { get { return m_daqBoard; } }
        private MccDaq.ErrorInfo ULStat;

        private const int AO_CHANNELS = 4;

        MccDaq.Range VoltageGain = Range.Uni5Volts;

        private const int NumPoints = 2000;   //  Number of data points to collect/output
        private int Rate = 2000;        //  rate of 'sample' output. 2000 ==> 1000 Hz clock wave of 50% duty cycle        
        private int[] Frequencies = new int[] { 1000 }; // default to one 1000 Hz AO
        private int NumChannels { get { return Math.Max(Triggers.Length, Frequencies.Length); } }   // active AO channels

        private int Count { get { return NumPoints * NumChannels; } }   // total number of samples

        private int FiniteRate;
        private int FiniteCount { get { return FiniteNumPoint * NumChannels; } }
        private int FiniteNumPoint;

        private bool IsFinite;

        private double[] DAData;        //  array to hold the output values

        private int TTL_LEVEL { get { return Trig.TTL_NORMAL; } }

        private IntPtr DAMemHandle;		//  handle for analog output array 
        //  memory allocated by Windows through MccDaq.MccService.WinBufAlloc()

        private Trig[] Triggers = new Trig[] { new Trig() { Frequency = 1000, Pulses = 1000 } };
        private bool IsInit;

        #endregion
    }
}
