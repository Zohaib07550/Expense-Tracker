using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using expensetracker2._0.Models;
using expensetracker2._0.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using expensetracker2._0.models;

namespace ExpenseTracker.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ExpenseDbContext _dbContext;

        public ExpenseController(IExpenseService expenseService, ExpenseDbContext dbContext)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost("AddOrCreateExpense")]
        public async Task<IActionResult> AddOrCreateExpense([FromBody] ExpenseDto expenseDto)
        {
            try
            {
                var result = await _expenseService.AddOrCreateExpenseAsync(expenseDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetExpenseAndDetailsByCategoryName")]
        public async Task<IActionResult> GetExpensesAndDetailsByCategoryName(string categoryName)
        {
            var expensesDto = await _expenseService.GetExpenseAndDetailsByCategoryNameAsync(categoryName);
            if (expensesDto.Count == 0)
            {
                return NotFound($"No expenses found for category '{categoryName}'.");
            }

            return Ok(expensesDto);
        }


        //[HttpGet("GetStoredProcedure/{categoryName}")]
        //public IActionResult MyAction(string categoryName)
        //{
            //var results = _expenseService.GetExpensesAndDetailsByCategoryName(categoryName);
           // return Ok(results);
       // }



        [HttpGet("GetViewStatement/{categoryName}")]
        public IActionResult Index(string categoryName)
        {
            var results = _expenseService.GetExpenseCategoryViewsByCategoryName(categoryName);
            return Ok(results);
        }

        [HttpGet]
        public IActionResult GetExpenses()
        {
            var expenses = _expenseService.GetExpenses();
            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public IActionResult GetExpense(int id)
        {
            var expense = _expenseService.GetExpenseById(id);
            if (expense == null)
            {
                return NotFound();
            }
            return Ok(expense);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateExpense(int id, [FromBody] ExpenseDto expenseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingExpense = _expenseService.GetExpenseById(id);
            if (existingExpense == null)
            {
                return NotFound();
            }

            try
            {
                _expenseService.UpdateExpense(id, expenseDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the expense: {ex.Message}");
            }
        }
        [HttpDelete("expense/{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            if (await _expenseService.DeleteExpenseAsync(id))
            {
                return Ok("Expense deleted successfully.");
            }
            else
            {
                return NotFound("Expense not found.");
            }
        }

        [HttpGet("search")]
        public IActionResult SearchExpenses([FromQuery] string query)
        {
            var expenses = _expenseService.SearchExpenses(query);
            return Ok(expenses);
        }

        [HttpGet("filter")]
        public IActionResult FilterExpenses([FromQuery] int category, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var expenses = _expenseService.FilterExpenses(category, startDate, endDate);
            return Ok(expenses);
        }
    }
}



