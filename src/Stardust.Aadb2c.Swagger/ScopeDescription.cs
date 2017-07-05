using System;
using System.Runtime.CompilerServices;

namespace Stardust.Aadb2c.Swagger
{
    public class ScopeDescription
    {
        private string _description;
        private string _scopeName;

        public string ScopeName
        {
            get { return _scopeName; }
            set
            {
                Validate(value,true);
                _scopeName = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                Validate(value,false);
                _description = value;
            }
        }

        private static void Validate(string value,bool notNull,[CallerMemberName]string parameterName=null)
        {
            if (value == null)
            {if(!notNull) return;
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Contains(";") || value.Contains("|")) throw new ArgumentException($"; and | are not allowed in {parameterName}",nameof(value));
        }
    }
}