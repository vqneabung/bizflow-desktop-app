# Avalonia UI Documentation — Bizflow Desktop App

> **Framework**: Avalonia 12.0.4 · .NET 10 · CommunityToolkit.Mvvm 8.4.1  
> **Project**: `bizflow-desktop-app`
> **Source**: [docs.avaloniaui.net](https://docs.avaloniaui.net) — Context7 library: `/avaloniaui/avalonia-docs`

---

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [MVVM Pattern](#2-mvvm-pattern)
3. [Data Binding](#3-data-binding)
4. [XAML Controls & Layout](#4-xaml-controls--layout)
5. [Styling & Themes](#5-styling--themes)
6. [Navigation](#6-navigation)
7. [Dialogs & Windows](#7-dialogs--windows)
8. [HttpClient & API Integration](#8-httpclient--api-integration)
9. [Dependency Injection](#9-dependency-injection)
10. [CommunityToolkit.Mvvm Deep Dive](#10-communitytoolkitmvvm-deep-dive)
11. [Compiled Bindings](#11-compiled-bindings)
12. [Custom Controls](#12-custom-controls)
13. [Troubleshooting & Best Practices](#13-troubleshooting--best-practices)

---

## 1. Getting Started

### Create a new Avalonia MVVM project

```bash
# Install Avalonia templates
dotnet new install Avalonia.Templates

# Create new project with MVVM template
dotnet new avalonia.mvvm -n MyApp

# Run the application
cd MyApp
dotnet run
```

### Project structure (MVVM template)

```
MyApp/
├── App.axaml            # Application XAML (styles, data templates)
├── App.axaml.cs         # Application code-behind (entry point, DI setup)
├── Program.cs           # BuildAvaloniaApp + StartWithClassicDesktopLifetime
├── ViewLocator.cs       # Convention-based ViewModel → View mapping
├── ViewModels/
│   ├── ViewModelBase.cs # Base class (extends ObservableObject)
│   └── MainWindowViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
├── Models/
├── Assets/
└── bizflow-desktop-app.csproj
```

### csproj configuration

Current project settings:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="12.0.4" />
    <PackageReference Include="Avalonia.Desktop" Version="12.0.4" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="12.0.4" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="12.0.4" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.1" />
  </ItemGroup>
</Project>
```

### Program.cs

```csharp
using Avalonia;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

### App.axaml (base XAML)

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="bizflow_desktop_app.App"
             RequestedThemeVariant="Default">
    <!-- Default = system theme; can also use "Dark" or "Light" -->

    <Application.DataTemplates>
        <local:ViewLocator/>
    </Application.DataTemplates>

    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
```

### ViewLocator (convention-based)

Maps `SomeViewModel` → `SomeView` using reflection:

```csharp
[RequiresUnreferencedCode("Uses reflection to find views")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
```

---

## 2. MVVM Pattern

### Core layers

| Layer | Responsibility | Example |
|---|---|---|
| **View** (`.axaml`) | UI layout, data binding, animation | `MainWindow.axaml` |
| **ViewModel** | State, commands, logic | `MainWindowViewModel.cs` |
| **Model** | Data, business rules, API DTOs | `User.cs`, `Product.cs` |

### ViewModelBase

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public abstract class ViewModelBase : ObservableObject
{
    // All ViewModels inherit from this base class.
    // ObservableObject provides INotifyPropertyChanged implementation.
}
```

### Basic ViewModel with property and command

```csharp
public partial class MainViewModel : ViewModelBase
{
    // [ObservableProperty] generates INotifyPropertyChanged
    [ObservableProperty]
    private string _name = string.Empty;

    // Property usage in code:
    //   Name = "John"    → sets field, raises PropertyChanged
    //   NameProperty     → compiler-generated property

    // [RelayCommand] generates ICommand from a method
    [RelayCommand]
    private void Save()
    {
        // Called via {Binding SaveCommand} in XAML
        // Save method runs here
    }
}
```

### Command with parameter

```csharp
[RelayCommand]
private async Task DeleteItem(int id)
{
    // Generates DeleteItemCommand — supports CanExecute
    await Task.Delay(100);
}
```

### Command with CanExecute

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save()
{
    // Logic here
}

private bool CanSave() => !string.IsNullOrWhiteSpace(Name);
// Button auto-disables when Name is empty
```

### XAML binding to ViewModel

```xml
<Window x:DataType="vm:MainWindowViewModel">
    <StackPanel Spacing="8">
        <TextBox Text="{Binding Name}" />
        <TextBlock Text="{Binding Name}" />
        <Button Content="Save" Command="{Binding SaveCommand}" />
        <Button Content="Delete" Command="{Binding DeleteItemCommand}"
                CommandParameter="42" />
    </StackPanel>
</Window>
```

### ViewModel with constructor injection

```csharp
public partial class MainViewModel : ViewModelBase
{
    private readonly IDataService _dataService;

    public MainViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var items = await _dataService.GetItemsAsync();
        Items = new ObservableCollection<Item>(items);
    }

    [ObservableProperty]
    private ObservableCollection<Item> _items = new();
}
```

---

## 3. Data Binding

### Binding modes

| Mode | Direction | Default for |
|---|---|---|
| `OneWay` | ViewModel → View | `TextBlock.Text` |
| `TwoWay` | ViewModel ↔ View | `TextBox.Text`, `Slider.Value` |
| `OneTime` | ViewModel → View (once) | Read-only display |
| `OneWayToSource` | View → ViewModel | Rare |

### Basic binding

```xml
<!-- OneWay (default for TextBlock) -->
<TextBlock Text="{Binding Name}" />

<!-- TwoWay (default for TextBox) -->
<TextBox Text="{Binding Email}" />

<!-- With mode explicit -->
<TextBox Text="{Binding Username, Mode=TwoWay}" />
<Slider Value="{Binding Brightness}" />  <!-- TwoWay by default -->
```

### Binding with converters

```xml
<!-- StringFormat -->
<TextBlock Text="{Binding Price, StringFormat='{}{0:C}'}" />

<!-- Bool to visibility (built-in) -->
<Panel IsVisible="{Binding IsLoading}" />
```

### Binding to commands

```xml
<Button Content="Save" Command="{Binding SaveCommand}" />
<Button Content="Delete" Command="{Binding DeleteCommand}"
        CommandParameter="{Binding SelectedItem}" />
```

### Compiled bindings (performance + compile-time checks)

```xml
<!-- Enable per-control -->
<UserControl x:DataType="vm:MyViewModel" x:CompileBindings="True">
    <TextBlock Text="{Binding Name}" />
</UserControl>

<!-- Opt-out for dynamic properties -->
<TextBlock Text="{ReflectionBinding DynamicProperty}" />
```

### Inherited DataContext

```xml
<!-- All child controls inherit the window's DataContext -->
<Window x:DataType="vm:MainWindowViewModel">
    <StackPanel>
        <TextBlock Text="{Binding Name}" />         <!-- MainWindowViewModel.Name -->
        <TextBox Text="{Binding Email}" />           <!-- MainWindowViewModel.Email -->
    </StackPanel>
</Window>
```

---

## 4. XAML Controls & Layout

### Layout Panels

| Panel | Behavior |
|---|---|
| `StackPanel` | Stacks children vertically/horizontally |
| `Grid` | Flexible row/column layout |
| `DockPanel` | Dock to edges (Top, Bottom, Left, Right) |
| `WrapPanel` | Wraps to next line when overflow |
| `Canvas` | Absolute positioning |
| `RelativePanel` | Position relative to other elements |

### Grid layout

```xml
<Grid ColumnDefinitions="Auto,*,2*" RowDefinitions="Auto,Auto,*">
    <!-- Row 0, Col 0 -->
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Label:" />

    <!-- Row 0, Col 1 (fills remaining space) -->
    <TextBox Grid.Row="0" Grid.Column="1" />

    <!-- Spanning 2 columns -->
    <Button Grid.Row="1" Grid.ColumnSpan="2" Content="Submit" />
</Grid>
```

### Common controls

**TextInput:**
```xml
<TextBox Text="{Binding Username}" PlaceholderText="Enter username"
         Watermark="Username" />
<PasswordBox Password="{Binding Password}" />
```

**Selection:**
```xml
<ComboBox ItemsSource="{Binding Countries}"
          SelectedItem="{Binding SelectedCountry}" />

<ListBox ItemsSource="{Binding Products}"
         SelectedItem="{Binding SelectedProduct}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### DataGrid (requires NuGet: `Avalonia.Controls.DataGrid`)

```xml
<DataGrid ItemsSource="{Binding People}"
          AutoGenerateColumns="False"
          IsReadOnly="True"
          CanUserReorderColumns="True"
          CanUserResizeColumns="True"
          CanUserSortColumns="False"
          GridLinesVisibility="All"
          BorderThickness="1" BorderBrush="Gray">

    <DataGrid.Columns>
        <DataGridTextColumn Header="First Name" Binding="{Binding FirstName}" Width="2*" />
        <DataGridTextColumn Header="Last Name" Binding="{Binding LastName}" Width="2*" />
        <DataGridTextColumn Header="Price" Binding="{Binding Price, StringFormat='{}{0:C}'}" Width="*" />
        <DataGridCheckBoxColumn Header="In Stock" Binding="{Binding InStock}" Width="Auto" />
    </DataGrid.Columns>
</DataGrid>
```

### Image

```xml
<!-- From application assets -->
<Image Source="avares://bizflow-desktop-app/Assets/logo.png" />

<!-- From URL (via helper) -->
<Image Source="{Binding AvatarUrl}" />
```

---

## 5. Styling & Themes

### FluentTheme base

```xml
<Application.Styles>
    <FluentTheme />
</Application.Styles>
```

### Override default styles

Place **after** `<FluentTheme />` to take precedence:

```xml
<Application.Styles>
    <FluentTheme />

    <Style Selector="TextBlock">
        <Setter Property="FontFamily" Value="Inter, Segoe UI, sans-serif" />
    </Style>

    <Style Selector="Button">
        <Setter Property="CornerRadius" Value="8" />
    </Style>
</Application.Styles>
```

### Custom color palette (light/dark)

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Light">
                <SolidColorBrush x:Key="CardBackground" Color="#FFFFFF" />
                <SolidColorBrush x:Key="CardBorder" Color="#E5E7EB" />
                <SolidColorBrush x:Key="TextPrimary" Color="#111827" />
            </ResourceDictionary>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="CardBackground" Color="#1F2937" />
                <SolidColorBrush x:Key="CardBorder" Color="#374151" />
                <SolidColorBrush x:Key="TextPrimary" Color="#F9FAFB" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Usage:
```xml
<Border Background="{DynamicResource CardBackground}"
        BorderBrush="{DynamicResource CardBorder}">
    <TextBlock Foreground="{DynamicResource TextPrimary}" Text="Hello" />
</Border>
```

### Control themes (custom button shape)

```xml
<ControlTheme x:Key="EllipseButton" TargetType="Button">
    <Setter Property="Background" Value="Blue"/>
    <Setter Property="Foreground" Value="Yellow"/>
    <Setter Property="Padding" Value="8"/>
    <Setter Property="Template">
        <ControlTemplate>
            <Panel>
                <Ellipse Fill="{TemplateBinding Background}"
                         HorizontalAlignment="Stretch"
                         VerticalAlignment="Stretch"/>
                <ContentPresenter Name="PART_ContentPresenter"
                                  Content="{TemplateBinding Content}"
                                  Margin="{TemplateBinding Padding}"/>
            </Panel>
        </ControlTemplate>
    </Setter>
</ControlTheme>
```

Apply:
```xml
<Button Theme="{StaticResource EllipseButton}"
        HorizontalAlignment="Center"
        VerticalAlignment="Center">
    Hello World!
</Button>
```

---

## 6. Navigation

### Simple navigation with ContentControl + DataTemplates

**ViewModels:**
```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableObject _currentPage;

    public MainViewModel()
    {
        _currentPage = new HomeViewModel();
    }

    [RelayCommand]
    private void GoHome() => CurrentPage = new HomeViewModel();

    [RelayCommand]
    private void GoSettings() => CurrentPage = new SettingsViewModel();
}
```

**XAML (MainWindow.axaml):**
```xml
<Window x:DataType="vm:MainViewModel">
    <Window.DataTemplates>
        <DataTemplate DataType="vm:HomeViewModel">
            <views:HomeView />
        </DataTemplate>
        <DataTemplate DataType="vm:SettingsViewModel">
            <views:SettingsView />
        </DataTemplate>
    </Window.DataTemplates>

    <Grid RowDefinitions="Auto,*">
        <StackPanel Orientation="Horizontal" Spacing="8" Margin="8">
            <Button Content="Home" Command="{Binding GoHomeCommand}" />
            <Button Content="Settings" Command="{Binding GoSettingsCommand}" />
        </StackPanel>

        <ContentControl Grid.Row="1" Content="{Binding CurrentPage}" />
    </Grid>
</Window>
```

### Animated transitions (TransitioningContentControl)

```xml
<TransitioningContentControl Content="{Binding CurrentPage}">
    <TransitioningContentControl.PageTransition>
        <PageSlide Duration="0:0:00.300" Orientation="Horizontal" />
    </TransitioningContentControl.PageTransition>
</TransitioningContentControl>
```

### Combined transition (slide + fade)

```xml
<TransitioningContentControl.PageTransition>
    <CompositePageTransition>
        <CrossFade Duration="0:0:0.2" />
        <PageSlide Duration="0:0:0.3" Orientation="Horizontal" />
    </CompositePageTransition>
</TransitioningContentControl.PageTransition>
```

### DataTemplate in Window resources (reuse across views)

```xml
<Window.DataTemplates>
    <DataTemplate DataType="vm:HomeViewModel">
        <views:HomeView />
    </DataTemplate>
</Window.DataTemplates>
```

---

## 7. Dialogs & Windows

### Open a modal dialog

```csharp
// Get the parent window
var parentWindow = TopLevel.GetTopLevel(this) as Window;

// Create and show dialog
var dialog = new MyDialog();
var result = await dialog.ShowDialog<string>(parentWindow);
```

### Dialog with result

**Dialog window:**
```csharp
public class ConfirmDialog : Window
{
    private void OnOkClick(object? sender, EventArgs e)
    {
        Close(true); // Returns true to caller
    }

    private void OnCancelClick(object? sender, EventArgs e)
    {
        Close(false);
    }
}
```

**Caller:**
```csharp
var dialog = new ConfirmDialog();
dialog.DataContext = new ConfirmDialogViewModel("Delete this item?");

bool? result = await dialog.ShowDialog<bool?>(parentWindow);
if (result == true)
{
    DeleteItem();
}
```

### Getting the main window anywhere

```csharp
var lifetime = Application.Current?.ApplicationLifetime
    as IClassicDesktopStyleApplicationLifetime;

var mainWindow = lifetime?.MainWindow;
```

### Opening from a ViewModel (with window service)

```csharp
public interface IDialogService
{
    Task<T?> ShowDialog<T>(WindowViewModelBase viewModel);
}

public class DialogService : IDialogService
{
    private readonly IServiceProvider _services;

    public DialogService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<T?> ShowDialog<T>(WindowViewModelBase viewModel)
    {
        var lifetime = Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime;

        var window = new Window
        {
            DataContext = viewModel,
            Width = 400,
            Height = 300
        };

        return await window.ShowDialog<T>(lifetime!.MainWindow);
    }
}
```

---

## 8. HttpClient & API Integration

### Image loading from web

```csharp
using System.Net.Http;
using Avalonia.Media.Imaging;

public static class ImageHelper
{
    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
            return null;
        }
    }
}
```

### Generic API service

```csharp
using System.Net.Http.Json;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
}

public class ApiService : IApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _http.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var response = await _http.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
```

### ViewModel using API service

```csharp
public partial class UserListViewModel : ViewModelBase
{
    private readonly IApiService _api;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    public UserListViewModel(IApiService api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsLoading = true;
        try
        {
            var users = await _api.GetAsync<List<UserDto>>("/api/users");
            Users = new ObservableCollection<UserDto>(users ?? []);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Loading indicator in XAML

```xml
<Grid>
    <!-- Loading spinner -->
    <ProgressBar IsIndeterminate="True"
                 IsVisible="{Binding IsLoading}"
                 VerticalAlignment="Top" />

    <!-- Content -->
    <DataGrid ItemsSource="{Binding Users}"
              IsVisible="{Binding IsLoading, Converter={StaticResource BoolInvert}}" />
</Grid>
```

---

## 9. Dependency Injection

### Step 1: Install NuGet package

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
```

### Step 2: Register services in App.axaml.cs

```csharp
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    private IServiceProvider? _services;

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IDialogService, DialogService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<UserListViewModel>();
        services.AddTransient<SettingsViewModel>();

        _services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### Step 3: Extension method (clean registration)

```csharp
public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<UserListViewModel>();
    }
}
```

Usage in `App.axaml.cs`:
```csharp
var services = new ServiceCollection();
services.AddCommonServices();
_services = services.BuildServiceProvider();
```

### HttpClient with typed client

```csharp
services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

---

## 10. CommunityToolkit.Mvvm Deep Dive

### ObservableProperty attribute

```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    // Generates:
    //   public string Name
    //   {
    //       get => _name;
    //       set => SetProperty(ref _name, value);
    //   }
}
```

### Property changed notification

```csharp
[ObservableProperty]
private string _fullName = string.Empty;

partial void OnFullNameChanged(string value)
{
    // Called automatically when FullName changes
    // Also notifies dependent properties:
    OnPropertyChanged(nameof(DisplayName));
}

public string DisplayName => $"User: {FullName}";
```

### RelayCommand variations

```csharp
// Synchronous command
[RelayCommand]
private void Save() { }

// Async command
[RelayCommand]
private async Task LoadData() { }

// With CanExecute
[RelayCommand(CanExecute = nameof(CanSubmit))]
private void Submit() { }
private bool CanSubmit() => IsValid;

// With parameter
[RelayCommand]
private void SelectItem(Item item) { }

// Async with parameter + CancellationToken
[RelayCommand]
private async Task ProcessAsync(int id, CancellationToken ct) { }
```

### ObservableObject manually

```csharp
public class MyViewModel : ObservableObject
{
    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public void Update()
    {
        // Manual notification
        OnPropertyChanged(nameof(Title));
    }
}
```

### ObservableCollection best practice

```csharp
[ObservableProperty]
private ObservableCollection<Item> _items = new();

// Always assign a new collection, don't modify in place
// Use await Dispatcher.UIThread.InvokeAsync() for cross-thread updates
```

---

## 11. Compiled Bindings

### Enable project-wide (already enabled in this project)

```xml
<PropertyGroup>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
</PropertyGroup>
```

With this setting, all bindings **require** `x:DataType` — missing it causes build errors.

### Declare DataType for compiled bindings

```xml
<!-- On Window/UserControl -->
<Window x:DataType="vm:MainWindowViewModel" x:CompileBindings="True">

<!-- On specific controls (inherits DataType) -->
<StackPanel x:DataType="vm:MainWindowViewModel">
    <TextBlock Text="{Binding Name}" />  <!-- Compiled -->
</StackPanel>
```

### Opt out for dynamic properties

```xml
<TextBlock Text="{ReflectionBinding SomeDynamicProperty}" />
```

### Benefits of compiled bindings

- **Compile-time validation**: Typo in property name → build error
- **Performance**: No runtime reflection
- **IntelliSense**: Better autocomplete in XAML
- **Refactoring**: Rename property → all bindings update

---

## 12. Custom Controls

### Templated control (simple)

```csharp
public class ToggleLabel : TemplatedControl
{
    public static readonly StyledProperty<string> LabelTextProperty =
        AvaloniaProperty.Register<ToggleLabel, string>(
            nameof(LabelText), "Default");

    public string LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }
}
```

### Control theme for templated control

```xml
<!-- Themes/Generic.axaml -->
<ResourceDictionary>
    <ControlTheme x:Key="{x:Type local:ToggleLabel}"
                  TargetType="local:ToggleLabel">
        <Setter Property="Template">
            <ControlTemplate>
                <Border Background="{TemplateBinding Background}" Padding="8">
                    <TextBlock Text="{TemplateBinding LabelText}" />
                </Border>
            </ControlTemplate>
        </Setter>
    </ControlTheme>
</ResourceDictionary>
```

### UserControl (composition of existing controls)

```xml
<UserControl x:Class="MyApp.Views.UserCard"
             xmlns:vm="using:MyApp.ViewModels"
             x:DataType="vm:UserViewModel">
    <Border Padding="16" CornerRadius="8"
            Background="{DynamicResource CardBackground}">
        <StackPanel Spacing="4">
            <TextBlock Text="{Binding FullName}" FontSize="18" FontWeight="Bold" />
            <TextBlock Text="{Binding Email}" Foreground="Gray" />
        </StackPanel>
    </Border>
</UserControl>
```

---

## 13. Troubleshooting & Best Practices

### Common issues

| Issue | Fix |
|---|---|
| **Binding not working** | Missing `x:DataType` or property name typo |
| **"Not Found: ViewName"** | Ensure View namespace matches ViewModel namespace |
| **Compiled binding error** | Add `x:DataType` or use `ReflectionBinding` |
| **Dialog not modal** | Use `ShowDialog()` not `Show()` |
| **DataGrid not found** | Install `Avalonia.Controls.DataGrid` NuGet package |
| **Cross-thread UI access** | Use `await Dispatcher.UIThread.InvokeAsync(() => ...)` |
| **Theme not applying** | Ensure `<FluentTheme />` is in `Application.Styles` |
| **Image not loading** | Use `avares://assembly/path` URI scheme |

### Best practices

1. **Use compiled bindings** — enabled globally in csproj
2. **Always specify `x:DataType`** in XAML files
3. **Use `[ObservableProperty]`** instead of manual property implementations
4. **Use `[RelayCommand]`** instead of manual ICommand classes
5. **Use DI container** for services and ViewModels (not manual instantiation)
6. **Keep ViewModels testable** — inject dependencies, avoid static references
7. **Use `ObservableCollection<T>`** for lists, not `List<T>`
8. **Separate API logic** into service classes, not in ViewModels
9. **Use async commands** for I/O operations (`[RelayCommand]` on `async Task` methods)
10. **Format strings** in XAML: `{Binding Price, StringFormat='{}{0:C}'}`

### Debugging bindings

```csharp
// Enable binding logging in Program.cs
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .WithInterFont()
    .LogToTrace();  // Shows binding errors in debug output
```

---

> **Reference**: Full documentation at [docs.avaloniaui.net](https://docs.avaloniaui.net)  
> **API docs**: [api-docs.avaloniaui.net](https://api-docs.avaloniaui.net)  
> **Community**: [avaloniaui.net](https://avaloniaui.net)
