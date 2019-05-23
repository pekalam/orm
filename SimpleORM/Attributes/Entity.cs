﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class Entity : Attribute, IEntityFieldAttribute
    {
        public bool Validate(Type enityType, Type decoratedPropType, string decoratedPropName)
        {
            return true;
        }
    }
}