namespace AdvancedCalculator.Core.Exceptions;

public class CalculationException : Exception
{
    public CalculationException(string message) : base(message) { }
    public CalculationException(string message, Exception innerException) : base(message, innerException) { }
}

public class DivisionByZeroCustomException : CalculationException
{
    public DivisionByZeroCustomException() : base("Cannot divide by zero") { }
}

public class InvalidExpressionCustomException : CalculationException
{
    public InvalidExpressionCustomException(string details = "Invalid expression") : base(details) { }
}

public class MathOverflowCustomException : CalculationException
{
    public MathOverflowCustomException() : base("Value is too large or overflowed") { }
}
