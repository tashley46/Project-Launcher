using System;

namespace ProjectLaunch.Core.Domain;

public abstract class EntityBase
{
    public int Id { get; set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedDateTime { get; private set; }

    public DateTimeOffset ModifiedDateTime { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    public void SetCreatedDateTime(DateTimeOffset createdDateTime)
    {
        CreatedDateTime = createdDateTime;
    }

    public void SetModifiedDateTime(DateTimeOffset modifiedDateTime)
    {
        ModifiedDateTime = modifiedDateTime;
    }

}
