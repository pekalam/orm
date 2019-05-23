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
        /// <param name="decoratedPropType"></param>
        /// <param name="decoratedPropName"></param>
        /// <returns></returns>
        bool Validate(Type enityType, Type decoratedPropType, string decoratedPropName);
    }
}