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

        public string DbFilePath { get; }

        public override object ObjVal  => Value;

        private  Lazy<T> inst;

        public DbInst(string db)
        {
            inst = new Lazy<T>(() => Data.ReadJson1<T>(db));
            DbFilePath = db;
        }
        public DbInst(string db, T def)
        {
            inst = new Lazy<T>(() => Data.ReadJson<T>(db,def));
            DbFilePath = db;
        }

        public void ReloadFromDisk()
        {
            inst = new Lazy<T>(() => Data.ReadJson1<T>(DbFilePath));
        }

        public override async Task Save()
        {
            await Data.WriteJsonAsync<T>(DbFilePath, Value);
        }
    }
}
