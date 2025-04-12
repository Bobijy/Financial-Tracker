using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Transaction
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // "Income" or "Expense"
    public string Category { get; set; }
    public DateTime Date { get; set; }

    public Transaction(string description, decimal amount, string type, string category, DateTime date)
    {
        Description = description;
        Amount = amount;
        Type = type;
        Category = category;
        Date = date;
    }

    public override string ToString()
    {
        return $"{Date.ToShortDateString()} | {Type} | {Category} | {Amount:C} | {Description}";
    }
}

class BudgetTracker
{
    private List<Transaction> transactions = new List<Transaction>();

    public void AddTransaction(Transaction transaction)
    {
        transactions.Add(transaction);
    }

    public decimal GetTotalIncome() =>
        transactions.Where(t => t.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);

    public decimal GetTotalExpenses() =>
        transactions.Where(t => t.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);

    public decimal GetNetSavings() => GetTotalIncome() - GetTotalExpenses();

    public Dictionary<string, decimal> GetSpendingByCategory()
    {
        return transactions
            .Where(t => t.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public void SortTransactions(string sortBy)
    {
        switch (sortBy.ToLower())
        {
            case "date":
                transactions = transactions.OrderBy(t => t.Date).ToList();
                break;
            case "amount":
                transactions = transactions.OrderByDescending(t => t.Amount).ToList();
                break;
            case "category":
                transactions = transactions.OrderBy(t => t.Category).ToList();
                break;
        }
    }

    public void DisplaySummary()
    {
        Console.WriteLine($"\nTotal Income: {GetTotalIncome():C}");
        Console.WriteLine($"Total Expenses: {GetTotalExpenses():C}");
        Console.WriteLine($"Net Savings: {GetNetSavings():C}\n");

        Console.WriteLine("Spending by Category:");
        var categories = GetSpendingByCategory();
        foreach (var category in categories)
        {
            Console.WriteLine($"{category.Key}: {category.Value:C}");
        }

        if (categories.Count > 0)
        {
            var maxCategory = categories.OrderByDescending(kv => kv.Value).FirstOrDefault();
            Console.WriteLine($"\nMost Spent Category: {maxCategory.Key} - {maxCategory.Value:C}");
        }
    }

    public void DisplayTextGraph()
    {
        Console.WriteLine("\n--- Expense Chart ---");
        foreach (var category in GetSpendingByCategory())
        {
            Console.Write($"{category.Key}: ");
            int barLength = (int)(category.Value / 10); // scale
            Console.WriteLine(new string('#', barLength));
        }
    }

    public void SaveToFile(string path)
    {
        using StreamWriter writer = new StreamWriter(path);
        foreach (var t in transactions)
        {
            writer.WriteLine($"{t.Description}|{t.Amount}|{t.Type}|{t.Category}|{t.Date}");
        }
    }

    public void LoadFromFile(string path)
    {
        if (!File.Exists(path)) return;

        transactions.Clear();
        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split('|');
            var transaction = new Transaction(
                parts[0],
                decimal.Parse(parts[1]),
                parts[2],
                parts[3],
                DateTime.Parse(parts[4])
            );
            transactions.Add(transaction);
        }
    }

    public void DisplayAllTransactions()
    {
        Console.WriteLine("\n--- All Transactions ---");
        foreach (var t in transactions)
        {
            Console.WriteLine(t);
        }
    }
}

class Program
{
    static void Main()
    {
        BudgetTracker tracker = new BudgetTracker();
        string filePath = "transactions.txt";
        tracker.LoadFromFile(filePath);

        while (true)
        {
            Console.WriteLine("\n1. Add Transaction\n2. View Summary\n3. View Transactions\n4. Sort Transactions\n5. Save & Exit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write("Description: ");
                        string desc = Console.ReadLine();

                        Console.Write("Amount: ");
                        decimal amt = decimal.Parse(Console.ReadLine());

                        Console.Write("Type (Income/Expense): ");
                        string type = Console.ReadLine();

                        Console.Write("Category: ");
                        string category = Console.ReadLine();

                        Console.Write("Date (yyyy-mm-dd): ");
                        DateTime date = DateTime.Parse(Console.ReadLine());

                        tracker.AddTransaction(new Transaction(desc, amt, type, category, date));
                        Console.WriteLine("Transaction added.");
                        break;

                    case "2":
                        tracker.DisplaySummary();
                        tracker.DisplayTextGraph();
                        break;

                    case "3":
                        tracker.DisplayAllTransactions();
                        break;

                    case "4":
                        Console.Write("Sort by (date/amount/category): ");
                        tracker.SortTransactions(Console.ReadLine());
                        break;

                    case "5":
                        tracker.SaveToFile(filePath);
                        Console.WriteLine("Data saved. Exiting...");
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}