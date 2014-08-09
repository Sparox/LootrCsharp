LootrCsharp
===========
[![Build Status](https://travis-ci.org/Sparox/LootrCsharp.svg?branch=master)](https://travis-ci.org/Sparox/LootrCsharp)
A loot system translated in Csharp from https://github.com/vincent/lootr

Adding items
=====

LootrCsharp, as his brother Lootr, is organized as a tree.

```cs
var loot = new Lootr("equipment");
loot.add(new { Name = "Stuff" });
```

Or with a real item wich have a name property :

```cs
loot.add(new Item("Stuff"));
```

Each level is composed by a list of items in List<Object> items and a list of branchs in List<Lootr> branchs
You can organize your repositories by adding some branchs

```cs
loot.branch("weapons");
loot.branch("/equipment/armor");
```

The 'branch' method returns itself, on wich you can 'add' items or nested branchs.

```cs
loot.branch("/equipment/weapons")
	.add(new Item() { Name = "Uzi" })
	.add(new Item() { Name = "Pistol" });
```

Rollin
=====

Random-pick something at top level with `Lootr.roll( path, depth = 0, chance = 1 )`

It will yield an item in the `path` branch or, if `depth` is given, in an up to `depth` deep branchs, if the depth-decreasing `chance` is greater than a `Math.random()`

```cs
//Loot something from top level
loot.roll("/equipment"); //only 'Stuff'

//Loot something from anywhere
loot.roll("/equipment", Lootr.INFINITY, 1f); //any item

//Loot an armor
loot.roll("/equipment/armor"); //one of ["Plates", "Leather"]

//Loot a weapon
loot.roll("/equipment/weapons", 3); //one of ["Pistol", "Uzi"]

```

Lootin'
=====

```cs
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

deadMonster.drops = new List<Drop>(){
			new Drop("/equipment", 1f, 1),
			new Drop("/equipment/armor", 0.5f, 2),
			new Drop("/equipment/weapons", 0.8f, 2)
		};
		
//Loot your reward from a dead monster
List<Object> reward = loot.loot(drops);

//reward can contain a random list of objects like : 'Stuff', 'Plates', 'Uzi', 'Uzi'
```
