using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using NUnit.Framework;

namespace SurrogateKey.Tests
{
    public class HiLoTests
    {
        [Test]
        public void ShouldWork()
        {
            //http://joseoncode.com/2011/03/23/hilo-for-entityframework/
            var expected = Enumerable.Range(0, 200).Select(i => (long)i).ToList();

            using (var context = new SampleContext())
            {
                //create 200 products:
                var products = Enumerable.Range(0, 200)
                    .Select(i => new Product { Name = $"Test product {i}"}).ToList();

                //add to the dbSet: DO NOT FLUSH THE CHANGES YET.
                products.ForEach(p => context.Products.Add(p));

                //Assert
                var hiloIds = products.Select(p => p.Id);
                Assert.That(hiloIds,Is.EqualTo(expected));

                context.SaveChanges();
            }
        }
    }
}
