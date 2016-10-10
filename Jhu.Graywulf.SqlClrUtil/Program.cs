using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Jhu.Graywulf.SqlClrUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = Assembly.LoadFrom(args[0]);
            var sec = AssemblySecurityLevel.Safe;

            if (args.Length > 1)
            {
                Enum.TryParse<AssemblySecurityLevel>(args[1], out sec);
            }

            var r = new SqlClrReflector(a, sec);

            var dir = Path.GetDirectoryName(a.Location);
            var name = Path.GetFileNameWithoutExtension(a.Location);
            var path = Path.Combine(dir, name);
            var crpath = path + ".Create.sql";
            var drpath = path + ".Drop.sql";

            using (var outfile = new StreamWriter(crpath))
            {
                r.ScriptCreate(outfile);
            }

            using (var outfile = new StreamWriter(drpath))
            {
                r.ScriptDrop(outfile);
            }
        }
    }
}
