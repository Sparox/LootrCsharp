using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class Lootr
{

	private string name;
	private List<Object> items = new List<Object>();
	private List<Lootr> branchs = new List<Lootr>();
	private List<Object> nameModifiers = new List<Object>();

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
			branch.items.add(item);
		}

		return this;
	}

	public Lootr branch(string name)
	{
		return this.getBranch(name, true);
	}

	public Lootr getBranch(string name, bool create)
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

}
