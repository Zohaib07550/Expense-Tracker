using ExpenseTracker.Services;
using expensetracker2._0.DTO;
using expensetracker2._0.Interface;
using expensetracker2._0.Models;
using expensetracker2._0.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace expensetracker2._0.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncomeSourcesController : ControllerBase
    {
        private readonly IIncomeSources _incomeSourcesService;

        public IncomeSourcesController(IIncomeSources incomeSourcesService)
        {
            _incomeSourcesService = incomeSourcesService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeSource>> GetIncomeSource(int id)
        {
            var incomeSource = await _incomeSourcesService.GetIncomeSourceByIdAsync(id);
            if (incomeSource == null)
            {
                return NotFound();
            }

            return Ok(incomeSource);
        }
        [HttpPut("income-sources/{id}")]
        public IActionResult UpdateIncomeSource(int id, [FromBody] IncomeSourcesDto incomeSourceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _incomeSourcesService.UpdateIncomeSource(id, incomeSourceDto);
                return NoContent(); // HTTP 204 No Content
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                return StatusCode(500, new { Message = "An error occurred while updating the Income Source.", Details = ex.Message });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchExpenses([FromQuery] string query)
        {
            var incomeSource = _incomeSourcesService.SearchIncomeSource(query);
            return Ok(incomeSource);
        }

        [HttpGet("filter")]
        public IActionResult IncomeSources([FromQuery] int category, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var incomeSource = _incomeSourcesService.FilterIncomeSource(category, startDate, endDate);
            return Ok(incomeSource);

        }

        [HttpPost("income-sources")]
        public async Task<IActionResult> AddOrCreateIncomeSourceAsync([FromBody] IncomeSourcesDto incomeSourcesDto)
        {
            try
            {
                var result = await _incomeSourcesService.AddOrCreateIncomeSourceAsync(incomeSourcesDto);
                return Ok(result); // Return a success response
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if necessary
                return StatusCode(500, new { Message = "Internal server error.", Details = ex.Message });
            }
        }

        [HttpDelete("incomeSource/{id}")]
        public async Task<IActionResult> DeleteIncomeSource(int id)
        {
            if (await _incomeSourcesService.DeleteIncomeSourceAsync(id))
            {
                return Ok("Income source deleted successfully.");
            }
            else
            {
                return NotFound("Income source not found.");
            }
        }


        [HttpGet("GetIncomeSourcesByCategory/{name}")]
        public async Task<IActionResult> GetIncomeSourcesByCategory(string name)
        {
            try
            {
                var incomeSources = await _incomeSourcesService.GetIncomeSourcesAndEntriesByCategoryNameAsync(name);

                if (incomeSources == null || !incomeSources.Any())
                {
                    return NotFound("No income sources found for the given category name.");
                }

                return Ok(incomeSources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}