# AGENTS.md — bizflow-desktop-app (Avalonia .NET 8 + C# MVVM)

## Cấu trúc dự án

```
bizflow-desktop-app/
├── Models/                      # Domain models (DTOs từ backend)
│   ├── Product.cs
│   ├── User.cs
│   └── ApiResponse.cs
├── ViewModels/                  # MVVM ViewModels
│   ├── ViewModelBase.cs         # Base class (ObservableObject + RelayCommand)
│   ├── MainWindowViewModel.cs
│   ├── ProductListViewModel.cs
│   ├── ProductEditViewModel.cs
│   └── LoginViewModel.cs
├── Views/                       # Avalonia XAML views
│   ├── MainWindow.axaml
│   ├── ProductListView.axaml
│   ├── ProductEditView.axaml
│   └── LoginView.axaml
├── Services/                    # Business services
│   ├── IApiService.cs           # Refit interface
│   ├── ApiService.cs            # HTTP client implementation
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDialogService.cs
│   └── DialogService.cs
├── Converters/                  # XAML value converters
├── Assets/                      # Images, icons
├── App.axaml                    # App-level XAML (styles, resources)
├── App.axaml.cs                 # App startup logic
├── Program.cs                   # Entry point
├── bizflow-desktop-app.csproj
└── README.md
```

## Tech stack

- **.NET 8** + **Avalonia 11** (cross-platform: Windows, macOS, Linux)
- **CommunityToolkit.Mvvm** — source-generated MVVM (`[ObservableProperty]`, `[RelayCommand]`)
- **Refit** — type-safe HTTP client (interface-based)
- **FluentAvalonia** — FluentUI design system (Windows 11 look)
- **Microsoft.Extensions.DependencyInjection** — IoC container
- **Microsoft.Extensions.Http** — `IHttpClientFactory`

## MVVM rules

1. **View** (XAML) chỉ bind + handle events, KHÔNG có business logic.
2. **ViewModel** chứa state + commands, KHÔNG reference View.
3. **Model** là plain data class (DTO từ backend).
4. **Services** (API, Navigation, Dialog) inject vào ViewModel qua constructor.

### Source-generated MVVM pattern

Dùng `[ObservableProperty]` và `[RelayCommand]` thay cho manual implementation:

```csharp
public partial class ProductListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Product> products = new();
    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        var result = await _apiService.GetProductsAsync();
        Products = new ObservableCollection<Product>(result);
    }
}
```

Generated properties: `Products`, `LoadProductsCommand` (auto PascalCase).

## Naming conventions

| Type | Convention | Example |
|---|---|---|
| Class | PascalCase | `ProductListViewModel` |
| Interface | `I` + PascalCase | `IApiService` |
| Method | PascalCase | `GetProductsAsync` |
| Property | PascalCase | `ProductName` |
| Field (private) | `_camelCase` | `_apiService` |
| Field (generated) | `camelCase` | `products` (auto-getter `Products`) |
| Constant | PascalCase | `MaxPageSize` |
| Namespace | `PascalCase` | `Bizflow.Desktop.ViewModels` |

## C# rules

1. **No `var` abuse** — dùng explicit type khi không obvious. Cho LINQ, anonymous types → OK.
2. **No `dynamic`** — dùng generic + reflection nếu cần runtime dispatch.
3. **`async`/`await` cho I/O-bound**: KHÔNG `.Result` hoặc `.Wait()` (deadlock).
4. **Dispose properly**: dùng `using` cho `IDisposable` (HttpClient, Stream, etc.).
5. **Null safety**: bật `<Nullable>enable</Nullable>`, dùng `?` cho nullable references.
6. **Pattern matching**: prefer `is` check thay vì `as` + null check.
7. **Records** cho DTOs immutable (nếu phù hợp).
8. **No comments "what"** — code C# đã tự giải thích, chỉ comment "why".

## API client rules (Refit)

```csharp
public interface IApiService
{
    [Get("/api/products")]
    Task<PaginationResponse<Product>> GetProductsAsync([Query] int page = 1, [Query] int size = 20);
    
    [Post("/api/products")]
    Task<ApiResponse<Product>> CreateProductAsync([Body] CreateProductRequest request);
    
    [Put("/api/products/{id}")]
    Task<ApiResponse<Product>> UpdateProductAsync(string id, [Body] UpdateProductRequest request);
}
```

Setup trong `Program.cs`:
```csharp
builder.Services.AddRefitClient<IApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:8080"));
```

## Verification

```bash
dotnet build                          # Compile
dotnet run                            # Dev server
dotnet test                           # Tests (nếu có)
dotnet publish -c Release -r win-x64  # Production build cho Windows
```

## Future folder predictions

```
├── Modules/                  # Feature-based modules (khi scale)
│   ├── Products/
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   └── Services/
│   ├── Orders/
│   └── Reports/
├── Infrastructure/          # Cross-cutting (logging, caching, etc.)
│   ├── Logging/
│   ├── Caching/
│   └── Persistence/
├── Tests/                    # Unit + integration tests
│   ├── Unit/
│   └── Integration/
└── Plugins/                  # Optional plugins
```

## Anti-patterns cần tránh

- ❌ Business logic trong code-behind (`*.axaml.cs`)
- ❌ View reference trong ViewModel (coupling ngược)
- ❌ `.Result` / `.Wait()` cho async (deadlock)
- ❌ `async void` (ngoại trừ event handlers)
- ❌ Singleton ViewModel cho multiple windows (memory leak)
- ❌ Hard-code API URL (dùng configuration)
- ❌ Catch `Exception` chung (catch specific exceptions)
- ❌ Empty catch block
- ❌ Comment "what" — chỉ comment "why"
