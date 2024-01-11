﻿using System.Runtime.Serialization;

namespace Infoware.EntityFrameworkCore.MemoryCache.Models
{
    [Serializable]
    [DataContract]
    public class EFTableColumnInfo
    {
        /// <summary>
        ///     The column's ordinal.
        /// </summary>
        [DataMember]
        public int Ordinal { get; set; }

        /// <summary>
        ///     The column's name.
        /// </summary>
        [DataMember]
        public string Name { get; set; } = default!;

        /// <summary>
        ///     The column's DbType Name.
        /// </summary>
        [DataMember]
        public string DbTypeName { get; set; } = default!;

        /// <summary>
        ///     The column's Type.
        /// </summary>
        [DataMember]
        public string TypeName { get; set; } = default!;

        /// <summary>
        ///     ToString
        /// </summary>
        public override string ToString() =>
            $"Ordinal: {Ordinal}, Name: {Name}, DbTypeName: {DbTypeName}, TypeName= {TypeName}.";
    }
}