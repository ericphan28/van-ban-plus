// Quick test script - run with: dotnet-script test-upload.csx
// Or just compile as part of a console app

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = "http://localhost:3010";
var email = "cym_sunset@yahoo.com";
var password = "Tnt@9961266";
var testImage = @"D:\AIVanBanCaNhan\image\AI_NGHIEP_VU_ROADMAP\1770883800658.png";

var handler = new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(15) };
var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };

// Login
Console.WriteLine("=== Login ===");
var loginResp = await client.PostAsJsonAsync($"{baseUrl}/api/upload/auth", new { email, password });
var loginJson = await loginResp.Content.ReadAsStringAsync();
Console.WriteLine($"Login status: {loginResp.StatusCode}");
var loginObj = JsonDocument.Parse(loginJson);
var token = loginObj.RootElement.GetProperty("access_token").GetString();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
Console.WriteLine("Token set OK");

// Create album
Console.WriteLine("\n=== Create Album ===");
var albumResp = await client.PostAsJsonAsync($"{baseUrl}/api/upload/albums", new { title = "C# Test Upload" });
var albumJson = await albumResp.Content.ReadAsStringAsync();
Console.WriteLine($"Album status: {albumResp.StatusCode}");
Console.WriteLine($"Album response: {albumJson.Substring(0, Math.Min(200, albumJson.Length))}");
var albumObj = JsonDocument.Parse(albumJson);
var albumId = albumObj.RootElement.GetProperty("id").GetString();

// Upload image - exact same way as AlbumUploadService
Console.WriteLine("\n=== Upload Image ===");
var boundary = $"----Upload{Guid.NewGuid():N}";
var form = new MultipartFormDataContent(boundary);
form.Headers.Remove("Content-Type");
form.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundary}");

var fileBytes = await File.ReadAllBytesAsync(testImage);
var fileContent = new ByteArrayContent(fileBytes);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
form.Add(fileContent, "file", "test.png");
form.Add(new StringContent("false"), "is_cover");

Console.WriteLine($"File size: {fileBytes.Length} bytes");
Console.WriteLine($"Content-Type: {form.Headers.ContentType}");

// Dump the actual request body to check boundary
using var debugMs = new MemoryStream();
await form.CopyToAsync(debugMs);
var bodyStr = System.Text.Encoding.UTF8.GetString(debugMs.ToArray());
Console.WriteLine($"Body starts with: {bodyStr.Substring(0, Math.Min(200, bodyStr.Length))}");

// Reset stream position for actual upload
var form2 = new MultipartFormDataContent(boundary);
form2.Headers.Remove("Content-Type");
form2.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundary}");
var fileContent2 = new ByteArrayContent(fileBytes);
fileContent2.Headers.ContentType = new MediaTypeHeaderValue("image/png");
form2.Add(fileContent2, "file", "test.png");
form2.Add(new StringContent("false"), "is_cover");

var response = await client.PostAsync($"{baseUrl}/api/upload/albums/{albumId}/images", form2);
var respBody = await response.Content.ReadAsStringAsync();
Console.WriteLine($"\nUpload status: {response.StatusCode}");
Console.WriteLine($"Response: {respBody.Substring(0, Math.Min(300, respBody.Length))}");

client.Dispose();
