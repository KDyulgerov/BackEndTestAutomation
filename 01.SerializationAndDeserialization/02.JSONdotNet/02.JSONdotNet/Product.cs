using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _02.JSONdotNet
{
    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public double Cost { get; set; }

        public Product(int id, string name, string? description, double cost)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Cost = cost;

        }

        public void DisplayProductInfo()
        {
            Console.WriteLine($"ID: {Id},{Environment.NewLine}Name: {Name},{Environment.NewLine}Description: {Description},{Environment.NewLine}Cost: {Cost}");
        }
    }
}
