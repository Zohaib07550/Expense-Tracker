using System;
using System.Collections.Generic;
using System.Linq;
using expensetracker2._0.DTO;
using expensetracker2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ExpenseDbContext _dbContext;

        public CategoryService(ExpenseDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Category> GetCategories()
        {
            return _dbContext.Categories.ToList();
        }
        public async Task<bool> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            if (!ValidateCategoryDto(categoryDto))
            {
                return false;
            }

            var category = new Category
            {
                CategoryId = categoryDto.CategoryId,
                Name = categoryDto.Name
            };

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Check if the budget amount is greater than 0.00
            if (categoryDto.Budget1.HasValue && categoryDto.Budget1 > 0.00m && !string.IsNullOrEmpty(categoryDto.Interval))
            {
                var budget = new Budget
                {
                    CategoryId = category.CategoryId,
                    Budget1 = categoryDto.Budget1,
                    Interval = categoryDto.Interval,
                    Description = categoryDto.Description
                };

                await _dbContext.Budgets.AddAsync(budget);
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }
            private bool ValidateCategoryDto(CreateCategoryDto categoryDto)
        {
            // Add your validation logic here
            // For example, you can check if Name is not null or empty
            return !string.IsNullOrEmpty(categoryDto.Name);
        }

        public async Task<(List<ExpenseDto>, List<IncomeSourcesDto>)> GetExpensesAndIncomeSourcesByCategoryNameAsync(string categoryName)
        {
            var expensesDto = new List<ExpenseDto>();
            var incomeSourcesDto = new List<IncomeSourcesDto>();

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

                    // Map expenses to DTOs
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

                    // Fetch income sources and entries based on category ID
                    var incomeSourcesWithEntries = await _dbContext.IncomeSources
                        .Include(i => i.IncomeEntries)
                        .Where(i => i.CategoryId == category.CategoryId)
                        .ToListAsync();

                    // Map income sources to DTOs
                    incomeSourcesDto = incomeSourcesWithEntries.Select(incomeSource => new IncomeSourcesDto
                    {
                        Description = incomeSource.Description,
                        CategoryId = incomeSource.CategoryId ?? 0, // Ensure non-null value for CategoryId
                        IncomeEntry = incomeSource.IncomeEntries.Select(entry => new IncomeEntryDto
                        {
                            Amount = entry.Amount ?? 0, // Ensure non-null value for Amount
                            Date = entry.Date ?? DateTime.MinValue // Ensure non-null value for Date
                        }).ToList()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Error in GetExpensesAndIncomeSourcesByCategoryNameAsync: {ex.Message}");
            }

            return (expensesDto, incomeSourcesDto);
        }
        public async Task UpdateCategoryAsync(int categoryId, BudgetDto budgetDto)
        {
            try
            {
                var existingCategory = await _dbContext.Categories
                    .Include(c => c.Budgets)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (existingCategory != null)
                {
                    // Check if the category already has a budget, interval, and description
                    var existingBudget = await _dbContext.Budgets
                        .FirstOrDefaultAsync(b => b.CategoryId == categoryId);

                    if (existingBudget != null)
                    {
                        // Update existing budget properties
                        existingBudget.Budget1 = budgetDto.Budget1;
                        existingBudget.Interval = budgetDto.Interval;
                        existingBudget.Description = budgetDto.Description;
                    }
                    else
                    {
                        // Create new budget properties
                        var newBudget = new Budget
                        {
                            CategoryId = categoryId,
                            Budget1 = budgetDto.Budget1,
                            Interval = budgetDto.Interval,
                            Description = budgetDto.Description
                        };
                        _dbContext.Budgets.Add(newBudget);
                    }

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException($"Category with ID {categoryId} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the category: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _dbContext.Categories
                    .Include(c => c.Budgets)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    return false; // Category with ID not found
                }

                // Remove the associated budgets first
                _dbContext.Budgets.RemoveRange(category.Budgets);

                // Remove the category itself
                _dbContext.Categories.Remove(category);

                await _dbContext.SaveChangesAsync();

                return true; // Category and associated budgets deleted successfully
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                throw new Exception("An error occurred while deleting the category.", ex);
            }
        }
    }
}
