
namespace LootrConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            GetStuff();
        }

        private static Lootr GetStuff()
        {
            var loot = new Lootr("equipment");
            loot.add(new { name = "Stuff", color = "orange" });
            loot.branch("/equipment/weapons")
                .add(new { name = "Uzi" })
                .add(new { name = "Pistol" });
            loot.branch("/equipment/armor")
                .add(new { name = "Plates" })
                .add(new { name = "Leather" });
            loot.branch("/equipment/armor/tough")
                .add(new { name = "Military_vest" })
                .add(new { name = "CSI_cap" });

            return loot;
        }
    }
}
