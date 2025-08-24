using Microsoft.Extensions.Configuration;
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

		//temp token for testing
		public string?Token {  get; private set; }

		public AuthService (HttpClient http, IConfiguration config)
		{
			_http = http;
			_config = config;

			var baseUrl = _config["ApiBaseUrl"]
				?? throw new InvalidOperationException("ApiBaseUrl missing in appsettings.json");
			_http.BaseAddress = new Uri(baseUrl);
		}

		public async Task<(bool ok, string? error)> RegisterAsync(string fullName, string email, string password)
		{
			var res = await _http.PostAsJsonAsync("api/Auth/register", new { fullName, email, password });
			if (res.IsSuccessStatusCode) return (true, null);
			return (false, await res.Content.ReadAsStringAsync());
		}

		public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
		{
			var res = await _http.PostAsJsonAsync("api/Auth/login", new { email, password });
			if (!res.IsSuccessStatusCode) return (false, await res.Content.ReadAsStringAsync());

			var payload = await res.Content.ReadFromJsonAsync<TokenDto>();
			if (string.IsNullOrWhiteSpace(payload?.Token)) return (false, "Empty token from server.");

			Token = payload!.Token;
			_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
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

		public Task LogoutAsync()
		{
			Token = null;
			_http.DefaultRequestHeaders.Authorization = null;
			return Task.CompletedTask;
		}

		// example call to a protected endpoint
		public Task<HttpResponseMessage> GetMeAsync()
			=> _http.GetAsync("auth/me");

		private sealed class TokenDto { public string Token { get; set; } = ""; }
	

	}
}
