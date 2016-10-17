using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateKey
{
    public class EntityBase
    {
        public long Id { get; set; }
    }

    public class HiLoSet<T> : IDbSet<T> where T : EntityBase
    {
        private readonly IDbSet<T> dbSet;
        private readonly IHiLoGenerator generator;

        public HiLoSet(IDbSet<T> dbSet, IHiLoGenerator generator)
        {
            this.dbSet = dbSet;
            this.generator = generator;
        }

        public T Add(T entity)
        {
            var add = dbSet.Add(entity);
            TrySetId(entity);
            return add;
        }

        private void TrySetId(EntityBase entity)
        {
            if (entity.Id == default(long))
            {
                entity.Id = generator.GetIdentifier();
                HandleChildsObjects(entity);
            }
        }

        private void HandleChildsObjects(EntityBase entity)
        {
            var propertyInfos = new List<PropertyInfo>();
            var typeToLook = entity.GetType();
            do
            {
                var newProps = entity.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => typeof(IEnumerable<EntityBase>).IsAssignableFrom(p.PropertyType)).ToList();
                propertyInfos.AddRange(newProps);
                typeToLook = typeToLook.BaseType;
            } while (typeToLook != typeof(object));


            var aggregated = propertyInfos
                .Select(p => p.GetValue(entity, null))
                .OfType<IEnumerable<EntityBase>>()
                .Where(col => col != null).SelectMany(col => col);
            foreach (var child in aggregated)
            {
                TrySetId(child);
            }
        }

        #region no op methods
        public IEnumerator<T> GetEnumerator()
        {
            return dbSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dbSet).GetEnumerator();
        }

        public Expression Expression
        {
            get { return dbSet.Expression; }
        }

        public Type ElementType
        {
            get { return dbSet.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return dbSet.Provider; }
        }

        public T Find(params object[] keyValues)
        {
            return dbSet.Find(keyValues);
        }

        public T Remove(T entity)
        {
            return dbSet.Remove(entity);
        }

        public T Attach(T entity)
        {
            return dbSet.Attach(entity);
        }

        public T Create()
        {
            return dbSet.Create();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return dbSet.Create<TDerivedEntity>();
        }

        public ObservableCollection<T> Local
        {
            get { return dbSet.Local; }
        }
        #endregion
    }

    public class HiLoGenerator<TDbContext> : IHiLoGenerator
        where TDbContext : DbContext, new()
    {
        private static readonly object ConcurrencyLock = new object();
        private readonly int _maxLo;
        private string _connectionString;
        private long _currentHi = -1;
        private int _currentLo;

        public HiLoGenerator(int maxLo)
        {
            this._maxLo = maxLo;
        }

        #region IHiLoGenerator Members

        public long GetIdentifier()
        {
            long result;
            lock (ConcurrencyLock)
            {
                if (_currentHi == -1)
                {
                    MoveNextHi();
                }
                if (_currentLo == _maxLo)
                {
                    _currentLo = 0;
                    MoveNextHi();
                }
                result = (_currentHi * _maxLo) + _currentLo;
                _currentLo++;
            }
            return result;
        }

        public void SetConnectionString(string newConnectionString)
        {
            _connectionString = newConnectionString;
        }

        #endregion

        private void MoveNextHi()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var tx = connection.BeginTransaction(IsolationLevel.Serializable))
                using (var selectCommand = connection.CreateCommand())
                {
                    selectCommand.Transaction = tx;
                    selectCommand.CommandText =
                        "SELECT next_hi FROM entityframework_unique_key;" +
                         "UPDATE entityframework_unique_key SET next_hi = next_hi + 1;";
                    _currentHi = (int)selectCommand.ExecuteScalar();

                    tx.Commit();
                }
                connection.Close();
            }

        }

        private IDbConnection CreateConnection()
        {
            using (var dbContext = new TDbContext())
            {
                var connectionType = dbContext.Database.Connection.GetType();
                _connectionString = _connectionString ?? dbContext.Database.Connection.ConnectionString;
                return (IDbConnection)Activator.CreateInstance(connectionType, _connectionString);
            }
        }
    }

    public interface IHiLoGenerator
    {
         long GetIdentifier();

        void SetConnectionString(string newConnectionString);
    }

    public class Product : EntityBase
    {
        public string Name { get; set; }
    }

    public class SampleContext : DbContext
    {
        private static readonly HiLoGenerator<SampleContext> HiloGenerator
            = new HiLoGenerator<SampleContext>(100);

        private IDbSet<Product> _products;
        public IDbSet<Product> Products
        {
            get
            {
                return _products;
            }
            set
            {
                _products = new HiLoSet<Product>(value, HiloGenerator);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<StoreGeneratedIdentityKeyConvention>();
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
        }
    }
}
