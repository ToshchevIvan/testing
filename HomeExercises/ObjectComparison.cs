using System;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace HomeExercises
{
    public class ObjectComparison
    {
        [Test]
        [Description("Проверка текущего царя")]
        [Category("ToRefactor")]
        public void CheckCurrentTsar()
        {
            Person actualTsar = TsarRegistry.GetCurrentTsar();

            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
          
            actualTsar.ShouldBeEquivalentTo(expectedTsar,
                options => options
                    .FromType(typeof(Person))
                    .ExcludeField(nameof(Person.Id)));
        }

        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public void CheckCurrentTsar_WithCustomEquality()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            Assert.True(AreEqual(actualTsar, expectedTsar));
        }

        private bool AreEqual(Person actual, Person expected)
        {
            if (actual == expected) return true;
            if (actual == null || expected == null) return false;
            return
                actual.Name == expected.Name
                && actual.Age == expected.Age
                && actual.Height == expected.Height
                && actual.Weight == expected.Weight
                && AreEqual(actual.Parent, expected.Parent);
        }
    }

    internal static class FluentAssertionsExtensions
    {
        public static TypeSelector<TSelf> FromType<TSelf>(this EquivalencyAssertionOptions<TSelf> options, Type type)
        {
            return new TypeSelector<TSelf>(type, options);
        }

        internal class TypeSelector<TSelf>
        {
            private readonly Type type;
            private readonly EquivalencyAssertionOptions<TSelf> options;

            internal TypeSelector(Type type, EquivalencyAssertionOptions<TSelf> options)
            {
                this.type = type;
                this.options = options;
            }

            public EquivalencyAssertionOptions<TSelf> ExcludeField(string fieldName)
            {
                return options.Excluding(opt => opt.SelectedMemberInfo.DeclaringType == type &&
                                                opt.SelectedMemberInfo.Name == fieldName);
            }
        }
    }

    public class TsarRegistry
    {
        public static Person GetCurrentTsar()
        {
            return new Person(
                "Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
        }
    }

    public class Person
    {
        public static int IdCounter = 0;
        public int Age, Height, Weight;
        public string Name;
        public Person Parent;
        public int Id;

        public Person(string name, int age, int height, int weight, Person parent)
        {
            Id = IdCounter++;
            Name = name;
            Age = age;
            Height = height;
            Weight = weight;
            Parent = parent;
        }
    }
}