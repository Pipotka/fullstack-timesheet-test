namespace Timesheet.Domain;

public readonly record struct ProjectId(string Value)
{
    public override string ToString() => Value;
}
