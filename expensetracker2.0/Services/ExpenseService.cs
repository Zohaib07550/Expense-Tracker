using expensetracker2._0.DTO;
using expensetracker2._0.models;
using expensetracker2._0.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class ExpenseService : IExpenseService

    {
        private readonly ExpenseDbContext _dbContext;

        public ExpenseService(ExpenseDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        //PROPER DATA MAPPING OF 3 TABLES AND GETTING VALUES
        public async Task<List<ExpenseDto>> GetExpenseAndDetailsByCategoryNameAsync(string categoryName)
        {
            var expensesDto = new List<ExpenseDto>();

            try
            {
                // Find the category by name
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
                if (category != null)
                {
                    // Fetch expenses and details based on category ID
                    var expensesWithDetails = await _dbContext.Expenses
                        .Include(e => e.ExpenseDetails)
                        .Where(e => e.CategoryId == category.CategoryId)
                        .ToListAsync();

                    // Map to DTOs
                    expensesDto = expensesWithDetails.Select(expense => new ExpenseDto
                    {
                        Description = expense.Description,
                        CategoryId = expense.CategoryId ?? 0, // Ensure non-null value for CategoryId
                        ExpenseDetails = expense.ExpenseDetails.Select(detail => new ExpenseDetailDto
                        {
                            Amount = detail.Amount ?? 0, // Ensure non-null value for Amount
                            Date = detail.Date ?? DateTime.MinValue // Ensure non-null value for Date
                        }).ToList()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Error in GetExpensesAndDetailsByCategoryNameAsync: {ex.Message}");
            }

            return expensesDto;
        }



        public IEnumerable<Expense> GetExpenses()
        {
            return _dbContext.Expenses.ToList();
        }

        public Expense GetExpenseById(int id)
        {
            return _dbContext.Expenses.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Expense> SearchExpenses(string query)
        {
            var lowerCaseQuery = query.ToLower();
            return _dbContext.Expenses
                             .Where(e => e.Description != null && e.Description.ToLower().Contains(lowerCaseQuery))
                             .ToList();
        }


        public IEnumerable<Expense> FilterExpenses(int category, DateTime startDate, DateTime endDate)
        {
            return _dbContext.Expenses
                            .Where(e => e.Date >= startDate && e.Date <= endDate)
                            .ToList();
        }

        public void AddExpense(Expense expense)
        {
            try
            {
                _dbContext.Expenses.Add(expense);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding the expense: {ex.Message}");
                throw;
            }
        }

        public void UpdateExpense(int id, ExpenseDto expenseDto)
        {
            try
            {
                var existingExpense = _dbContext.Expenses
                    .Include(e => e.ExpenseDetails)
                    .FirstOrDefault(e => e.Id == id);

                if (existingExpense != null)
                {
                    // Update expense fields
                    existingExpense.Description = expenseDto.Description;
                    existingExpense.CategoryId = expenseDto.CategoryId;

                    // Update expense details
                    existingExpense.ExpenseDetails.Clear();
                    foreach (var detail in expenseDto.ExpenseDetails)
                    {
                        existingExpense.ExpenseDetails.Add(new ExpenseDetail
                        {
                            Amount = detail.Amount,
                            Date = detail.Date
                        });
                    }

                    _dbContext.SaveChanges();
                }
                else
                {
                    throw new InvalidOperationException($"Expense with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the expense: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            try
            {
                var expenseToRemove = await _dbContext.Expenses
                    .Include(e => e.ExpenseDetails)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expenseToRemove == null)
                {
                    return false; // Expense with ID not found
                }

                // Remove the associated expense details
                _dbContext.ExpenseDetails.RemoveRange(expenseToRemove.ExpenseDetails);

                // Remove the expense itself
                _dbContext.Expenses.Remove(expenseToRemove);

                await _dbContext.SaveChangesAsync();

                return true; // Expense and associated details deleted successfully
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                throw new Exception("An error occurred while deleting the expense.", ex);
            }
        }

        //PROPER RELATION OF 3 TABLES TO POST THE VALUES
        public async Task<IActionResult> AddOrCreateExpenseAsync(ExpenseDto expenseDto)
        {
            try
            {
                // Find the category associated with the expense
                var category = await _dbContext.Categories
                    .Include(c => c.Budgets)
                    .Include(c => c.Expenses)
                        .ThenInclude(e => e.ExpenseDetails)
                    .FirstOrDefaultAsync(c => c.CategoryId == expenseDto.CategoryId);

                if (category == null)
                {
                    return new NotFoundObjectResult("Category not found.");
                }

                // Calculate total current expenses
                var totalCurrentExpenses = category.Expenses
                    .SelectMany(e => e.ExpenseDetails)
                    .Sum(ed => ed.Amount);

                // Calculate total expenses after adding the new expense
                var newExpenseTotal = expenseDto.ExpenseDetails.Sum(ed => ed.Amount);
                var totalAfterNewExpense = totalCurrentExpenses + newExpenseTotal;

                // Check if any of the associated budgets are exceeded
                foreach (var budgetDto in category.Budgets)
                {
                    if (budgetDto.Budget1.HasValue && totalAfterNewExpense > budgetDto.Budget1.Value)
                    {
                        return new BadRequestObjectResult("A predefined allowance or limit for acceptable errors or failures within a system or process.");
                    }
                }

                // If an existing expense with the same description exists, update its details
                var existingExpense = category.Expenses.FirstOrDefault(e => e.Description == expenseDto.Description);

                if (existingExpense == null)
                {
                    // Create a new expense
                    var newExpense = new Expense
                    {
                        Description = expenseDto.Description,
                        CategoryId = expenseDto.CategoryId
                    };

                    _dbContext.Expenses.Add(newExpense);

                    // Add expense details
                    foreach (var detailDto in expenseDto.ExpenseDetails)
                    {
                        var expenseDetail = new ExpenseDetail
                        {
                            Amount = detailDto.Amount,
                            Date = detailDto.Date,
                            Expense = newExpense
                        };
                        _dbContext.ExpenseDetails.Add(expenseDetail);
                    }

                    await _dbContext.SaveChangesAsync();

                    return new OkObjectResult("Expense created and details added successfully.");
                }
                else
                {
                    // Update existing expense details
                    foreach (var detailDto in expenseDto.ExpenseDetails)
                    {
                        var expenseDetail = new ExpenseDetail
                        {
                            Amount = detailDto.Amount,
                            Date = detailDto.Date,
                            Expense = existingExpense
                        };
                        _dbContext.ExpenseDetails.Add(expenseDetail);
                    }

                    await _dbContext.SaveChangesAsync();

                    return new OkObjectResult("Expense details added to existing expense successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }




        //Calling The STORED PROCEDURE BY USING BACKEND
        // public List<ExpenseDetailsByCategory> GetExpensesAndDetailsByCategoryName(string categoryName)
        // {
        //   return _dbContext.ExpenseDetailsByCategory
        //     .FromSqlRaw("EXEC GetExpensesAndDetailsByCategoryName @CategoryName",
        //       new SqlParameter("@CategoryName", categoryName))
        //   .ToList();
        // }


        //Calling The VEIW STATEMENT BY USING BACKEND
        public List<ExpenseCategoryView> GetExpenseCategoryViewsByCategoryName(string categoryName)
        {
            var expenses = _dbContext.ExpenseCategoryViews
                .FromSqlRaw("SELECT * FROM ExpenseCategoryView WHERE CategoryName = @CategoryName",
                    new SqlParameter("@CategoryName", categoryName))
                .ToList();

            return expenses; // Return the list of expenses
        }
    }
}
