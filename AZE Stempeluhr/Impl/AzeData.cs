using System;
using System.Windows.Media.TextFormatting;

namespace AZE.Impl
{
    public class AzeData
    {
        /// <inheritdoc />
        public AzeData(int rowNumber, DateTime? begin, DateTime? end, DateTime? pause, DateTime? workingTime)
        {
            this.RowNumber = rowNumber;
            this.Begin = begin;
            this.End = end;
            this.Pause = pause;
            this.WorkingTime = workingTime;
        }

        public int RowNumber { get; }

        public DateTime? Begin { get; }

        public string BeginText => this.Begin?.ToShortTimeString() ?? "-";

        public DateTime? End { get; }
        public string EndText => this.End?.ToShortTimeString() ?? "-";

        public DateTime? Pause { get; }

        public string PauseText => !this.Pause.HasValue ? "-" : (this.Pause.Value - this.Pause.Value.Date).ToString(@"hh\:mm");

        public DateTime? WorkingTime { get; }

        public string WorkingTimeText => this.WorkingTime?.ToShortTimeString() ?? "-";
    }
}