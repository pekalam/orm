using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    public class OnDelete : Attribute, IEntityFieldAttribute
    {
        private string _onDelete;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onDelete">Jedna z wartości: CASCADE / SET NULL / NO ACTION / SET DEFAULT</param>
        public OnDelete(string onDelete)
        {
            _onDelete = onDelete;
        }

        public string OnDeleteStr => _onDelete;
        public void Validate(Type enityType)
        {
            return;
        }
    }
}
