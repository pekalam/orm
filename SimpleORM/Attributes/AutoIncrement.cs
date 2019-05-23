﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    //TODO abstract class?
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIncrement : Attribute, IEntityFieldAttribute
    {
        public bool Validate(Type enityType, Type decoratedPropType, string decoratedPropName)
        {
            return true;
        }
    }
}
