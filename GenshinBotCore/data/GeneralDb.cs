using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class GeneralDb
    {
        public class Folder
        {
            public class Node
            {
                public Dictionary<Size, Point2d> Points { get; set; } = new Dictionary<Size, Point2d>();
            }

            public Dictionary<string, Node> Nodes { get; set; } = new Dictionary<string, Node>();
            public Dictionary<string, Folder> Folders { get; set; } = new Dictionary<string, Folder>();
            public Folder FindFolder(string[] path, bool createMissing = false)
            {
                Folder f = this;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (!f.Folders.ContainsKey(path[i]))
                    {
                        if (createMissing)
                            f.Folders[path[i]] = new Folder();
                        else
                            return null;
                    }
                    f = f.Folders[path[i]];
                }
                return f;
            }
            public Node Find(string[] path, bool createMissing=false)
            {
                var v = FindFolder(path[..^1],createMissing)?.Nodes;
                if (v==null || !v.ContainsKey(path[^1]))
                {
                    if (createMissing)
                        v[path[^1]] = new Node();
                    else
                        return null;
                }
                return v[path[^1]];
            }

            public void Add(string[] path, Folder g, bool createMissing = true)
            {
               FindFolder(path[..^1],createMissing).Folders.Add(path[^1], g);
            }
            public void Add(string[] path, Node g, bool createMissing = true)
            {
                FindFolder(path[..^1], createMissing).Nodes.Add(path[^1], g);
            }
            public void Add(string[] path, Size s,Point2d d, bool createMissing = true)
            {
                Find(path, createMissing).Points[s] = d;
            }


            public Node Find(string path) => Find(path.Split('.'));
            public void Add(string path, Folder f, bool createMissing = true) => Add(path.Split('.'), f, createMissing);
            public void Add(string path, Node g, bool createMissing = true) => Add(path.Split('.'), g, createMissing);
            public void Add(string path, Size s, Point2d d, bool createMissing = true) => Add(path.Split('.'), s, d, createMissing);
        }
        public Folder Root { get; set; } = new Folder();
    }

}
