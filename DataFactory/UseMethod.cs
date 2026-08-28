using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.Extensions.Configuration;

namespace DataFactory
{
    public class UseMethod
    {

        private MultiDbRepositoryFactory factory;

        public UseMethod()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            factory = new MultiDbRepositoryFactory(configuration);
        }


        public void FactoryUseMethod()
        {
            List<TestDbUsers> list = new List<TestDbUsers>();
            using (var windRepo = factory.GetRepository<TestDbUsers>("KingBase_TestDBConnection"))
            {
                list = windRepo.FindAll().ToList();

                var list2 = windRepo.FindQueryable(a => a.Id > 3);

                int y = 12;

                var users = windRepo.QueryBySql("SELECT * FROM Users");

            }

            list = list.OrderBy(a => a.Name).ToList();

        }

    }
}
