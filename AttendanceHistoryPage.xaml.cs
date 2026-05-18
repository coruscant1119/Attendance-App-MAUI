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

            // 
            bool dataModified = false;
            foreach (var record in allRecords)
            {
                string original = record.Location ?? "";
                bool isValid = original.Contains("Campus Information Building") ||
                               original.Contains("RiXin Building") ||
                               original.Contains("Outside Campus");
                if (!isValid && !string.IsNullOrWhiteSpace(original))
                {
                    dataModified = true;
                    if (record.Status == "Absent")
                        record.Location = "Outside Campus";
                    else if (original.Contains("日新") || original.Contains("RiXin"))
                        record.Location = "RiXin Building";
                    else if (original.Contains("信息") || original.Contains("Information") || original.Contains("科大高新"))
                        record.Location = "Campus Information Building";
                    else
                        record.Location = "Unknown location";
                }
            }
            if (dataModified)
            {
                Preferences.Set("records", JsonSerializer.Serialize(allRecords));
            }

            // 
            var sortedRecords = allRecords
                .Where(r => !string.IsNullOrEmpty(r.Time))
                .OrderByDescending(r => ParseDateTimeFlexible(r.Time))
                .ToList();

            var vm = new HistoryViewModel
            {
                Records = sortedRecords
            };

            BindingContext = vm;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"History load failed: {ex.Message}", "OK");
        }
    }

    // 
    private DateTime ParseDateTimeFlexible(string timeStr)
    {
        // 
        if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        // 
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
        //
        if (DateTime.TryParse(modified, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return dt;

        return DateTime.MinValue;
    }
}