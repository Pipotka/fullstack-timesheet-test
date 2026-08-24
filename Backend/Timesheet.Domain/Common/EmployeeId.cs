namespace Timesheet.Domain;

public readonly record struct EmployeeId(string Value)
{
    public override string ToString() => Value;
}
