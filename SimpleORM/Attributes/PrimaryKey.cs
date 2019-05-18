using System;

namespace SimpleORM.Attributes
{
    public interface IEntityFieldAttribute
    {

    }

    public class PrimaryKey : Attribute, IEntityFieldAttribute
    {
        
    }
}