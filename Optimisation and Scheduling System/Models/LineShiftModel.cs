using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("lineshift")]
    public class LineShift
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public TimeSpan ShiftTimeStart { get; set; }
        public TimeSpan ShiftTimeEnd { get; set; }
        public int Day { get; set; }
        public bool IsDayShift { get; set; }

        public virtual Line Line { get; set; }

        [NotMapped]
        public string DisplayText
        {
            get
            {
                string lineName = Line?.Name ?? "Unknown Line";
                string dayString = GetDayName(Day);
                string shiftType = IsDayShift ? "Day" : "Night";
                return $"{lineName} | {dayString} | {ShiftTimeStart:hh\\:mm} - {ShiftTimeEnd:hh\\:mm} ({shiftType})";
            }
        }

        private string GetDayName(int day)
        {
            return Enum.GetName(typeof(DayOfWeek), day % 7) ?? $"Day {day}";
        }
    }
}
