using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidator_should
    {
        private const string InvalidScaleMessage = "scale must be a non-negative number less or equal than precision";
        private const string InvalidPrecisionMessage = "precision must be a positive number";

        [TestCase(-1, 2, true, InvalidPrecisionMessage,
            TestName = "when precision is negative")]

        //CR(epeshk): При scale == 0 исключение не кидается, тесткейс на самом деле проверяет случай "when scale is equal to precision"
        // Это не так: тесткейс проверяет условие precision <= 0. 
        // Если убрать из него проверку сообщения, тогда действительно непонятно, что он проверяет (может сработать и на то и на другое)
        [TestCase(0, 0, true, InvalidPrecisionMessage,
            TestName = "when scale is zero")]
        [TestCase(1, -1, true, InvalidScaleMessage,
            TestName = "when scale is negative")]
        [TestCase(6, 6, true, InvalidScaleMessage,
            TestName = "when scale is equal to precision")]
        [TestCase(1, 2, true, InvalidScaleMessage,
            TestName = "when scale is greater than precision")]
        public static void ThrowArgumentException(int precision, int scale, bool onlyPositive,
            string expectedMessage)
        {
            new Action(() => new NumberValidator(precision, scale, onlyPositive))
                .ShouldThrow<ArgumentException>()
                .Which.Message.Should().Be(expectedMessage);
            // Проверка сообщения в исключениях - ужасный механизм: изменение сообщения сразу же влечёт недействительность теста
            // Однако нет иного способа удостовериться, что было выброшено нужное исключение
            // Предположим, что одна из проверок написана неверно и второй тест падает из-за равенства scale и precision, 
            // а не из-за того, что scale равен 0. Без проверки сообщения нет шансов узнать, что именно пошло не так
            // Итого: тест проходит при неправильно работающем коде. Такой тест не только некорректен, но и вреден,
            // поскольку создаёт ложное впечатление о работоспособности кода.
        }

        [TestCase(1, 0, true)]
        [TestCase(10, 9, true)]
        public static void NotThrow(int precision, int scale, bool onlyPositive)
        {
            //CR(epeshk): стоит использовать Assert.DoesNotThrow(() => ...) или new Action(() => ...).ShouldNotThrow();
            new Action(() => new NumberValidator(precision, scale, onlyPositive))
                .ShouldNotThrow();
        }


        [TestCase("0", 1, 0)]
        [TestCase("0.0", 2, 1, TestName = "when number has fraction part")]
        [TestCase("0,0", 2, 1, TestName = "when comma is used as separator")]
        [TestCase("1.2", 2, 1, TestName = "when number is not zero")]
        [TestCase("+1.2", 3, 1, TestName = "when number has sign")]
        [TestCase("-120.12", 7, 2, false, TestName = "when number is negative")]
        [TestCase("15.2", 10, 1, TestName = "when number is shorter than precision")]
        [TestCase("54.2", 3, 2, TestName = "when fraction is shorter than scale")]
        [TestCase("00.00", 4, 2, TestName = "when number has leading zeroes")]
        //CR(epeshk): стоит добавить тесткейс c ведущими нулями
        public static void ValidateNumber(string number, int precision, int scale, bool onlyPositive = true)
        {
            //CR(epeshk): давай заиспользуем здесь FluentAssertions
            new NumberValidator(precision, scale, onlyPositive)
                .IsValidNumber(number)
                .Should()
                .BeTrue();
        }

        [TestCase(null, 5, 4)]
        [TestCase("", 5, 4)]
        [TestCase("+", 1, 0)]
        [TestCase("-", 1, 0)]
        [TestCase(".1", 1, 0, TestName = "when number has no int part")]
        [TestCase("1.", 1, 0, TestName = "when fraction part is omitted")]
        [TestCase("10", 1, 0, TestName = "when int part exceeds precision")]
        [TestCase("00.00", 3, 2, TestName = "when number length exceeds precision")]
        [TestCase("0.0", 2, 0, TestName = "when fraction part exceeds scale")]
        [TestCase("+1.2", 2, 1, TestName = "when number with sign is longer than precision")]
        [TestCase("-0.0", 2, 1, false, TestName = "when negative number with sign is longer than precision")]
        [TestCase("-0.0", 3, 1, TestName = "when only-positive validator is supplied with negative number")]
        [TestCase("a.sd", 3, 2, TestName = "when number contains non-digits")]
        //CR(epeshk): стоит добавить тесткейсы, в которых нет целой или дробной части "1.", ".1"
        public static void NotValidateNumber(string number, int precision, int scale, bool onlyPositive = true)
        {
            new NumberValidator(precision, scale, onlyPositive)
                .IsValidNumber(number)
                .Should()
                .BeFalse();
        }

        //CR(epeshk): надо добавить тест, в котором NumberValidator создаётся с параметрами по умолчанию и проверяется, что проверка производится в соответствии с ожидаемым по умолчанию значениями
        [TestCase("-5", 2, ExpectedResult = true, TestName = "and number is negative")]
        [TestCase("0.0", 2, ExpectedResult = false, TestName = "and FAIL when number has fraction part")]
        public static bool ValidateNumber_WhenItHasDefaultParameters(string number, int precision)
        {
            return new NumberValidator(precision)
                .IsValidNumber(number);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("scale must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}