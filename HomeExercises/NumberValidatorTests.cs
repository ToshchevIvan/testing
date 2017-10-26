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
        [TestCase(1, -1, true, InvalidScaleMessage, 
            TestName = "when scale is negative")]
        [TestCase(6, 6, true, InvalidScaleMessage,
            TestName = "when scale is equal to precision")]
        [TestCase(1, 2, true, InvalidScaleMessage,
            TestName = "when scale is greater than precision")]
        public static void ThrowArgumentException(int precision, int scale, bool onlyPositive, 
            string expectedMessage)
        {
            var exc = Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
            Assert.That(exc.Message, Is.EqualTo(expectedMessage));
        }

        [TestCase(1, 0, true)]
        [TestCase(10, 9, true)]
	    public static void NotThrow(int precision, int scale, bool onlyPositive)
	    {
	        new NumberValidator(precision, scale, onlyPositive);
	    }

		[TestCase(5, 4, true, null, ExpectedResult = false)]
		[TestCase(5, 4, true, "", ExpectedResult = false)]
		[TestCase(1, 0, true, "+", ExpectedResult = false)]
		[TestCase(1, 0, true, "-", ExpectedResult = false)]
		[TestCase(1, 0, true, "5", ExpectedResult = true)]
		[TestCase(1, 0, true, "0.0", ExpectedResult = false)]
		[TestCase(1, 0, true, "10", ExpectedResult = false)]
		[TestCase(2, 1, true, "0.0", ExpectedResult = true)]
		[TestCase(2, 1, true, "0,0", ExpectedResult = true)]
		[TestCase(2, 1, true, "1.2", ExpectedResult = true)]
		[TestCase(2, 1, true, "+1.2", ExpectedResult = false)]
		[TestCase(2, 1, false, "-0.0", ExpectedResult = false)]
        [TestCase(3, 1, true, "+1.2", ExpectedResult = true)]
		[TestCase(3, 1, true, "-0.0", ExpectedResult = false)]
		[TestCase(3, 1, false, "-0.0", ExpectedResult = true)]
		[TestCase(3, 2, true, "5.42", ExpectedResult = true)]
		[TestCase(3, 2, true, "54.2", ExpectedResult = true)]
	    [TestCase(3, 2, true, "a.sd", ExpectedResult = false)]
		public static bool ValidateNumber(int precision, int scale, bool onlyPositive, string number)
		{
            return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(number);
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