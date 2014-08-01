using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootrConsole
{
    class Program
    {
        static string lootText = "";
        static void Main(string[] args)
        {
            var loot = new Lootr("equipment");
            loot.add(new { name = "Stuff", color = "orange" });
            loot.branch("/equipment/weapons")
                .add(new { name = "Uzi" })
                .add(new { name = "Pistol" });
            loot.branch("/equipment/armor")
                .add(new { name= "Plates" })
                .add(new { name= "Leather" });
            loot.branch("/equipment/armor/tough")
                .add(new { name= "Military_vest" })
                .add(new { name= "CSI_cap" });
            getLootBranch(loot);
            Console.Write(lootText);
            Console.Read();
        }
        /// <summary>
        /// Write the list of branch
        /// </summary>
        /// <param name="loot">head loot node</param>
        private static void getLootBranch(Lootr loot)
        {
            lootText += loot.name;

            lootText += "\n";
            foreach (var branch in loot.branchs)
            {
                lootText += loot.name+"->";
                getLootBranch(branch);
            }
            lootText += "\n";
        }
    }
}
