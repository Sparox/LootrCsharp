using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LootrConsole
{
    public class Drop
    {
        public string Branch { get; set; }
        public int Depth { get; set; }
        public float Luck { get; set; }
        public int Stack { get; set; }

        public Drop(string branch, float luck, int stack, int depth = 0)
        {
            Branch = branch;
            Depth = depth;
            Luck = luck;
            Stack = stack;
        }
    }

    public class Lootr
    {
        public const int INFINITY = -1;
        public string name;
        public List<Object> items = new List<Object>();
        public List<Lootr> branchs = new List<Lootr>();
        public List<Object> nameModifiers = new List<Object>();

        private Random r = new Random();

        /// <summary>
        /// Get a new branch
        /// </summary>
        /// <param name="name">Name of that branch</param>
        public Lootr(string name)
        {
            var branchName = name;

            branchName = this.clean(branchName);
            if (branchName.IndexOf('/') > -1)
            {
                throw new Exception("Specified name should not contain a / separator");
            }
            this.name = branchName;

        }

        /// <summary>
        /// Clean a path, trim left /characters
        /// </summary>
        /// <param name="name">Path to clean</param>
        /// <returns>Path  cleaned</returns>
        private string clean(string name)
        {
            if (name.StartsWith("/"))
                name = name.TrimStart('/');
            if (name.EndsWith("/"))
                name = name.TrimEnd('/');
            return name.Trim();
        }

        /// <summary>
        /// Add an item in current branch, or the nested branch specified
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="path">Path to branch</param>
        /// <returns>The current branch</returns>
        public Lootr add(Object item, string path = "")
        {
            if (path == "")
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

        /// <summary>
        /// Return or create a new branch under the current one
        /// </summary>
        /// <param name="name">Branch name</param>
        /// <returns>The current branch</returns>
        public Lootr branch(string name)
        {
            return this.getBranch(name, true);
        }

        /// <summary>
        /// Return or create a new branch under the current one
        /// </summary>
        /// <param name="name">Branch name</param>
        /// <param name="create">If true, and the specified branch does not exist, create one</param>
        /// <returns>The current branch</returns>
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

                if (this.name == head)
                {
                    return this.getBranch(newPath, create);
                }

                if (create)
                {
                    return this.branchs.First(b => b.name == head).getBranch(newPath, create);
                }
            }

            return null;
        }

        /// <summary>
        /// Return all items in the current and nested branchs
        /// </summary>
        /// <returns>List of items</returns>
        public List<Object> getAllItems()
        {
            var allItems = new List<Object>(this.items);

            foreach (var branch in this.branchs)
            {
                allItems.AddRange(branch.getAllItems());
            }

            return allItems;
        }

        /// <summary>
        /// Randomly pick an item
        /// </summary>
        /// <param name="allowedNesting">Depth limit</param>
        /// <param name="threshold">Chances (0-1) we go deeper</param>
        /// <returns>Picked item</returns>
        public Object randomPick(int allowedNesting = 0, float threshold = 1f)
        {
            var pickedItems = new List<Object>();

            if (r.NextDouble() < threshold && this.items.Count() > 0)
            {
                pickedItems.Add(this.items[r.Next(this.items.Count)]);
            }

            if (allowedNesting != 0)
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
            return this.items.Count > 0 && pickedItems.Count > 0 ? 
                pickedItems[r.Next(pickedItems.Count)] : null;
        }

        /// <summary>
        /// Randomly pick an item from the specified branch
        /// </summary>
        /// <param name="catalogPath">Branch to get an item from</param>
        /// <param name="nesting">Depth limit</param>
        /// <param name="threshold">Chances (0-1) we go deeper</param>
        /// <returns>Picked item</returns>
        public Object roll(string catalogPath, int nesting = 0, float threshold = 1f)
        {
            var branch = this.getBranch(catalogPath);

            return branch.randomPick(nesting, threshold);
        }

        /// <summary>
        /// Roll against a looting list
        /// </summary>
        /// <param name="drops">The looting list</param>
        /// <returns>List of items</returns>
        public List<Object> loot(List<Drop> drops)
        {
            var reward = new List<Object>();

            foreach (var drop in drops)
            {
                var item = this.roll(drop.Branch, drop.Depth, drop.Luck);
                if (item == null)
                {
                    continue;
                }

                var jsonItem = JsonConvert.SerializeObject(item);

                for (int i = 0; i < drop.Stack;i++ )
                {
                    var clone = JsonConvert.DeserializeObject(jsonItem, item.GetType());

                    reward.Add(clone);    
                }

            }

            return reward;
        }
    }
}
