using System.Collections.Generic;
using System.Linq;

namespace Project;

public class HistoryViewModel
{
    public List<AttendanceRecord> Records { get; set; }

    public int PresentCount => Records?.Count(r => r.Status == "Present") ?? 0;
    public int LateCount => Records?.Count(r => r.Status == "Late") ?? 0;
    public int AbsentCount => Records?.Count(r => r.Status == "Absent") ?? 0;

    public int TotalCount => Records?.Count ?? 0;

    public double AttendanceRate =>
        TotalCount == 0 ? 0 :
        ((PresentCount + LateCount) * 100.0 / TotalCount);

    public string AttendanceText => $"{AttendanceRate:F0}%";

    public string RateColor =>
        AttendanceRate >= 80 ? "#16A34A" :
        AttendanceRate >= 60 ? "#F97316" :
        "#EF4444";
}