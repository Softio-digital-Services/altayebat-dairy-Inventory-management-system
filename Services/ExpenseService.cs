using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class ExpenseService
    {
        public void ProcessRecurringExpenses()
        {
            try
            {
                string currentMonth = DateTime.Now.ToString("yyyy-MM");

                // Use the most recent recurring record per category/description as the template source.
                string sqlTemplates = @"
                    SELECT category, amount, description, recorded_by 
                    FROM expenses 
                    WHERE is_recurring = 1 
                    AND expense_id IN (SELECT MAX(expense_id) FROM expenses WHERE is_recurring = 1 GROUP BY category, description)";

                DataTable templates = DatabaseHelper.ExecuteDataTable(sqlTemplates);

                if (templates.Rows.Count > 0)
                {
                    foreach (DataRow row in templates.Rows)
                    {
                        // Skip if this recurring template was already generated for this month.
                        int alreadyExists = DatabaseHelper.ExecuteScalar<int>(
                            @"SELECT COUNT(*) FROM expenses
                              WHERE is_recurring = 1
                                AND date_deleted IS NULL
                                AND strftime('%Y-%m', expense_date) = @month
                                AND IFNULL(category,'') = @cat
                                AND IFNULL(description,'') = @desc",
                            new SqliteParameter("@month", currentMonth),
                            new SqliteParameter("@cat", row["category"]?.ToString() ?? string.Empty),
                            new SqliteParameter("@desc", row["description"]?.ToString() ?? string.Empty));
                        if (alreadyExists > 0) continue;

                        // Create a new unpaid record for the current month.
                        string sqlInsert = @"
                            INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_paid, is_recurring, last_processed_month)
                            VALUES (@cat, @date, @amt, @desc, @usr, 0, 1, @month)";

                        DatabaseHelper.ExecuteNonQuery(sqlInsert,
                            new SqliteParameter("@cat", row["category"]),
                            new SqliteParameter("@date", DateTime.Now),
                            new SqliteParameter("@amt", row["amount"]),
                            new SqliteParameter("@desc", row["description"].ToString() + " (Auto-Generated)"),
                            new SqliteParameter("@usr", "System"),
                            new SqliteParameter("@month", currentMonth));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ProcessRecurringExpenses failed");
            }
        }

        public int GetUnpaidExpensesCount()
        {
            return DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM expenses WHERE is_paid = 0 AND date_deleted IS NULL");
        }

        public decimal GetTotalExpenses(int month, int year)
        {
            string sql = "SELECT SUM(amount) FROM expenses WHERE MONTH(expense_date) = @m AND YEAR(expense_date) = @y AND date_deleted IS NULL";
            object result = DatabaseHelper.ExecuteScalar<object>(sql, 
                new SqliteParameter("@m", month),
                new SqliteParameter("@y", year));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public bool MarkAsPaid(int expenseId)
        {
            return DatabaseHelper.ExecuteNonQuery("UPDATE expenses SET is_paid = 1 WHERE expense_id = @id",
                new SqliteParameter("@id", expenseId));
        }

        public bool MarkAsUnpaid(int expenseId)
        {
            return DatabaseHelper.ExecuteNonQuery("UPDATE expenses SET is_paid = 0 WHERE expense_id = @id",
                new SqliteParameter("@id", expenseId));
        }
    }
}
