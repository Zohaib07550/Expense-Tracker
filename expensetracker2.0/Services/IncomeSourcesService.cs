using expensetracker2._0.DTO;
using expensetracker2._0.Interface;
using expensetracker2._0.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;  // Ensure this is included for IEnumerable
using System.Linq;
using System.Threading.Tasks;

namespace expensetracker2._0.Services
{
    public class IncomeSourcesService : IIncomeSources  // Implement the interface
    {
        private readonly ExpenseDbContext _dbContext;

        public IncomeSourcesService(ExpenseDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IncomeSource> GetIncomeSourceByIdAsync(int id)
        {
            return _dbContext.IncomeSources.FirstOrDefault(e => e.Id == id);
        }

        public async Task<List<IncomeSourcesDto>> GetIncomeSourcesAndEntriesByCategoryNameAsync(string categoryName)
        {
            var incomeSourcesDto = new List<IncomeSourcesDto>();

            try
            {
                // Find the category by name
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
                if (category != null)
                {
                    // Fetch income sources and entries based on category ID
                    var incomeSourcesWithEntries = await _dbContext.IncomeSources
                        .Include(i => i.IncomeEntries)
                        .Where(i => i.CategoryId == category.CategoryId)
                        .ToListAsync();

                    // Map to DTOs
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
                Console.WriteLine($"Error in GetIncomeSourcesAndEntriesByCategoryNameAsync: {ex.Message}");
            }

            return incomeSourcesDto;
        }


        public void UpdateIncomeSource(int id, IncomeSourcesDto incomeSourceDto)
        {
            try
            {
                var existingIncomeSource = _dbContext.IncomeSources
                    .Include(i => i.IncomeEntries)
                    .FirstOrDefault(i => i.Id == id);

                if (existingIncomeSource != null)
                {
                    // Update income source fields
                    existingIncomeSource.Description = incomeSourceDto.Description;
                    existingIncomeSource.CategoryId = incomeSourceDto.CategoryId;

                    // Remove existing income entries
                    foreach (var entry in existingIncomeSource.IncomeEntries.ToList())
                    {
                        _dbContext.IncomeEntries.Remove(entry);
                    }

                    // Add new income entries
                    foreach (var entry in incomeSourceDto.IncomeEntry)
                    {
                        existingIncomeSource.IncomeEntries.Add(new IncomeEntry
                        {
                            Amount = entry.Amount,
                            Date = entry.Date
                        });
                    }

                    _dbContext.SaveChanges();
                }
                else
                {
                    throw new InvalidOperationException($"Income Source with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the income source: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<IncomeSource> SearchIncomeSource(string query)
        {
            var lowerCaseQuery = query.ToLower();
            return _dbContext.IncomeSources
                             .Where(e => e.Description != null && e.Description.ToLower().Contains(lowerCaseQuery))
                             .ToList();
        }

        public IEnumerable<IncomeSource> FilterIncomeSource(int category, DateTime startDate, DateTime endDate)
        {
            return _dbContext.IncomeSources
                            .Where(e => e.Date >= startDate && e.Date <= endDate)
                            .ToList();
        }
        public async Task<bool> DeleteIncomeSourceAsync(int id)
        {
            try
            {
                var incomeSourceToRemove = await _dbContext.IncomeSources
                    .Include(i => i.IncomeEntries)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (incomeSourceToRemove == null)
                {
                    return false; // IncomeSource with ID not found
                }

                // Remove associated income entries
                _dbContext.IncomeEntries.RemoveRange(incomeSourceToRemove.IncomeEntries);

                // Remove the income source itself
                _dbContext.IncomeSources.Remove(incomeSourceToRemove);

                await _dbContext.SaveChangesAsync();

                return true; // IncomeSource and associated entries deleted successfully
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                throw new Exception("An error occurred while deleting the income source.", ex);
            }
        }

        public async Task<IActionResult> AddOrCreateIncomeSourceAsync(IncomeSourcesDto incomeSourcesDto)
        {
            try
            {
                var existingIncomeSource = await _dbContext.IncomeSources
                    .FirstOrDefaultAsync(e => e.Description == incomeSourcesDto.Description && e.CategoryId == incomeSourcesDto.CategoryId);

                if (existingIncomeSource == null)
                {
                    var newIncomeSource = new IncomeSource
                    {
                        Description = incomeSourcesDto.Description,
                        CategoryId = incomeSourcesDto.CategoryId // Assigning category ID to the new income source
                    };

                    _dbContext.IncomeSources.Add(newIncomeSource);
                    await _dbContext.SaveChangesAsync();

                    foreach (var incomeEntryDto in incomeSourcesDto.IncomeEntry)
                    {
                        // Check if the date is within the valid range
                        if (incomeEntryDto.Date >= DateTime.MinValue && incomeEntryDto.Date <= DateTime.MaxValue)
                        {
                            var incomeEntry = new IncomeEntry
                            {
                                Amount = incomeEntryDto.Amount,
                                Date = incomeEntryDto.Date,
                                IncomeSourceId = newIncomeSource.Id
                            };
                            _dbContext.IncomeEntries.Add(incomeEntry);
                        }
                        else
                        {
                            // Handle the case where the date is outside the valid range
                            throw new ArgumentOutOfRangeException("Date must be between 1/1/1753 and 12/31/9999.");
                        }
                    }

                    await _dbContext.SaveChangesAsync();

                    return new OkObjectResult("Income source created and entries added successfully.");
                }
                else
                {
                    foreach (var incomeEntryDto in incomeSourcesDto.IncomeEntry)
                    {
                        // Check if the date is within the valid range
                        if (incomeEntryDto.Date >= DateTime.MinValue && incomeEntryDto.Date <= DateTime.MaxValue)
                        {
                            var incomeEntry = new IncomeEntry
                            {
                                Amount = incomeEntryDto.Amount,
                                Date = incomeEntryDto.Date,
                                IncomeSourceId = existingIncomeSource.Id
                            };
                            _dbContext.IncomeEntries.Add(incomeEntry);
                        }
                        else
                        {
                            // Handle the case where the date is outside the valid range
                            throw new ArgumentOutOfRangeException("Date must be between 1/1/1753 and 12/31/9999.");
                        }
                    }

                    await _dbContext.SaveChangesAsync();

                    return new OkObjectResult("Income entries added to existing income source successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
                throw;
            }
        }
    }
}
