﻿using System.Runtime.Serialization;

namespace Infoware.EntityFrameworkCore.MemoryCache.Models
{
    [Serializable]
    [DataContract]
    public class EFTableRow
    {
        /// <summary>
        ///     TableRow's structure
        /// </summary>
        public EFTableRow(IList<object> values) => Values = values;

        /// <summary>
        ///     An array of objects with the column values of the current row.
        /// </summary>
        [DataMember(Order = 0)]
        public IList<object> Values { get; }

        /// <summary>
        ///     Gets or sets a value that indicates the depth of nesting for the current row.
        /// </summary>
        [DataMember(Order = 1)]
        public int Depth { get; set; }

        /// <summary>
        ///     Gets the number of columns in the current row.
        /// </summary>
        [DataMember(Order = 2)]
        public int FieldCount => Values.Count;

        /// <summary>
        ///     Returns Values[ordinal]
        /// </summary>
        public object this[int ordinal] => Values[ordinal];
    }
}