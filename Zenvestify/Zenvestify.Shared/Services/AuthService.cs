using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Zenvestify.Shared.Services
{
	public class AuthService
	{
		private readonly HttpClient _http;
		private readonly IConfiguration _config;
		private readonly IJSRuntime _js;

		//temp token for testing

		// Let pages await until token is loaded
		private readonly TaskCompletionSource<bool> _readyTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public Task WhenReady => _readyTcs.Task;
		public string?Token {  get; private set; }

		private sealed class TokenDto { public string Token { get; set; } = ""; }

		public AuthService (HttpClient http, IConfiguration config, IJSRuntime js)
		{
			_http = http;
			_config = config;
			_js = js;

			var baseUrl = _config["ApiBaseUrl"]
				?? throw new InvalidOperationException("ApiBaseUrl missing in appsettings.json");
			_http.BaseAddress = new Uri(baseUrl);
		}

		public HttpClient Http => _http;

		public async Task<(bool ok, string? error)> RegisterAsync(string fullName, string email, string password)
		{
			var res = await _http.PostAsJsonAsync("api/Auth/register", new { fullName, email, password });
			if (res.IsSuccessStatusCode) return (true, null);
			return (false, await res.Content.ReadAsStringAsync());
		}

		public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
		{
			Console.WriteLine($"[AuthService.LoginAsync] Login start for {email}");

			var res = await _http.PostAsJsonAsync("api/Auth/login", new { email, password });

			Console.WriteLine($"[AuthService.LoginAsync] HTTP Status={res.StatusCode}");

			if (!res.IsSuccessStatusCode)
			{
				var err = await res.Content.ReadAsStringAsync();
				Console.WriteLine($"[AuthService.LoginAsync] ERROR: {err}");
				return (false, err);
				//return (false, await res.Content.ReadAsStringAsync());
			}

			var payload = await res.Content.ReadFromJsonAsync<TokenDto>();

			Console.WriteLine($"[AuthService.LoginAsync] Token received={payload?.Token?.Substring(0, 20)}...");
			
			if (string.IsNullOrWhiteSpace(payload?.Token))
				
				return (false, "Empty token from server.");

			Token = payload!.Token;
			await _js.InvokeVoidAsync("localStorage.setItem", "authToken", Token);

			Console.WriteLine($"[AuthService.LoginAsync] Token saved to localStorage");

			_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

			Console.WriteLine($"[AuthService.LoginAsync] Authorization header set ✅");

			if (!_readyTcs.Task.IsCompleted) _readyTcs.TrySetResult(true);

			return (true, null);
		}

		public async Task<(bool ok, string? error)> RequestPasswordReserAsync(string email)
		{
			var res = await _http.PostAsJsonAsync("api/Auth/forgotpassword", new { email });
			if (res.IsSuccessStatusCode) return (true, null);
			return (false, await res.Content.ReadAsStringAsync());
		}

		public async Task<(bool ok, string? error)> ResetPasswordAsync(string token, string newPassword)
		{
			var res = await _http.PostAsJsonAsync("api/Auth/resetpassword", new { token, newPassword });
			if (res.IsSuccessStatusCode) return (true, null);
			return (false, await res.Content.ReadAsStringAsync());
		}

		public async Task LogoutAsync()
		{
			Token = null;
			_http.DefaultRequestHeaders.Authorization = null;
			await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
			//return Task.CompletedTask;
		}

		public async Task LoadTokenAsync()
		{
			if (OperatingSystem.IsBrowser())
			{

				Token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
				Console.WriteLine($"[AuthService.LoadTokenAsync] Loaded token from localStorage = {Token?.Substring(0, 20)}...");


				if (!string.IsNullOrWhiteSpace(Token))
				{
					_http.DefaultRequestHeaders.Authorization =
						new AuthenticationHeaderValue("Bearer", Token);

					Console.WriteLine("[AuthService.LoadTokenAsync] Authorization header set");
				}
			}

			_readyTcs.TrySetResult(true);
		}

		// example call to a protected endpoint
		public Task<HttpResponseMessage> GetMeAsync()
			=> _http.GetAsync("api/auth/me");
	}
}
