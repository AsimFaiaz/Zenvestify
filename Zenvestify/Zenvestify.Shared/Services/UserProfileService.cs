using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Zenvestify.Shared.Models;
using static Zenvestify.Shared.Models.UserProfileDtos;
using System.Net.Http.Headers;

namespace Zenvestify.Shared.Services
{
	public class UserProfileService
	{
		//private readonly HttpClient _http;
		//private readonly IConfiguration _config;
		//private readonly IJSRuntime _js;

		private readonly AuthService _auth;

		public UserProfileService(AuthService auth)
		{
			_auth = auth;
		}

		private HttpClient Http => _auth.Http;

		//public UserProfileService(HttpClient http, IConfiguration config, IJSRuntime js)
		//{
		//	_http = http;
		//	_config = config;
		//	_js = js;

		//	var baseUrl = _config["ApiBaseUrl"]
		//		?? throw new InvalidOperationException("ApiBaseUrl missing in appsettings.json");

		//	_http.BaseAddress = new Uri(baseUrl);

		//	EnsureAuthHeaderAsync().GetAwaiter().GetResult();
		//}

		//private async Task EnsureAuthHeaderAsync()
		//{
		//	if (OperatingSystem.IsBrowser())
		//	{
		//		var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

		//		if (!string.IsNullOrWhiteSpace(token))
		//		{
		//			_http.DefaultRequestHeaders.Authorization =
		//				new AuthenticationHeaderValue("Bearer", token);
		//		}
		//	}
		//}


		//Primary Income/Income
		public async Task<HttpResponseMessage> SetIncomeAsync(IncomeDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/income", dto);
		}

		public async Task<IncomeDto?> GetIncomeAsync()
		{
			return await Http.GetFromJsonAsync<IncomeDto>("api/userprofile/income");
		}

		//Secondary job/Other income sources
		public async Task<HttpResponseMessage> AddOtherIncomeAsync(OtherIncomeDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/otherincome", dto);
		}

		public async Task<List<OtherIncomeDto>?> GetOtherIncomeAsync()
		{
			return await Http.GetFromJsonAsync<List<OtherIncomeDto>>("api/userprofile/otherincome");
		}

		public async Task<HttpResponseMessage> UpdateOtherIncomeAsync(Guid id, OtherIncomeDto dto)
		{
			return await Http.PutAsJsonAsync($"api/userprofile/otherincome/{id}", dto);
		}

		public async Task<HttpResponseMessage> DeleteOtherIncomeAsync(Guid id)
		{
			return await Http.DeleteAsync($"api/userprofile/otherincome/{id}");
		}

		//Transactions
		public async Task<HttpResponseMessage> AddIncomeTransactionAsync(IncomeTransactionDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/income/transaction", dto);
		}

		public async Task<List<IncomeTransactionDto>?> GetIncomeTransactionsAsync(Guid sourceId)
		{
			return await Http.GetFromJsonAsync<List<IncomeTransactionDto>>($"api/userprofile/income/transactions/{sourceId}");
		}

		public async Task<HttpResponseMessage> UpdateIncomeTransactionAsync(Guid id, IncomeTransactionDto dto)
		{
			return await Http.PutAsJsonAsync($"api/userprofile/income/transaction/{id}", dto);
		}

		public async Task<HttpResponseMessage> DeleteIncomeTransactionAsync(Guid id)
		{
			return await Http.DeleteAsync($"api/userprofile/income/transaction/{id}");
		}

		//Savings
		public async Task<HttpResponseMessage> AddSavingsGoalAsync(SavingsGoalDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/savings", dto);
		}

		public async Task<List<SavingsGoalDto>?> GetSavingsGoalsAsync()
		{
			return await Http.GetFromJsonAsync<List<SavingsGoalDto>>("api/userprofile/savings");
		}

		//Bills
		public async Task<HttpResponseMessage> AddBillAsync(BillDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/bills", dto);
		}

		public async Task<List<BillDto>?> GetBillsAsync()
		{
			return await Http.GetFromJsonAsync<List<BillDto>>("api/userprofile/bills");
		}

		//Loans
		public async Task<HttpResponseMessage> AddLoanAsync(LoanDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/loans", dto);
		}

		public async Task<List<LoanDto>?> GetLoansAsync()
		{
			return await Http.GetFromJsonAsync<List<LoanDto>>("api/userprofile/loans");
		}

		//Tax
		public async Task<HttpResponseMessage> SetTaxSettingsAsync(TaxSettingsDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/tax", dto);
		}

		public async Task<TaxSettingsDto?> GetTaxSettingsAsync()
		{
			return await Http.GetFromJsonAsync<TaxSettingsDto>("api/userprofile/tax");
		}

		//Expenses
		public async Task<HttpResponseMessage> AddExpenseAsync(ExpenseDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/expense", dto);
		}

		public async Task<List<ExpenseDto>?> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
		{
			string query = "";
			if (from.HasValue && to.HasValue)
				query = $"?from={from.Value:yyyy-MM-dd}&to={to.Value:yyyy-MM-dd}";

			return await Http.GetFromJsonAsync<List<ExpenseDto>>($"api/userprofile/expenses{query}");
		}

		//Onboarding complete
		public async Task<HttpResponseMessage> CompleteOnboardingAsync(CompleteOnboardingDto dto)
		{
			return await Http.PostAsJsonAsync("api/userprofile/complete", dto);
		}
	}
}
