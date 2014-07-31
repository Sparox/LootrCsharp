using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootrConsole
{
    public class Drop
    {
        public string Branch { get; set; }
        public int Depth { get; set; }
        public float Luck { get; set; }
        public int Stack { get; set; }

        public Drop(string branch, int depth, float luck, int stack)
        {
            Branch = branch;
            Depth = depth;
            Luck = luck;
            Stack = stack;
        }
    }

    public class Lootr
    {

        private string name;
        private List<Object> items = new List<Object>();
        private List<Lootr> branchs = new List<Lootr>();
        private List<Object> nameModifiers = new List<Object>();

        private Random r = new Random();
        public Lootr(string name)
        {
            var branchName = name;

            branchName = this.clean(branchName);
            if (name.IndexOf('/') > -1)
            {
                throw new Exception("Specified name should not contain a / separator");
            }
            this.name = branchName;

        }

        private string clean(string name)
        {
            return name.Trim();
        }

        public Lootr add(Object item, string path)
        {
            if (path.Count() == 0)
            {
                this.items.Add(item);
            }
            else
            {
                var branch = this.branch(path);
                branch.items.Add(item);
            }

            return this;
        }

        public Lootr branch(string name)
        {
            return this.getBranch(name, true);
        }

        public Lootr getBranch(string name, bool create = false)
        {
            var path = this.clean(name).Split('/');

            if (!this.branchs.Any(b => b.name == path[0])
               && path[0] != this.name
               && create)
            {
                this.branchs.Add(new Lootr(path[0]));
            }

            if (path.Length == 1)
            {
                return path[0] == this.name ? this : this.branchs.FirstOrDefault(b => b.name == path[0]);
            }
            else if (path.Length > 1)
            {
                var head = path.Take(1).ToArray()[0];
                path = path.Skip(1).ToArray();
                var newPath = string.Join("/", path);

                if (this.branchs.Any(b => b.name == head))
                {
                    return this.branchs.First(b => b.name == head).getBranch(newPath, create);
                }

                if (create)
                {
                    this.branchs.Add(new Lootr(head));
                    return this.branchs.First(b => b.name == head).getBranch(newPath, create);
                }
            }

            return null;
        }

        public List<Object> getAllItems()
        {
            var allItems = new List<Object>(this.items);

            foreach (var branch in this.branchs)
            {
                allItems.AddRange(branch.getAllItems());
            }

            return allItems;
        }

        public Object randomPick(int allowedNesting, float threshold = 0.9f)
        {
            var pickedItems = new List<Object>();

            if (r.NextDouble() < threshold && this.items.Count() > 0)
            {
                pickedItems.Add(this.items[r.Next(this.items.Count)]);
            }

            if (allowedNesting > 0)
            {
                foreach (var branch in this.branchs)
                {
                    var nestedChance = r.NextDouble();
                    if (nestedChance <= threshold)
                    {
                        var others = branch.randomPick(
                                                allowedNesting - 1, 
                                                (float)(threshold - r.NextDouble() / allowedNesting));
                        if (others != null)
                        {
                            pickedItems.Add(others);
                        }
                    }
                }
            }

            return this.items.Count > 0 ? pickedItems[r.Next(pickedItems.Count)] : null;
        }

        public Object roll(string catalogPath, int nesting, float threshold = 0.9f)
        {
            var branch = this.getBranch(catalogPath);

            return branch.randomPick(nesting, threshold);
        }

        public List<Object> loot(List<Drop> drops)
        {
            var reward = new List<Object>();

            foreach (var drop in drops)
            {
                var item = this.roll(drop.Branch, drop.Depth, drop.Luck);
                if (item != null)
                {
                    continue;
                }

                reward.Add(item);
            }

            return reward;
        }
    }
}
