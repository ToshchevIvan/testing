using FluentAssertions;
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
            // Важно, чтобы actual и expected были одного типа
            // Что, если GetCurrentTsar() поменяется и будет отдавать объект другого типа?
            // Например, Person2? В контексте этого теста и (мнимой) задачи, сравнение объектов разного типа не имеет смысла
            // Но ShouldBeEquivalentTo это не учитывает
            // Можно отказаться от явной проверки и сделать так:
            Person actualTsar = TsarRegistry.GetCurrentTsar();
            // Хотя это (в отличие от явной проверки) не гарантирует, что actualTsar - не объект некоего подкласса Person...

            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            // overspecification - это про количество Assert'ов или про количество проверяемых условий?
            // Например, если условие - "два объекта Person равны" - это overspecification, или нет?
            //CR(epeshk): получается, что требований два - 1) тип Person, 2) равенство полей.
            // т.к. в C# - статическая типизация - проверку типа обычно можно пропустить. Но если надо проверить тип - лучше сделать это в отдельном тесте
            actualTsar.ShouldBeEquivalentTo(expectedTsar,
                options => options.Excluding(ctx => ctx.SelectedMemberInfo.DeclaringType == expectedTsar.GetType() &&
                                                    ctx.SelectedMemberInfo.Name == "Id"));
            //CR(epeshk): GetType() -> typeof(Person), ctx -> opt (ctx обычно используется для CanellationToken или context)
            //CR(epeshk): давай теперь скроем явное использование SelectedMemberInfo внутри extension-метода. Примерно так:
            // actualTsar.ShouldBeEquivalentTo(expectedTsar,
            //    opt => opt
            //        .FromType(typeof(Person))
            //        .ExcludeField(nameof(Person.Id)));
            
            // Почему изначально было сделано не так:
            // Поскольку expectedTsar объявляется прямо в тесте, нет никакой проблемы, чтобы изменить условие в assert
            // И оставить тест простым для понимания
            // Если же тест сравнивает множество произвольных объектов, тогда, конечно, необходима универсальная проверка
        }

        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public void CheckCurrentTsar_WithCustomEquality()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

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