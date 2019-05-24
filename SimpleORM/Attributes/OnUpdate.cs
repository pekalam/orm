using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class OnUpdate : Attribute, IEntityFieldAttribute
    {
        private string _onUpdate;

        public OnUpdate(string onUpdate)
        {
            _onUpdate = onUpdate;
        }

        public void Validate(Type enityType)
        {
            return;
        }

        public string OnUpdateStr => _onUpdate;
    }
}
