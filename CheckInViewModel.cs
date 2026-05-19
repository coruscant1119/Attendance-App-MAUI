using System.ComponentModel;

namespace Project;

public class CheckInViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Location { get; set; }
    public string StatusText { get; set; }
    public string DistanceText { get; set; }
    public string StatusColor { get; set; }
}
