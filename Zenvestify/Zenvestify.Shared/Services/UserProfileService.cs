using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Zenvestify.Shared.Models;
using static Zenvestify.Shared.Models.UserProfileDtos;

namespace Zenvestify.Shared.Services
{
	public class UserProfileService
	{
		private readonly HttpClient _http;
		private readonly IConfiguration _config;
		private readonly IJSRuntime _js;

		public UserProfileService(HttpClient http, IConfiguration config, IJSRuntime js)
		{
			_http = http;
			_config = config;
			_js = js;

			var baseUrl = _config["ApiBaseUrl"]
				?? throw new InvalidOperationException("ApiBaseUrl missing in appsettings.json");

			//_http.BaseAddress = new Uri(baseUrl);
		}

		//Primary Income/Income
		public async Task<HttpResponseMessage> SetIncomeAsync(IncomeDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/income", dto);
		}

		public async Task<IncomeDto?> GetIncomeAsync()
		{
			return await _http.GetFromJsonAsync<IncomeDto>("api/userprofile/income");
		}

		//Secondary job/Other income sources
		public async Task<HttpResponseMessage> AddOtherIncomeAsync(OtherIncomeDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/otherincome", dto);
		}

		public async Task<List<OtherIncomeDto>?> GetOtherIncomeAsync()
		{
			return await _http.GetFromJsonAsync<List<OtherIncomeDto>>("api/userprofile/otherincome");
		}

		//Savings
		public async Task<HttpResponseMessage> AddSavingsGoalAsync(SavingsGoalDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/savings", dto);
		}

		public async Task<List<SavingsGoalDto>?> GetSavingsGoalsAsync()
		{
			return await _http.GetFromJsonAsync<List<SavingsGoalDto>>("api/userprofile/savings");
		}

		//Bills
		public async Task<HttpResponseMessage> AddBillAsync(BillDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/bills", dto);
		}

		public async Task<List<BillDto>?> GetBillsAsync()
		{
			return await _http.GetFromJsonAsync<List<BillDto>>("api/userprofile/bills");
		}

		//Loans
		public async Task<HttpResponseMessage> AddLoanAsync(LoanDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/loans", dto);
		}

		public async Task<List<LoanDto>?> GetLoansAsync()
		{
			return await _http.GetFromJsonAsync<List<LoanDto>>("api/userprofile/loans");
		}

		//Tax
		public async Task<HttpResponseMessage> SetTaxSettingsAsync(TaxSettingsDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/tax", dto);
		}

		public async Task<TaxSettingsDto?> GetTaxSettingsAsync()
		{
			return await _http.GetFromJsonAsync<TaxSettingsDto>("api/userprofile/tax");
		}

		//Expenses
		public async Task<HttpResponseMessage> AddExpenseAsync(ExpenseDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/expense", dto);
		}

		public async Task<List<ExpenseDto>?> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
		{
			string query = "";
			if (from.HasValue && to.HasValue)
				query = $"?from={from.Value:yyyy-MM-dd}&to={to.Value:yyyy-MM-dd}";

			return await _http.GetFromJsonAsync<List<ExpenseDto>>($"api/userprofile/expenses{query}");
		}

		//Onboarding complete
		public async Task<HttpResponseMessage> CompleteOnboardingAsync(CompleteOnboardingDto dto)
		{
			return await _http.PostAsJsonAsync("api/userprofile/complete", dto);
		}
	}
}
