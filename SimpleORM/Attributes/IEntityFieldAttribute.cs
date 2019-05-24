using System;
using System.Reflection;

namespace SimpleORM.Attributes
{
    /// <summary>
    /// Interfejs implementowany przez atrybuty SimpleORM
    /// </summary>
    public interface IEntityFieldAttribute
    {
        //TODO: void
        /// <summary>
        /// Sprawdza czy atrybut został poprawnie przypisany do encji. W  przeciwnym wypdaku wyrzuca wyjątek.
        /// </summary>
        /// <param name="enityType"></param>
        /// <returns></returns>
        void Validate(Type enityType);
    }
}