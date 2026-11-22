# UpInBlue.Leonardo.Wpf

Leonardo is the design language of up in blue. This package brings its values to WPF apps on .NET 10, on top of the
built-in WPF Fluent theme (`Application.ThemeMode`).

Leonardo adds to Fluent; it does not replace it. There are no custom controls. The package adds:

- **Token resources** for the light, dark and contrast themes, for example `LeonardoAccentBrush`, `LeonardoSpaceM`,
  `LeonardoRadiusL` and `LeonardoDurationNormal`.
- **The accent override:** the Fluent theme's accent colors become the Leonardo blue. In a Windows contrast theme, the
  override is left out and only system colors are used.
- **Text styles for the type roles:** `LeonardoPlaceNameTextStyle`, `LeonardoViewTextStyle`, `LeonardoSectionTextStyle`,
  `LeonardoBodyTextStyle` and `LeonardoMetaTextStyle`, plus strong and tabular variants.
- **Motion:** `LeonardoMotion` reads the Windows animation setting and runs the staggered entrance; every motion becomes
  a short crossfade when animations are off.

## Requirements

- Windows 11 and .NET 10 (`net10.0-windows`, `UseWPF`)
- The WPF Fluent theme. `ThemeMode` is still marked experimental in .NET 10, so suppress `WPF0001` in your project.

## Get started

1. Suppress the experimental warning for `ThemeMode` in the app's project file:

   ```xml
   <NoWarn>$(NoWarn);WPF0001</NoWarn>
   ```

2. Apply Leonardo at startup. `LeonardoTheme.Apply` sets `ThemeMode`, merges the Leonardo dictionary for the active
   theme, and follows the Windows app mode and contrast themes while the app runs. Do not merge the dictionaries by hand.

   ```csharp
   using System.Windows;
   using UpInBlue.Leonardo.Wpf;

   public partial class App : Application
   {
       protected override void OnStartup(StartupEventArgs e)
       {
           base.OnStartup(e);
           LeonardoTheme.Apply(this, ThemeMode.System);
           new MainWindow().Show();
       }
   }
   ```

3. Use the resources. Never write literal colors, sizes or durations:

   ```xml
   <StackPanel Margin="{StaticResource LeonardoSpaceXLHorizontalThickness}">
       <TextBlock
           AutomationProperties.HeadingLevel="Level1"
           Style="{StaticResource LeonardoPlaceNameTextStyle}"
           Text="Contracts" />
       <TextBlock Style="{StaticResource LeonardoMetaTextStyle}" Text="Hofmann Logistics" />
   </StackPanel>
   ```

4. Let the groups of a view enter in reading order:

   ```csharp
   LeonardoMotion.Stagger([HeaderGroup, ListGroup, DetailsGroup]);
   ```

## Learn more

The design language and the quick starts for the other platforms are in the Leonardo repository:
https://github.com/upinblue/leonardo

## License

The code is licensed under the MIT License. The names "up in blue" and "uib" and the up in blue logos are not covered by
that license and are not part of this package.
