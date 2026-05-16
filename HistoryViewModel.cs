using System.Collections.Generic;
using System.Linq;

namespace Project;

public class HistoryViewModel
{
    public List<AttendanceRecord> Records { get; set; }

    // ⭐ 自动统计
    public int PresentCount => Records.Count(r => r.Status == "Present");

    public int LateCount => Records.Count(r => r.Status == "Late");

    public int AbsentCount => Records.Count(r => r.Status == "Absent");
}