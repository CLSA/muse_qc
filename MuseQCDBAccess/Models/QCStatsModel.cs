namespace MuseQCDBAccess.Models
{
    public class QCStatsModel
    {
        /// <summary>
        /// The duration of the collection in hours
        /// </summary>
        public double Dur { get; set; }

        /// <summary>
        /// The percent of time channel 1 had data point for
        /// </summary>
        public double Ch1 { get; set; }

        /// <summary>
        /// The percent of time channel 2 had data point for
        /// </summary>
        public double Ch2 { get; set; }

        /// <summary>
        /// The percent of time channel 3 had data point for
        /// </summary>
        public double Ch3 { get; set; }

        /// <summary>
        /// The percent of time channel 4 had data point for
        /// </summary>
        public double Ch4 { get; set; }
        public double Ch12 { get; set; }
        public double Ch13 { get; set; }
        public double Ch43 { get; set; }
        public double Ch42 { get; set; }

        /// <summary>
        /// The percentage of time any frontal contact has data
        /// </summary>
        public double FAny { get; set; }

        /// <summary>
        /// The percentage of time both frontal contacts have data
        /// </summary>
        public double FBoth { get; set; }

        /// <summary>
        /// The percentage of time any temporal contact has data
        /// </summary>
        public double TAny { get; set; }

        /// <summary>
        /// The percentage of time both temporal contacts have data
        /// </summary>
        public double TBoth { get; set; }

        /// <summary>
        /// The percentage of time either FBoth or TBoth have data
        /// </summary>
        public double FtAny { get; set; }

        /// <summary>
        /// The percentage of time any eeg contact has data
        /// </summary>
        public double EegAny { get; set; }

        /// <summary>
        /// The percentage of time all contacts have data
        /// </summary>
        public double EegAll { get; set; }
    }
}
