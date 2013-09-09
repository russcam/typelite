using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace TypeLite.TsModels
{
    /// <summary>
    /// Represents an enume
    /// </summary>
    public class TsEnum : TsType
    {
        private TsModule _module;

        /// <summary>
        /// Gets collection of properties of the class.
        /// </summary>
        public ICollection<TsEnumValue> Values { get; private set; }

        /// <summary>
        /// Gets or sets module, that contains this class.
        /// </summary>
        public TsModule Module
        {
            get
            {
                return _module;
            }
            set
            {
                if (_module != null)
                {
                    _module.AddEnum(this);
                }
                _module = value;
                if (_module != null)
                {
                    _module.AddEnum(this);
                }
            }
        }

      

        /// <summary>
        /// Gets or sets the name of the enum.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this enum will be ignored by TsGenerator.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsEnum class with the specific CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type represented by this instance of the TsEnum</param>
        public TsEnum(Type clrType) 
            : base(clrType)
        {
            if(!clrType.IsEnum)
                throw new NotImplementedException("type passed to constructor is not an enum");
            
            Name = clrType.Name;
            
            Values = new List<TsEnumValue>();
            var fieldsArray = clrType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var f in fieldsArray)
            {
                var name = f.Name;
                var value = (ulong) Convert.ChangeType(f.GetValue(null), typeof (ulong),null);
                Values.Add(new TsEnumValue() { Name = name, Value = value });
            }
            
            this.Module = new TsModule(this.ClrType.Namespace);

         
        }
    }
}
