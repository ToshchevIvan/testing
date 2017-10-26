using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class ObjectComparison
    {
        [Test]
        [Description("Проверка текущего царя")]
        [Category("ToRefactor")]
        //CR(epeshk): Run this!
        public void CheckCurrentTsar()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();

            var expectedTsar =
                new Person("Ivan IV The Terrible", 54, 170, 70,
                    new Person("Vasili III of Russia", 28, 170, 60,
                        new Person("Ivan III Vasilievich", 65, 170, 70, null)));
            // ShouldBeEquivalentTo не проверяет равенство типов
            //CR(epeshk): overspecification - надо оставить по одному assert'у в тесте
            actualTsar.Should().BeOfType(expectedTsar.GetType());
            actualTsar.ShouldBeEquivalentTo(expectedTsar,
                options => options.Excluding(o => o.Id).Excluding(o => o.Parent.Id));
            // Теперь тест автоматически сравнивает все публичные поля и свойства, кроме исключённых из сравнения явно (Id)
            // Что логично: два объекта Person равны, когда равно всё то, что видно извне
        }

        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public void CheckCurrentTsar_WithCustomEquality()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar =
                new Person("Ivan IV The Terrible", 54, 170, 70,
                    new Person("Vasili III of Russia", 28, 170, 60,
                        new Person("Ivan III Vasilievich", 65, 170, 70, null)));

            // - Какие недостатки у такого подхода? 
            // - Отсутствие информации о том, что пошло не так. Тест сообщит только то, что false - это не true.
            //   Нельзя понять, где закралась ошибка, т.к. как только что-то не совпало, валится всё и сразу
            //   При изменении набора публичных полей в классе Person нужно будет дополнить тест
            //   Можно забыть это сделать
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

    public class TsarRegistry
    {
        public static Person GetCurrentTsar()
        {
            return 
                new Person(
                "Ivan IV The Terrible", 54, 170, 70,
                    new Person("Vasili III of Russia", 28, 170, 60,
                        new Person("Ivan III Vasilievich", 65, 170, 70, null)));
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