using System.Text.Json;

namespace Project;

public partial class AttendanceHistoryPage : ContentPage
{
    public AttendanceHistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
       
        var json = Preferences.Get("records", "[]");

        var list = JsonSerializer.Deserialize<List<AttendanceRecord>>(json)
                   ?? new List<AttendanceRecord>();

        // 
        if (list.Count == 0)
        {
            list = new List<AttendanceRecord>
            {
                new AttendanceRecord
                {
                    Username = "iris",
                    CourseName = "Data Structures",
                    Time = DateTime.Now.AddDays(-1).ToString("dd MMM yyyy • HH:mm"),
                    Status = "Present"
                },
                new AttendanceRecord
                {
                    Username = "iris",
                    CourseName = "Java Programming",
                    Time = DateTime.Now.AddDays(-2).ToString("dd MMM yyyy • HH:mm"),
                    Status = "Late"
                },
                new AttendanceRecord
                {
                    Username = "iris",
                    CourseName = "Python Programming",
                    Time = DateTime.Now.AddDays(-3).ToString("dd MMM yyyy • HH:mm"),
                    Status = "Absent"
                },
                new AttendanceRecord
                {
                    Username = "iris",
                    CourseName = "Database Systems",
                    Time = DateTime.Now.AddDays(-4).ToString("dd MMM yyyy • HH:mm"),
                    Status = "Present"
                }
            };

            Preferences.Set("records", JsonSerializer.Serialize(list));
        }

        // ⭐ 最新在最前
        list = list.OrderByDescending(x => x.Time).ToList();

        var vm = new HistoryViewModel
        {
            Records = list
        };

        BindingContext = vm;

    }
}