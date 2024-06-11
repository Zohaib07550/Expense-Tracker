using expensetracker2._0.DTO;
using expensetracker2._0.models;
using expensetracker2._0.Models;
using Microsoft.AspNetCore.Mvc;

public interface IExpenseService
{
    IEnumerable<Expense> GetExpenses();
    Expense GetExpenseById(int id);
    IEnumerable<Expense> SearchExpenses(string query);
    IEnumerable<Expense> FilterExpenses(int category, DateTime startDate, DateTime endDate);
    void AddExpense(Expense expense);
    void UpdateExpense(int id, ExpenseDto expenseDto);
    Task<bool> DeleteExpenseAsync(int id);
    Task<IActionResult> AddOrCreateExpenseAsync(ExpenseDto expenseDto);
    Task<List<ExpenseDto>> GetExpenseAndDetailsByCategoryNameAsync(string categoryName);
   // List<ExpenseDetailsByCategory> GetExpensesAndDetailsByCategoryName(string categoryName);
    List<ExpenseCategoryView> GetExpenseCategoryViewsByCategoryName(string categoryName);
}

