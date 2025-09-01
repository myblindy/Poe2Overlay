using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Poe2Overlay;

public partial class MainWindow : Window
{
    public ObservableCollection<bool> PowerCharges { get; } = [false, false, false];

    public MainWindow()
    {
        InitializeComponent();

        ImageListener.PowerChargeUpdated += (count, duration) => Dispatcher.InvokeAsync(() =>
        {
            while (PowerCharges.Count < count)
                PowerCharges.Add(false);
            for (int i = 0; i < PowerCharges.Count; ++i)
                PowerCharges[i] = i < count;
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set the window to be transparent and always on top
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = (WINDOW_EX_STYLE)PInvoke.GetWindowLong(new(hwnd), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        PInvoke.SetWindowLong(new(hwnd), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            (int)(exStyle | WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TRANSPARENT));
    }
}

public class PowerChargeToFillColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
         new SolidColorBrush(value is true ? Colors.Aqua : Colors.Gray);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}