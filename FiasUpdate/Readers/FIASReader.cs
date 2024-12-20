﻿using System.Collections.Generic;

namespace FiasUpdate.Readers
{
    internal class FIASReader : XMLDataReader
    {
        public FIASReader(string File, IEnumerable<string> Columns) : base(File, Columns) { }

        protected override bool IsValidRow()
        {
            return XML.HasAttributes && (XML.GetAttribute("ID") != null || XML.GetAttribute("OBJECTID") != null || XML.GetAttribute("NAME") != null);
        }

        protected override string GetAttribute(string name)
        {
            var Value = base.GetAttribute(name);
            // Опять костыль. В некоторых XML Boolean хранится как Integer.
            if ((name == "ISACTIVE" || name == "ISACTUAL") && !bool.TryParse(Value, out _))
            {
                return Value == "1" ? bool.TrueString : bool.FalseString;
            }
            else
            {
                return Value;
            }
        }
    }
}