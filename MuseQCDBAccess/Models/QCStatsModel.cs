namespace MuseQCDBAccess.Models
{
    public class QCStatsModel
    {
        public string QCID { get; set; }
        public double Duration { get; set; }
        public double Eegch1 { get; set; }
        public double Eegch2 { get; set; }
        public double Eegch3 { get; set; }
        public double Eegch4 { get; set; }
        public double Eeg_ch1_eeg_ch2 { get; set; }
        public double Eeg_ch1_eeg_ch3 { get; set; }
        public double Eeg_ch4_eeg_ch3 { get; set; }
        public double Eeg_ch4_eeg_ch2 { get; set; }
        public double FAny { get; set; }
        public double FBoth { get; set; }
        public double TAny { get; set; }
        public double TBoth { get; set; }
        public double FTAny { get; set; }
        public double EegAny { get; set; }
        public double EegAll { get; set; }
    }
}
