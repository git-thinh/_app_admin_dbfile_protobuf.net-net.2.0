//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic;
using System.Windows.Forms;

namespace Dynamic
{
    public class Program
    {
        public class Owner
        {
            public string Name { get; set; }
        }

        public interface IPet
        {
            string Name { get; }
            int Age { get; }
            string OwnerName { get; }
        }

        public class Pet : IPet
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Owner Owner { get; set; }

            string IPet.OwnerName { get { return Owner.Name; } }
        }

        public class PetInfo
        {
            public Pet Pet { get; set; }
            public string Summary { get; set; }
            public bool Old { get; set; }
        }

        public static List<Owner> owners;
        public static List<Pet> pets;
        public static List<IndexedPetData> pet_data;

        public static void PopulateLists()
        {
            owners = new List<Owner>
                    {
                        new Owner() { Name = "Suzie" },
                        new Owner() { Name = "John" },
                    };

            pets =
                    new List<Pet>{ new Pet { Name="Barley", Age=8, Owner = owners[0] },
                                   new Pet { Name="Boots", Age=4 , Owner = owners[1] },
                                   new Pet { Name="Whiskers", Age=1, Owner = owners[1] },
                                   new Pet { Name="Daisy", Age=4, Owner = owners[0] } };

            pet_data = pets.Select(pet => new IndexedPetData(pet)).ToList();
        }

        public class IndexedPetData : Pet
        {
            public IndexedPetData(Pet pet)
            {
                Name = pet.Name;
                Age = pet.Age;
                Owner = pet.Owner;
            }

            public object this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return Name;
                        case 1:
                            return Age;
                        case 2:
                            return Owner.Name;
                        default:
                            return null;
                    }
                }
            }
        }

        static void PrintOut(IQueryable queryable, Array array)
        {
            Console.WriteLine("Query: {0} \n Result: {1}", queryable, array);
        }

        static void FilteringExample()
        {
            var query1 = pets.AsQueryable().Where("Age < 8");
            var result1 = query1.ToArray();

            PrintOut(query1, result1);

            var query2 = pets.AsQueryable().Where("Owner.Name == @0", "Suzie");
            var result2 = query1.ToArray();

            PrintOut(query2, result2);
        }

        static void ProjectionExample()
        {
            var query1 = pets.AsQueryable().Select("new (Name as PetName, Owner.Name as OwnerName)");
            var result1 = query1.OfType<object>().ToArray();

            PrintOut(query1, result1);

            var query1a = pets.AsQueryable().Select<object>("new (Name as PetName, Owner.Name as OwnerName)");
            var result1a = query1a.ToArray();

            PrintOut(query1a, result1a);

            var query1b = pets.AsQueryable().Select<object>("new (Name, Owner.Name as Owner)");
            var result1b = query1b.ToArray();

            PrintOut(query1b, result1b);

            var query2 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, Name + \" of \" + Owner.Name as Summary, Age >= 8 as Old)");
            var result2 = query2.ToArray();

            PrintOut(query2, result2);

            var query2a = pets.AsQueryable().Select<PetInfo>("new Program.PetInfo (it as Pet, Name + \" of \" + Owner.Name as Summary, Age >= 8 as Old)");
            var result2a = query2a.ToArray();

            PrintOut(query2a, result2a);

            var query2b = pets.AsQueryable().Select<PetInfo>("new @0 (it as Pet, Name + \" of \" + Owner.Name as Summary, Age >= 8 as Old)", typeof(PetInfo));
            var result2b = query2b.ToArray();

            PrintOut(query2b, result2b);

            var query2c = pets.AsQueryable().Select("new Program.PetInfo (it as Pet, Name + \" of \" + Owner.Name as Summary, Age >= 8 as Old)");
            var result2c = query2c.OfType<PetInfo>().ToArray();

            PrintOut(query2c, result2c);
        }

        static void GroupingExample()
        {
            var query1 = pets.AsQueryable().Where("Age >= 2").GroupBy<int, string>("Age", "Name");
            var result1 = query1.ToArray();

            PrintOut(query1, result1);

            var query2 = pets.AsQueryable().GroupBy<int, Pet>("Age");
            var result2 = query2.ToArray();

            PrintOut(query2, result2);
        }

        static void NestedNewExample()
        {
            var query1 = pets.AsQueryable().Select<Pet>("new @out(Name, Age, new @0(@1 as Name) as Owner)", typeof(Owner), "New Owner");
            var result1 = query1.ToArray();

            PrintOut(query1, result1);
        }

        static void IndexerExamples()
        {
            var query1 = pet_data.AsQueryable().Where("Int32(it[1]) < 5");
            var result1 = query1.ToArray();

            PrintOut(query1, result1);

            var query1a = pet_data.AsQueryable().Where("Int32([1]) < 5");
            var result1a = query1a.ToArray();

            PrintOut(query1a, result1a);

            var query2 = pet_data.AsQueryable().Where("[2] == @0", "Suzie");
            var result2 = query2.ToArray();

            PrintOut(query2, result2);

            var query3 = pet_data.AsQueryable().Select<string>("String([0])");
            var result3 = query3.ToArray();

            PrintOut(query3, result3);

            var query4 = pet_data.AsQueryable().Select<object>("new ([0] as name, Int32([1]) as age)");
            var result4 = query4.ToArray();

            PrintOut(query4, result4);
        }

        static string GenerateSummary(Pet pet)
        {
            return pet.Name + " of " + pet.Owner.Name;
        }

        static void DelegatePassingExample()
        {
            Func<Pet, string> func1 = GenerateSummary;

            var query1 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, @0(it) as Summary, Age >= 8 as Old)", func1);
            var result1 = query1.ToArray();

            PrintOut(query1, result1);

            var query1a = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, @0(it) as Summary, Age >= 8 as Old)", new Func<Pet, string>(GenerateSummary));
            var result1a = query1a.ToArray();

            PrintOut(query1a, result1a);


            Func<Pet, string> func2 = pet => pet.Name + " of " + pet.Owner.Name;

            var query2 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, @0(it) as Summary, Age >= 8 as Old)", func2);
            var result2 = query2.ToArray();

            PrintOut(query2, result2);

            Expression<Func<Pet, string>> func3 = pet => pet.Name + " of " + pet.Owner.Name;

            var query3 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, @0(it) as Summary, Age >= 8 as Old)", func3);
            var result3 = query3.ToArray();

            PrintOut(query3, result3);

            Func<Pet, string> func4 = delegate (Pet pet)
            {
                return pet.Name + " of " + pet.Owner.Name;
            };

            var query4 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, @0(it) as Summary, Age >= 8 as Old)", func4);
            var result4 = query4.ToArray();

            PrintOut(query2, result2);
        }

        static void EvalExample()
        {
            var query1 = pets.AsQueryable().Select<PetInfo>("new @out (it as Pet, eval @0 as Summary, Age >= 8 as Old)", "Name + \" of \" + Owner.Name");
            var result1 = query1.ToArray();

            Func<string, DateTime> func = delegate (string item)
            {
                return DateTime.ParseExact(item, "yyMMddHHmmss", null);
            };

            demo[] dt = new demo[] { new demo() { time = DateTime.Now.ToString("yyMMddHHmmss") } };

            //var l0 = dt.AsQueryable().Select(typeof(Result), @"new @out (@0(it.time) as data)", func);

            var l0 = dt.AsQueryable().Select(@"new (@0(it.time) as data)", func);

            //var l1 = (IList)typeof(List<>).MakeGenericType(l0.ElementType).GetConstructor(Type.EmptyTypes).Invoke(null);
            //foreach (var elem in l0) l1.Add(elem);

            IList l2 = new List<object>();
            foreach (var elem in l0) l2.Add(elem);

            PrintOut(query1, result1);
        }

        static void InterfaceExample()
        {
            var query1 = pets.AsQueryable().Select<IPet>("new @out (Name, Age)");
            var result1 = query1.ToArray();

            PrintOut(query1, result1);

            var query2 = pets.AsQueryable().Select<IPet>("new @out (Name, Age, Age + 10 As AgeNextDecade)");
            var result2 = query2.ToArray();

            PrintOut(query2, result2);

            var query3 = pets.AsQueryable().Select<IPet>("new @out (Name, Age, Owner.Name as OwnerName)");
            var result3 = query3.ToArray();

            PrintOut(query3, result3);
        }

        static void Main(string[] args)
        {
            PopulateLists();

            FilteringExample();
            ProjectionExample();
            GroupingExample();
            NestedNewExample();
            IndexerExamples();
            DelegatePassingExample();
            EvalExample();
            InterfaceExample();

            Console.ReadLine();
        }
    }

    public class demo
    {
        public string time { set; get; }
    }

    public class Result
    {
        public DateTime data { set; get; }

        public Result()
        {
            //time = DateTime.ParseExact(dt, "yyMMddHHmmss", null);
        }
    }
}
