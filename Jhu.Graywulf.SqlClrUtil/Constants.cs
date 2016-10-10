using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace Jhu.Graywulf.SqlClrUtil
{
    static class Constants
    {
        public static readonly StringComparer StringComparer = StringComparer.InvariantCultureIgnoreCase;

        public static readonly HashSet<string> SystemSchemas = new HashSet<string>(StringComparer)
        {
            "dbo", "sys", "guest"
        };

        public static readonly Dictionary<Type, string> SqlTypes = new Dictionary<Type, string>()
        {
            { typeof(Boolean), "bit" },
            { typeof(Byte), "tinyint" },
            { typeof(SByte), "tinyint" },
            { typeof(Int16), "smallint" },
            { typeof(UInt16), "smallint" },
            { typeof(Int32), "int" },
            { typeof(UInt32), "int" },
            { typeof(Int64), "bigint" },
            { typeof(UInt64), "bitint" },
            { typeof(DateTime), "datetime" },
            { typeof(Decimal), "decimal" },
            { typeof(Single), "real" },
            { typeof(Double), "float" },
            { typeof(Guid), "uniqueidentifier" },
            { typeof(Byte[]), "varbinary(max)" },
            { typeof(String), "nvarchar(max)" },

            { typeof(SqlBoolean), "bit" },
            { typeof(SqlByte), "tinyint" },
            { typeof(SqlInt16), "smallint" },
            { typeof(SqlInt32), "int" },
            { typeof(SqlInt64), "bigint" },
            { typeof(SqlDateTime), "datetime" },
            { typeof(SqlDecimal), "decimal" },
            { typeof(SqlMoney), "decimal" },
            { typeof(SqlSingle), "real" },
            { typeof(SqlDouble), "float" },
            { typeof(SqlGuid), "uniqueidentifier" },
            { typeof(SqlBinary), "varbinary(max)" },
            { typeof(SqlBytes), "varbinary(max)" },
            { typeof(SqlString), "nvarchar(max)" },
            { typeof(SqlChars), "nvarchar(max)" },
        };

        public static readonly Dictionary<AssemblySecurityLevel, string> AssemblySecurityLevels = new Dictionary<AssemblySecurityLevel, string>()
        {
            { AssemblySecurityLevel.Safe, "SAFE" },
            { AssemblySecurityLevel.External, "EXTERNAL_ACCESS" },
            { AssemblySecurityLevel.Unrestricted, "UNSAFE" },
        };

        //.Net libraries supported by SQL were taken from https://msdn.microsoft.com/en-us/library/ms403279.aspx
        public static readonly HashSet<string> SQLSupportedLibraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CustomMarshalers",
            "Microsoft.VisualBasic",
            "Microsoft.VisualC",
            "System.Configuration",
            "System.Data",
            "System.Data.OracleClient",
            "System.Data.SqlXml",
            "System.Deployment",
            "System.Security",
            "System.Transactions",
            "System.Web.Services",
            "System.Core",
            "System.Xml",
            "System.Xml.Linq",
            "System",
            "System.Numerics",
            "mscorlib"
        };


    }
}
