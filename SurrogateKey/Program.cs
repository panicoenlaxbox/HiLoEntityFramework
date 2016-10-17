using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateKey
{
    //class MyContext:DbContext
    //{
    //    public DbSet<Customer> Customers { get; set; }
    //    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //    {
    //        modelBuilder.Configurations.Add(new CustomerMap());
    //        base.OnModelCreating(modelBuilder);
    //    }
    //}

    //class CustomerMap:EntityTypeConfiguration<Customer>
    //{
    //    public CustomerMap()
    //    {
    //        ToTable("Customers");
    //        //Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
    //    }
    //}

    //public class Entity
    //{
    //    public Entity()
    //    {
    //        Id = Guid.NewGuid();
    //    }

    //    public Guid Id { get; set; }
    //}

    //public class Customer:Entity
    //{
    //    public Customer()
    //    {
    //        Orders=new Collection<Order>();
    //    }
    //    public string Name { get; set; }
    //    public virtual ICollection<Order> Orders { get; set; }
    //}

    //public class Order : Entity
    //{
    //    public int Units { get; set; }
    //    public Guid CustomerId { get; set; }
    //    public virtual Customer Customer { get; set; }
    //}

    class Program
    {
        static void Main(string[] args)
        {
            //using (var context = new MyContext())
            //{
            //    //http://www.modestosanjuan.com/claves-primarias-inmutabilidad-y-generacion/
            //    //context.Customers.ToList().ForEach(p=>context.Customers.Remove(p));
            //    //context.SaveChanges();
            //    //var customer = new Customer
            //    //{
            //    //    Name = "Customer 1"
            //    //};
            //    //Debug.Print(customer.Id.ToString());
            //    //customer.Orders.Add(new Order()
            //    //{
            //    //    Units=5
            //    //});
            //    //context.Customers.Add(customer);
            //    //context.SaveChanges();

            //}
        }
    }
}
