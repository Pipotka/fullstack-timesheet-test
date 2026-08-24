namespace Timesheet.Domain;

public readonly record struct TimeEntryId(string Value)
{
    public override string ToString() => Value;
}
