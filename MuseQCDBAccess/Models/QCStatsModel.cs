namespace MuseQCDBAccess.Models
{
    public class QCStatsModel
    {
        public string QCID { get; set; }
        public double Dur { get; set; }
        public double Ch1 { get; set; }
        public double Ch2 { get; set; }
        public double Ch3 { get; set; }
        public double Ch4 { get; set; }
        public double Ch12 { get; set; }
        public double Ch13 { get; set; }
        public double Ch43 { get; set; }
        public double Ch42 { get; set; }
        public double FAny { get; set; }
        public double FBoth { get; set; }
        public double TAny { get; set; }
        public double TBoth { get; set; }
        public double FtAny { get; set; }
        public double EegAny { get; set; }
        public double EegAll { get; set; }
    }
}
