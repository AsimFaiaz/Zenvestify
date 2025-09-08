using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zenvestify.Web.Data;
using Zenvestify.Web.Models;
using static Zenvestify.Shared.Models.UserProfileDtos;

namespace Zenvestify.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UserProfileController : ControllerBase
	{
		private readonly UserRepository _userRepository;

		public UserProfileController(UserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		private Guid GetUserId()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("User not authenticated");
			return Guid.Parse(userId);
		}


		//On boarding
		[HttpPost("complete")]
		public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			// Save onboarding data
			await _userRepository.UpdateOnboardingStatusAsync(Guid.Parse(userId), 2);

			return Ok(new { message = "Onboarding completed." });
		}

		//Income
		[HttpPost("income")]
		public async Task<IActionResult> SetIncome([FromBody] IncomeDto dto)
		{
			var userId = GetUserId();
			await _userRepository.SetIncomeAsync(userId, dto.PayFrequency, dto.NetPayPerCycle, dto.GrossPayPerCycle, dto.TaxWithheld, dto.UsualPayDay);
			return Ok();
		}


		[HttpGet("income")]
		public async Task<IActionResult> GetIncome()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetIncomeAsync(userId);
			return Ok(data);
		}


		//Secondary Income
		[HttpPost("otherincome")]
		public async Task<IActionResult> AddOtherIncome([FromBody] OtherIncomeDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddOtherIncomeAsync(userId, dto.Source, dto.Amount, dto.Frequency);
			return Ok();
		}

		[HttpGet("otherincome")]
		public async Task<IActionResult> GetOtherIncome()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetOtherIncomeAsync(userId);
			return Ok(data);
		}

	
		[HttpPut("otherincome/{id:guid}")]
		public async Task<IActionResult> UpdateOtherIncome(Guid id, [FromBody] OtherIncomeDto dto)
		{
			await _userRepository.UpdateOtherIncomeAsync(id, dto.Source, dto.Amount, dto.Frequency);
			return Ok(new { message = "Secondary Income updated successfully" });
		}

		
		[HttpDelete("otherincome/{id:guid}")]
		public async Task<IActionResult> DeleteOtherIncome(Guid id)
		{
			var userId = GetUserId();
			await _userRepository.DeleteOtherIncomeAsync(id, userId);
			return Ok(new { message = "Secondary Income deleted successfully" });
		}

		//Income Transactions

		[HttpPost("income/transaction")]
		public async Task<IActionResult> AddIncomeTransaction([FromBody] IncomeTransactionDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddIncomeTransactionAsync(userId, dto.SourceId, dto.TxnDate, dto.GrossAmount, dto.NetAmount, dto.Notes);
			return Ok(new { message = "Transaction added successfully" });
		}


		[HttpGet("income/transactions/{sourceId:guid}")]
		public async Task<IActionResult> GetIncomeTransactions(Guid sourceId)
		{
			var userId = GetUserId();
			var data = await _userRepository.GetIncomeTransactionsAsync(userId, sourceId);
			return Ok(data);
		}



		[HttpPut("income/transaction/{id:guid}")]
		public async Task<IActionResult> UpdateIncomeTransaction(Guid id, [FromBody] IncomeTransactionDto dto)
		{
			var userId = GetUserId();
			await _userRepository.UpdateIncomeTransactionAsync(userId, id, dto.TxnDate, dto.GrossAmount, dto.NetAmount, dto.Notes);
			return Ok(new { message = "Transaction updated successfully" });
		}


		[HttpDelete("income/transaction/{id:guid}")]
		public async Task<IActionResult> DeleteIncomeTransaction(Guid id)
		{
			var userId = GetUserId();
			await _userRepository.DeleteIncomeTransactionAsync(userId, id);
			return Ok(new { message = "Transaction deleted successfully" });
		}

		//Saving
		[HttpPost("savings")]
		public async Task<IActionResult> AddSavings([FromBody] SavingsGoalDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddSavingsGoalAsync(userId, dto.Name, dto.TargetAmount, dto.TargetDate, dto.AmountSavedToDate ?? 0, dto.Status);
			return Ok();
		}

		[HttpGet("savings")]
		public async Task<IActionResult> GetSavings()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetSavingsGoalsAsync(userId);
			return Ok(data);
		}

		//Bills
		[HttpPost("bills")]
		public async Task<IActionResult> AddBill([FromBody] BillDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddBillAsync(userId, dto.Name, dto.Amount, dto.Frequency, dto.FirstDueDate);
			return Ok();
		}

		[HttpGet("bills")]
		public async Task<IActionResult> GetBills()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetBillsAsync(userId);
			return Ok(data);
		}

		//Loans
		[HttpPost("loans")]
		public async Task<IActionResult> AddLoan([FromBody] LoanDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddLoanAsync(userId, dto);
			return Ok(new { message = "Loan added successfully" });
		}

		[HttpGet("loans")]
		public async Task<IActionResult> GetLoans()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetLoansAsync(userId);
			return Ok(data);
		}

		//Tax
		[HttpPost("tax")]
		public async Task<IActionResult> SetTax([FromBody] TaxSettingsDto dto)
		{
			var userId = GetUserId();
			await _userRepository.SetTaxSettingsAsync(userId, dto.TaxWithheldPerCycle, dto.MedicareLevyExempt, dto.PrivateHealthCover);
			return Ok();
		}

		[HttpGet("tax")]
		public async Task<IActionResult> GetTax()
		{
			var userId = GetUserId();
			var data = await _userRepository.GetTaxSettingsAsync(userId);
			return Ok(data);
		}

		//Expenses
		[HttpPost("expense")]
		public async Task<IActionResult> AddExpense([FromBody] ExpenseDto dto)
		{
			var userId = GetUserId();
			await _userRepository.AddExpenseAsync(userId, dto.Category, dto.Amount, dto.DateSpent, dto.Notes, dto.IsTaxDeductible, dto.IsWorkRelated);
			return Ok();
		}

		[HttpGet("expenses")]
		public async Task<IActionResult> GetExpenses(DateTime? from = null, DateTime? to = null)
		{
			var userId = GetUserId();
			var data = await _userRepository.GetExpensesAsync(userId, from, to);
			return Ok(data);
		}

	}	
}
