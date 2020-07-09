using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Uno.PackageDiff
{
	public class DiffIgnore
	{
		[XmlArray()]
		public IgnoreSet[] IgnoreSets { get; set; }

		public static IgnoreSet ParseDiffIgnore(string diffIgnoreFile, string baseVersion)
		{
			if(!string.IsNullOrEmpty(diffIgnoreFile))
			{
				using(var file = File.OpenRead(diffIgnoreFile))
				{
					return ParseDiffIgnore(baseVersion, file);
				}
			}

			return new IgnoreSet();
		}

		public static IgnoreSet ParseDiffIgnore(string baseVersion, Stream stream)
		{
			var dcs = new XmlSerializer(typeof(DiffIgnore));
			var diffIgnore = (DiffIgnore)dcs.Deserialize(stream);

			if(diffIgnore.IgnoreSets.FirstOrDefault(s => s.BaseVersion == baseVersion) is IgnoreSet set)
			{
				return set;
			}

			return new IgnoreSet();
		}
	}

	public class IgnoreSet
	{
		[XmlAttribute("baseVersion")]
		public string BaseVersion { get; set; } = "";

		[XmlArray()]
		public Member[] Assemblies { get; set; } = new Member[0];

		[XmlArray()]
		public Member[] Types { get; set; } = new Member[0];

		[XmlArray()]
		public Member[] Properties { get; set; } = new Member[0];

		[XmlArray()]
		public Member[] Methods { get; set; } = new Member[0];

		[XmlArray()]
		public Member[] Fields { get; set; } = new Member[0];

		[XmlArray()]
		public Member[] Events { get; set; } = new Member[0];
	}

	public class Member
	{
		[XmlAttribute("fullName")]
		public string FullName { get; set; }
	}
}
