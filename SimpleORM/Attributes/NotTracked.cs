using System;

namespace SimpleORM.Attributes
{
    /// <summary>
    /// Powoduje, ze pole encji nie jest mapowane przez ORM
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotTracked : Attribute
    {
    }
}
