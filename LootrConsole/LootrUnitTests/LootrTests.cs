using System;
using LootrConsole;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace LootrUnitTests
{

    public class GoodException : Exception
    {

        public GoodException(string p)
            : base(p)
        {
        }

    }

    public class Item
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class LootrTests
    {
        private static Lootr GetStuff()
        {
            var loot = new Lootr("equipment");
            loot.add(new Item(){ Name = "Stuff", Color = "orange" });
            loot.branch("/equipment/weapons")
                .add(new Item() { Name = "Uzi" })
                .add(new Item() { Name = "Pistol" });
            loot.branch("/equipment/armor")
                .add(new Item() { Name = "Plates" })
                .add(new Item() { Name = "Leather" });
            loot.branch("/equipment/armor/tough")
                .add(new Item() { Name = "Military_vest" })
                .add(new Item() { Name = "CSI_cap" });

            return loot;
        }

        [Fact]
        public void SetupAssertionsTest()
        {
            try
            {
                var prefixedLootr = new Lootr("/notFaultyPath");
            }
            catch (Exception)
            {
                throw new Exception("\nDoes not fail if /-prefixed named branch");
            }

            try
            {
                var suffixedLootr = new Lootr("notFaultyPath/");
            }
            catch (Exception)
            {
                throw new Exception("Does not fail if /-suffixed named branch");
            }

            var loot = GetStuff();
            Assert.Equal(7, loot.getAllItems().Count);
            Assert.Throws<GoodException>(() =>
                {
                    try
                    {
                        var faultyLootr = new Lootr("/faulty/path");
                    }
                    catch (Exception)
                    {
                        throw new GoodException("Faulty named branch");
                    }
                });
        }

        [Fact]
        public void RollinUsageTest()
        {
            var loot = GetStuff();

            var weapons = new string[] { "Uzi", "Pistol" };
            var simplArmors = new string[] { "Plates", "Leather" };
            var toughArmors = new string[] { "Military_vest", "CSI_cap" };
            var all = new string[] { "Stuff" };
            all = all.Concat(weapons).Concat(simplArmors).Concat(toughArmors).ToArray();

            //should loot a useless equipment (Stuff)
            Assert.Equal("Stuff", (loot.roll("/equipment") as Item).Name);

            //should loot any equipment
            Assert.Equal(true, Array.IndexOf(all, (loot.roll("/equipment") as Item).Name) > -1);

            //should loot any equipment
            Assert.Equal(true, Array.IndexOf(all, (loot.roll("/equipment", 3, 100) as Item).Name) > -1);

            //should loot a weapon
            Assert.Equal(true, Array.IndexOf(weapons, (loot.roll("/equipment/weapons", 3) as Item).Name) > -1);

            //should loot a simple armor
            Assert.Equal(true, Array.IndexOf(simplArmors, (loot.roll("/equipment/armor") as Item).Name) > -1);

            //should loot an armor
            Assert.Equal(true, Array.IndexOf(simplArmors.Concat(toughArmors).ToArray(), (loot.roll("/equipment/armor", 1) as Item).Name) > -1);
        }

        [Fact]
        public void RollinUsageWithLuck()
        {
            var loot = GetStuff();

            var drops = new List<Drop>(){
                new Drop("/equipment", 1f, 1),
                new Drop("/equipment/armor", 0.5f, 2),
                new Drop("/equipment/weapons", 0.8f, 2)
            };
            var reward = loot.loot(drops);
            Assert.Equal(true, reward.Count > 0);
        }

        [Fact]
        public void TenThousandRollStats()
        {
            var loot = GetStuff();
            var all = loot.getAllItems();

            var drops = new List<Drop>(){
                new Drop("/equipment", 1f, 1),
                new Drop("/equipment/armor", 0.5f, 1, 1),
                new Drop("/equipment/weapons", 0.8f, 1, 1)
            };

            var rolls = 10000;
            Dictionary<string, int> overallRewards = new Dictionary<string, int>();
            for (int i = 0; i < rolls; i++)
            {
                var reward = loot.loot(drops);
                foreach (var item in reward)
                {
                    Item it = (item as Item);
                    if (overallRewards.ContainsKey(it.Name))
                        overallRewards[it.Name]++;
                    else
                        overallRewards.Add(it.Name, 1);
                }
            }

            bool allPresent = true;
            foreach (var item in all)
            {
                allPresent = allPresent && overallRewards.ContainsKey((item as Item).Name);
            }

            //At least there is one of each
            Assert.Equal(true, allPresent);
            //Everytime I get grey stuff
            Assert.Equal(true, overallRewards["Stuff"] >= rolls);

            var weaponRatio = Math.Round(((overallRewards["Uzi"] + overallRewards["Pistol"]) / (double)rolls), 2);
            var armorRatio = Math.Round(((overallRewards["Plates"] + overallRewards["Leather"] + overallRewards["Military_vest"] + overallRewards["CSI_cap"]) / (double)rolls), 2);

            Assert.Equal(true, weaponRatio >= 0.6 && weaponRatio <= 0.9);
            Console.WriteLine(weaponRatio * 100 + "% weapons");
            Assert.Equal(true, armorRatio >= 0.3 && armorRatio <= 0.7);
            Console.WriteLine(armorRatio * 100 + "% armory");

        }
    }
}
