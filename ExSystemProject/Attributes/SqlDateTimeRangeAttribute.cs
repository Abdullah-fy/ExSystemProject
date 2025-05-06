using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.Attributes
{
    public class SqlDateTimeRangeAttribute : ValidationAttribute
{
    private readonly DateTime _min = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime _max = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dt)
            {
                if (dt < _min || dt > _max)
                {
                    return new ValidationResult(
                        $"{validationContext.DisplayName} must be between {_min:MM/dd/yyyy HH:mm:ss} and {_max:MM/dd/yyyy HH:mm:ss}.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
