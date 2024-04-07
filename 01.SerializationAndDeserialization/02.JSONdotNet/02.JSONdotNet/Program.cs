using _02.JSONdotNet;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        // Serialization from C# to JSON

        Product pump = new Product(0, "Oil Pump", null, 25.0);
        Product filter = new Product(0, "Oil Filter", null, 15.0);

        pump.DisplayProductInfo();
        filter.DisplayProductInfo();

        string firstProductJson = JsonConvert.SerializeObject(pump, Formatting.Indented);
        Console.WriteLine(firstProductJson);

        string secondProductJson = JsonConvert.SerializeObject(filter, Formatting.Indented);
        Console.WriteLine(secondProductJson);

        // Serialization from JSON to C#

        string jsonString = File.ReadAllText(Path.Combine(Environment.CurrentDirectory + "/../../../Products.json"));

        var objProducts = JsonConvert.DeserializeObject<Dictionary<string, Product>>(jsonString);

        foreach (var kvp in objProducts)
        {
            Product product = kvp.Value;
            Console.WriteLine($"Product Type: {kvp.Key},{Environment.NewLine}Name: {product.Name},{Environment.NewLine}Cost: {product.Cost}");
        }
    }
}