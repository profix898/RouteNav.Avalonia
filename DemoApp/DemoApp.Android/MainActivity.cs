using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace DemoApp.Android;

[Activity(Label = "DemoApp.Android",
          Theme = "@style/MyTheme.NoActionBar",
          Icon = "@drawable/icon",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}

// Avalonia 12 initializes the Android application (and its activities) through an
// AvaloniaAndroidApplication<TApp>. App builder customization (previously done on the activity)
// now lives here.
