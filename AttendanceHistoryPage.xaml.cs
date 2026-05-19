using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Controls;

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

        try
        {
            string json = Preferences.Get("records", "[]");

            var allRecords = JsonSerializer.Deserialize<List<AttendanceRecord>>(json)
                             ?? new List<AttendanceRecord>();

            // =========================
            // ⭐⭐⭐ 强制加入测试数据
            // =========================
            allRecords.AddRange(new List<AttendanceRecord>
            {
                new AttendanceRecord
                {
                    CourseName = "Python",
                    Time = "10 Jun 2026 • 08:10",
                    Status = "Present",
                    Location = "Campus Information Building"
                },
                new AttendanceRecord
                {
                    CourseName = "Data Structure",
                    Time = "10 Jun 2026 • 08:18",
                    Status = "Late",
                    Location = "Campus Information Building"
                },
                new AttendanceRecord
                {
                    CourseName = "Java",
                    Time = "09 Jun 2026 • 08:10",
                    Status = "Absent",
                    Location = "Outside Campus"
                }
            });

            // =========================
            // ⭐ 地址规范化
            // =========================
            foreach (var record in allRecords)
            {
                string original = record.Location ?? "";

                if (string.IsNullOrWhiteSpace(original))
                {
                    record.Location = "Campus Information Building";
                }
                else if (original.Contains("信息") || original.Contains("Information"))
                {
                    record.Location = "Campus Information Building";
                }
                else if (original.Contains("图书馆"))
                {
                    record.Location = "Library";
                }
            }

            // =========================
            // ⭐ 排序（最新在前）
            // =========================
            var sortedRecords = allRecords
                .Where(r => !string.IsNullOrEmpty(r.Time))
                .OrderByDescending(r => ParseDateTimeFlexible(r.Time))
                .ToList();

            // =========================
            // ⭐ 绑定
            // =========================
            BindingContext = new HistoryViewModel
            {
                Records = sortedRecords
            };
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"History load failed: {ex.Message}", "OK");
        }
    }

    // =========================
    // ⭐ 时间解析
    // =========================
    private DateTime ParseDateTimeFlexible(string timeStr)
    {
        if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        var chineseMonths = new[] { "1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月" };
        var englishMonths = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        string modified = timeStr;

        for (int i = 0; i < chineseMonths.Length; i++)
        {
            if (modified.Contains(chineseMonths[i]))
            {
                modified = modified.Replace(chineseMonths[i], englishMonths[i]);
                break;
            }
        }

        if (DateTime.TryParse(modified, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return dt;

        return DateTime.MinValue;
    }
}