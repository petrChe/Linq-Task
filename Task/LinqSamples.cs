// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();
        private int sumBorder = 100;
        private List<int> sumBorders = new List<int> { 10000, 20000, 50000, 100000 };

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        //Задание по модулю
        [Category("Homework")]
        [Title("1 task")]
        [Description("Sum of all the orders more than X")]
        public void Linq3_1()
        {
            var enumerator = sumBorders.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var customers = (from cust in dataSource.Customers
                                 where cust.Orders.Sum(ord => ord.Total) > enumerator.Current
                                 select new
                                 {
                                     Name = cust.CompanyName,
                                     Total = cust.Orders.Sum(ord => ord.Total)
                                 }).ToList();

                foreach (var cust in customers)
                {
                    Console.WriteLine(string.Format("Name : {0}, Orders total sum: {1}", cust.Name, cust.Total));
                }

                Console.WriteLine("==================================================================");
            }
        }

        [Category("Homework")]
        [Title("2 task")]
        [Description("For each customer select suppliers from the same country and city. With grouping and without")]
        public void Linq4_2()
        {
            Console.WriteLine("Without group by");

            var queryGrouping = (from cust in dataSource.Customers
                                 join supp in dataSource.Suppliers on new { country = cust.Country, city = cust.City } 
                                                               equals new { country = supp.Country, city = supp.City } 
                                 select new
                                 {
                                     CustName = cust.CompanyName,
                                     SuppName = supp.SupplierName,
                                     Country = cust.Country,
                                     City = cust.City
                                 }).ToList();

            foreach (var q in queryGrouping)
            {
                Console.WriteLine(string.Format("Customer {0} and Supplier {1} are locating in {2}, {3}",
                    q.CustName, q.SuppName, q.City, q.Country));
            }


            Console.WriteLine("With group by");

            var query = (from cust in dataSource.Customers
                         select new
                         {
                             CustName = cust.CompanyName,
                             Supps = from supp in dataSource.Suppliers
                                     where supp.Country == cust.Country
                                     group supp by cust.Country into countrySupps

                                     from countrySupp in countrySupps
                                     where countrySupp.City == cust.City
                                     group countrySupp by cust.City into citySupps
                                     
                                     from citySupp in citySupps
                                     select citySupp,
                             Country = cust.Country,
                             City = cust.City
                         }).Where(_ => _.Supps.Count() != 0).ToList();

            foreach (var q in query)
            {
                foreach (var sup in q.Supps)
                {
                    Console.WriteLine(string.Format("Customer {0} and Supplier {1} are locating in {2}, {3}",
                        q.CustName, sup.SupplierName, q.City, q.Country));
                }
            }
        }

        [Category("Homework")]
        [Title("3 task")]
        [Description("Each customer which orders are more than X ")]
        public void Linq5_3()
        {
            var query = (from cust in dataSource.Customers
                           from ord in cust.Orders
                           where ord.Total > sumBorder
                           select new
                           {
                               CustName = cust.CompanyName,
                               OrderId = ord.OrderID,
                               OrderTotal = ord.Total
                           }).ToList();

            foreach(var q in query)
                Console.WriteLine(string.Format("Customer - {0}, OrderId - {1}, OrderTotal - {2}",
                    q.CustName, q.OrderId, q.OrderTotal));                
        }

        [Category("Homework")]
        [Title("4 task")]
        [Description("Customers start date")]
        public void Linq6_4()
        {
            var customers = (from cust in dataSource.Customers
                             select new
                             {
                                 CustName = cust.CompanyName,
                                 FirstOrder = (from ord in cust.Orders
                                               orderby ord.OrderDate
                                               select ord).Take(1)

                             }).ToList();

            foreach (var cust in customers)
            {
                if(cust.FirstOrder.Count() != 0)
                    Console.WriteLine(string.Format("Customer - {0}, FirstOrderMonth - {1}, FirstOrderYear - {2}",
                        cust.CustName, cust.FirstOrder.First().OrderDate.Month, cust.FirstOrder.First().OrderDate.Year));
            }
        }

        [Category("Homework")]
        [Title("5 task")]
        [Description("Customers start date & sorting ")]
        public void Linq7_5()
        {
            var customers = (from cust in dataSource.Customers
                             select new
                             {
                                 CustName = cust.CompanyName,
                                 FirstOrder = (from ord in cust.Orders
                                               where cust.Orders.Count() != 0
                                               orderby ord.OrderDate
                                               select ord).Take(1)

                             }).Where(_ => _.FirstOrder.Count() != 0)
                               .OrderBy(_ => _.FirstOrder.First().OrderDate.Year)
                               .ThenBy(_ => _.FirstOrder.First().OrderDate.Month)
                               .ThenByDescending(_ => _.FirstOrder.First().Total)
                               .ThenBy(_ => _.CustName).ToList();

            foreach (var cust in customers)
            {
                if (cust.FirstOrder.Count() != 0)
                    Console.WriteLine(string.Format("Customer - {0}, Total - {1} FirstOrderMonth - {2}, FirstOrderYear - {3}",
                        cust.CustName, cust.FirstOrder.First().Total, cust.FirstOrder.First().OrderDate.Month, cust.FirstOrder.First().OrderDate.Year));
            }        
        }

        [Category("Homework")]
        [Title("6 task")]
        [Description("If customers have non-integer email code or region is empty or phone operator code is empty")]
        public void Linq8_6()
        {   
            Char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            var customers = dataSource.Customers
                                      .Where(cust => cust.PostalCode == null ||
                                                     !cust.PostalCode.All(c => numbers.Contains(c)) ||
                                                     cust.Region == null ||
                                                     cust.Phone.First() != '(')
                                          .Select(c => c);
        
            foreach(var cust in customers)
                Console.WriteLine(string.Format("Customer name - {0}, Postal code - {1}, Region - {2}, Phone - {3}",
                    cust.CompanyName, cust.PostalCode, cust.Region, cust.Phone));
        }

        [Category("Homework")]
        [Title("7 task")]
        [Description("Group products by categories, then by existense in stock, then by price")]
        public void Linq9_7()
        {
            var products = (from product in dataSource.Products
                            group product by product.Category into productCategories
                            select new
                            {
                                ProductCategories = productCategories,
                                ProductsInStock = from productCat in productCategories
                                                  group productCat by productCat.UnitsInStock != 0 into productsInStock
                                                  select new
                                                  {
                                                      ProductsInStock = productsInStock,
                                                      ProductsGroupingByPrice = from productInStock in productsInStock
                                                                                group productInStock by productInStock.UnitPrice into ProductsGroupingByPrice
                                                                                select ProductsGroupingByPrice
                                                  }
                            });

            foreach (var product in products)
            {
                ObjectDumper.Write(product, 3);
            }
        }

        [Category("Homework")]
        [Title("8 task")]
        [Description("Group products by cheap, more expensive, expensive")]
        public void Linq10_8()
        {
            var cheapProductsQuery = (from product in dataSource.Products
                                group product by product.UnitPrice into cheapProducts
                                from cheapProduct in cheapProducts
                                      where cheapProduct.UnitPrice < 5.0000M
                                select cheapProduct).ToList();

            foreach (var p in cheapProductsQuery)
                Console.WriteLine(p.ProductName + " " + p.UnitPrice);

            Console.WriteLine("=============================================");

            var moreExpensiveProductsQuery = (from product in dataSource.Products
                                      group product by product.UnitPrice into cheapProducts
                                      from cheapProduct in cheapProducts
                                      where cheapProduct.UnitPrice > 90.0000M && cheapProduct.UnitPrice < 100.0000M
                                      select cheapProduct).ToList();

            foreach (var p in moreExpensiveProductsQuery)
                Console.WriteLine(p.ProductName + " " + p.UnitPrice);

            Console.WriteLine("=============================================");

            var expensiveProductsQuery = (from product in dataSource.Products
                                      group product by product.UnitPrice into cheapProducts
                                      from cheapProduct in cheapProducts
                                      where cheapProduct.UnitPrice > 100.0000M
                                      select cheapProduct).ToList();

            foreach (var p in expensiveProductsQuery)
                Console.WriteLine(p.ProductName + " " + p.UnitPrice);
        }

        [Category("Homework")]
        [Title("9 task")]
        [Description("Average sum from every city & averange orders count on each customer from every city")]
        public void Linq11_9()
        {
            var query = from cust in dataSource.Customers
                        from ord in cust.Orders
                        group ord by cust.City into cityOrders
                        select new
                        {
                            CityName = cityOrders.Key,
                            OrdersSum = cityOrders.Sum(ord => ord.Total),
                            OrderCount = cityOrders.Count(),
                        };

            foreach (var q in query)
            {
                Console.WriteLine(string.Format("City - {0} Average sum {1}", q.CityName, (q.OrdersSum / q.OrderCount).ToString("0.00")));
            }

            Console.WriteLine("===================================================");

            var secondQuery = from cust in dataSource.Customers
                              group cust by cust.City into cityCustomers
                              select new
                              {
                                  CityCustomersCount = cityCustomers.Count(),
                                  CustCity = cityCustomers.Key,
                                  CityOrders =  from cityCust in cityCustomers
                                                from ord in cityCust.Orders
                                                group ord by cityCust.City into cityOrders
                                                select new
                                                {
                                                    OrderCount = cityOrders.Count()
                                                }
                              };

            foreach (var q in secondQuery)
            {
                foreach (var co in q.CityOrders)
                {
                    Console.WriteLine(string.Format("City - {0} Average count {1}", q.CustCity, (co.OrderCount / q.CityCustomersCount)));
                }
            }
        }

        [Category("Homework")]
        [Title("10 task")]
        [Description("Statistics by month in every year, statistics by year, union of both statistics")]
        public void Linq12_10()
        {
            //TODO
            var yearsCount = (dataSource.Customers
                                  .SelectMany(c => c.Orders)
                                  .GroupBy(c => c.OrderDate.Year)).Count();
                                  

            var query = dataSource.Customers
                                  .SelectMany(c => c.Orders)
                                  .GroupBy(c => c.OrderDate.Year, (key, order) => new { Year = key, Average = order.Count() / yearsCount });

            foreach(var q in query)
                Console.WriteLine(string.Format("Year {0} - avr {1}", q.Year, q.Average));

            var monthsQuery = dataSource.Customers
                                  .SelectMany(c => c.Orders)
                                  .GroupBy(c => c.OrderDate.Month, (key, order) => new { Month = key, Average = order.Count() / 12 });


            foreach (var q in monthsQuery)
                Console.WriteLine(string.Format("Month {0} - avr {1}", q.Month, q.Average));

            //var firstQuery = from cust in dataSource.Customers
            //                 select new
            //                 {
            //                     YearsGroup = from ord in cust.Orders
            //                                  group ord by ord.OrderDate.Year into yearsGroup
            //                                  select new
            //                                  {
            //                                      Year = yearsGroup.Key,
            //                                      MonthsGroups = from ord in yearsGroup
            //                                                     group ord by ord.OrderDate.Month into monthsGroup
            //                                                     select new
            //                                                     {
            //                                                         Month = monthsGroup.Key,
            //                                                         Orders = monthsGroup,
            //                                                     }
            //                                  }
            //                 };

            //var customersCount = firstQuery.Count();
            //foreach (var q in firstQuery)
            //{
       
            //}
        }

	}
}
