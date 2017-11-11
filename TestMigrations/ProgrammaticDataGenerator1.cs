using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Abstractions;
using TestMigrations.Data;

namespace TestMigrations
{
    public class ProgrammaticDataGenerator1 : IProgrammaticDataGenerator
    {
        private readonly TestMigrationsDataContext _dataContext;

        public ProgrammaticDataGenerator1(TestMigrationsDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public string Name => "ProgGen1";
        public void GenerationRoutine()
        {
            _dataContext.TestTableOne.Add(new TestTableOne()
            {
                Value = 1
            });
            _dataContext.TestTableOne.Add(new TestTableOne()
            {
                Value = 2
            });
            _dataContext.TestTableOne.Add(new TestTableOne()
            {
                Value = 3
            });

            _dataContext.TestTableTwo.Add(new TestTableTwo()
            {
                Value = 1
            });
            _dataContext.TestTableTwo.Add(new TestTableTwo()
            {
                Value = 2
            });
            _dataContext.TestTableTwo.Add(new TestTableTwo()
            {
                Value = 3
            });

            _dataContext.TestTableThree.Add(new TestTableThree()
            {
                Value = 1
            });
            _dataContext.TestTableThree.Add(new TestTableThree()
            {
                Value = 2
            });
            _dataContext.TestTableThree.Add(new TestTableThree()
            {
                Value = 3
            });

            _dataContext.SaveChanges();
        }
    }
}
