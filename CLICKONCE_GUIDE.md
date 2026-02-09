# Hướng dẫn Publish với ClickOnce

## Cách 1: Dùng Visual Studio
1. Mở AIVanBan.sln
2. Right-click AIVanBan.Desktop project → Publish
3. Chọn target: Folder / Web Server / Microsoft Azure
4. Cấu hình:
   - Publish mode: Self-contained hoặc Framework-dependent
   - Target runtime: win-x64
   - File publish options
5. Click "Publish"

## Cách 2: Dùng Command Line
```powershell
# Publish ClickOnce
dotnet publish AIVanBan.Desktop\AIVanBan.Desktop.csproj `
    -c Release `
    -p:PublishProfile=ClickOnceProfile `
    -p:ApplicationVersion=1.0.0.0

# Tạo publish profile nếu chưa có
msbuild AIVanBan.Desktop\AIVanBan.Desktop.csproj `
    /t:Publish `
    /p:Configuration=Release `
    /p:PublishUrl="D:\AIVanBanCaNhan\publish\" `
    /p:InstallUrl="https://yourwebsite.com/aivanban/" `
    /p:ProductName="AIVanBan Desktop" `
    /p:PublisherName="Your Company" `
    /p:ApplicationVersion=1.0.0.0
```

## Lợi ích ClickOnce
- Tự động update qua network
- Dễ deploy
- No admin rights required
- Built-in .NET

## Hạn chế
- Cần hosting nếu muốn auto-update
- Ít tùy chỉnh hơn
