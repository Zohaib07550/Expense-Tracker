using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using expensetracker2._0.Models;
using expensetracker2._0.DTO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _categoryService.GetCategories();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto categoryDto)
        {
            var result = await _categoryService.CreateCategoryAsync(categoryDto);
            if (!result)
            {
                return BadRequest("Failed to create category.");
            }

            return Ok("Category created successfully.");
        }

        [HttpGet("{categoryName}")]
        public async Task<IActionResult> GetExpensesAndIncomeSourcesByCategoryName(string categoryName)
        {
            try
            {
                var (expensesDto, incomeSourcesDto) = await _categoryService.GetExpensesAndIncomeSourcesByCategoryNameAsync(categoryName);

                if (expensesDto == null && incomeSourcesDto == null)
                {
                    return NotFound($"No data found for category '{categoryName}'.");
                }

                var result = new
                {
                    Expenses = expensesDto,
                    IncomeSources = incomeSourcesDto
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Error in GetExpensesAndIncomeSourcesByCategoryName: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
    }

        [HttpPut("category/{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] BudgetDto budgetDto)
        {
            try
            {
                await _categoryService.UpdateCategoryAsync(categoryId, budgetDto);
                return NoContent(); // HTTP 204 No Content
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                return StatusCode(500, new { Message = "An error occurred while updating the category.", Details = ex.Message });
            }
        }

        [HttpDelete("category/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            if (await _categoryService.DeleteCategoryAsync(categoryId))
            {
                return Ok("Category deleted successfully.");
            }
            else
            {
                return NotFound("Category not found.");
            }
        }
    }
}
