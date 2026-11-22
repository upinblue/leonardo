# UpInBlue.Leonardo.WinUI

Leonardo is the design language of up in blue. This package brings it to WinUI 3 apps on the Windows App SDK.

Leonardo adds to Fluent; it does not replace it. Every Fluent control stays as it is. The package adds:

- **Token resources:** colors, spacing, radii, densities and motion values as `Leonardo…` resources, for example `LeonardoAccentBrush`, `LeonardoSpaceM`, `LeonardoRadiusL` and `LeonardoDurationNormal`.
- **The accent ramp:** `SystemAccentColor` and its light and dark steps, so Fluent controls use the Leonardo blue.
- **Text styles for the type roles:** `LeonardoPlaceNameTextStyle`, `LeonardoViewTextStyle`, `LeonardoSectionTextStyle`, `LeonardoBodyTextStyle` and `LeonardoMetaTextStyle`, plus strong and tabular variants.
- **Motion helpers** in `UpInBlue.Leonardo.WinUI.Motion`: `DiveIn`, `Transform`, `Stagger`, `Reflow`, `Arrive` and `Leave`. Every helper respects the system animation setting.
- **Three controls** in `UpInBlue.Leonardo.WinUI.Controls`: `PlaceHeader`, `SummaryLine` and `StatusDot`.

## Requirements

- Windows 11 (10.0.22000) or later
- .NET 10 and the Windows App SDK (the package depends on `Microsoft.WindowsAppSDK`)
- The app's target framework is `net10.0-windows10.0.26100.0` or later. `TargetPlatformMinVersion` can stay at
  `10.0.22000.0`.

## Get started

1. Merge the Leonardo dictionary after `XamlControlsResources` in `App.xaml`:

   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
               <ResourceDictionary Source="ms-appx:///UpInBlue.Leonardo.WinUI/Themes/Leonardo.xaml" />
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

2. Use the Mica backdrop for the window, and initialize the motion settings once on the UI thread:

   ```xml
   <Window.SystemBackdrop>
       <MicaBackdrop />
   </Window.SystemBackdrop>
   ```

   ```csharp
   UpInBlue.Leonardo.WinUI.Motion.MotionSettings.Initialize();
   ```

3. Use the resources and controls. Never write literal colors, sizes or durations:

   ```xml
   <Page xmlns:leonardo="using:UpInBlue.Leonardo.WinUI.Controls">
       <Grid RowSpacing="{StaticResource LeonardoSpaceXL}">
           <Grid.RowDefinitions>
               <RowDefinition Height="Auto" />
               <RowDefinition Height="*" />
           </Grid.RowDefinitions>
           <leonardo:PlaceHeader
               Margin="{StaticResource LeonardoSpaceXLHorizontalThickness}"
               Meta="Hofmann Logistics"
               ScrollSource="{x:Bind FileList}"
               Title="Contracts">
               <leonardo:SummaryLine
                   Margin="{StaticResource LeonardoSpaceSTopThickness}"
                   Items="{x:Bind ViewModel.Summary}"
                   Prefix="Since your last visit:" />
           </leonardo:PlaceHeader>
           <ListView x:Name="FileList" Grid.Row="1" />
       </Grid>
   </Page>
   ```

## Learn more

The design language, the gallery app and the quick starts for the other platforms are in the Leonardo repository:
https://github.com/upinblue/leonardo

## License

The code is licensed under the MIT License. The names "up in blue" and "uib" and the up in blue logos are not covered by
that license and are not part of this package.
