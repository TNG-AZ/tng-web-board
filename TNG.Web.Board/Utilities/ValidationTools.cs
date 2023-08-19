using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Utilities
{
    public class IsToday : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is equal to today in MST timezone
            return d.Date == DateTime.Now.ToAZTime().Date;

        }
    }

    public class IsOfAge : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is ~18 years or more from today in MST timezone
            var eligibileAge = DateTime.Now.ToAZTime().AddYears(-18);

            return d.Date <= eligibileAge.Date;
        }
    }
}
