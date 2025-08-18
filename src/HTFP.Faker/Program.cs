using System.Globalization;
using System.Text;
using Bogus;
using HTFP.Shared.Models;

Console.WriteLine("Create test file wizard!");
Console.OutputEncoding = Encoding.UTF8;

Console.Write("How many ExecutionOrder objects do you want to generate? ");
if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
{
    Console.WriteLine("Invalid number.");
    return;
}

Console.Write("What is the file name? ");
var fileName = Console.ReadLine();

// Faker configuration
var faker = new Faker<ExecutionOrder>("en")
    .RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
    .RuleFor(o => o.ExternalId, f => f.Random.AlphaNumeric(10))
    .RuleFor(o => o.DateTime, f =>
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1); // yesterday, 00:00:00
                var randomTime = TimeSpan.FromMilliseconds(f.Random.Int(0, 86_399_999)); 
                return yesterday.Add(randomTime); // yesterday + random time
            })
    .RuleFor(o => o.AssetId, f => f.Finance.Currency().Code)
    .RuleFor(o => o.TradingAccount, f => f.Finance.Account())
    .RuleFor(o => o.Quantity, f => f.Random.Int(1, 1000))
    .RuleFor(o => o.UnitPrice, f => f.Finance.Amount(1, 100));

var orders = faker.Generate(count);

string filePath = $"../HTFP.FileSpliter/Samples/{fileName}.csv";
using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

foreach (var order in orders)
{
    var line = string.Join(",",
        order.Id,
        order.ExternalId,
        order.DateTime.ToString("O", CultureInfo.InvariantCulture), // ISO 8601
        order.AssetId,
        order.TradingAccount,
        order.Quantity,
        order.UnitPrice.ToString(CultureInfo.InvariantCulture));

    writer.WriteLine(line);
}

Console.WriteLine($"✅ {count} execution orders saved to {filePath}");