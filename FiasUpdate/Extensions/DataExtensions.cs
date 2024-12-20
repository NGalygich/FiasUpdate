﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using System.Data;


namespace FiasUpdate
{
    internal static class DataExtensions
    {
        public static Column Clone(this Column column)
        {
            var copy = new Column
            {
                Name = column.Name,
                DataType = column.DataType,
                Default = column.Default,
                Identity = column.Identity,
                IdentityIncrement = column.IdentityIncrement,
                IdentitySeed = column.IdentitySeed,
                Nullable = column.Nullable
            };
            return copy;
        }

        public static Column CloneTo(this Column column, Table table)
        {
            var copy = new Column(table, column.Name)
            {
                DataType = column.DataType,
                Default = column.Default,
                Identity = column.Identity,
                IdentityIncrement = column.IdentityIncrement,
                IdentitySeed = column.IdentitySeed,
                Nullable = column.Nullable
            };
            table.Columns.Add(copy);
            return copy;
        }
    }
}
