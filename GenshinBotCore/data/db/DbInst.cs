using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class DbInst<T> where T : new()
    {
        public  T Value
        {
            get
            {
                return inst.Value;
            }
        }

        public string DbFile { get; }

        private  Lazy<T> inst;

        public DbInst(string db)
        {
            inst = new Lazy<T>(() => Data.ReadJson1<T>(db));
            DbFile = db;
        }

        public async Task Save()
        {
            await Data.WriteJsonAsync<T>(DbFile, Value);
        }
    }
}
