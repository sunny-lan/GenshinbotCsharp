using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public abstract class DbInst {
        public abstract object ObjVal { get; }

        public abstract Task Save();
    }
    public class DbInst<T>:DbInst where T : new()
    {

        public  T Value
        {
            get
            {
                return inst.Value;
            }
        }

        public string DbFile { get; }

        public override object ObjVal  => Value;

        private  Lazy<T> inst;

        public DbInst(string db)
        {
            inst = new Lazy<T>(() => Data.ReadJson1<T>(db));
            DbFile = db;
        }

        public override async Task Save()
        {
            await Data.WriteJsonAsync<T>(DbFile, Value);
        }
    }
}
