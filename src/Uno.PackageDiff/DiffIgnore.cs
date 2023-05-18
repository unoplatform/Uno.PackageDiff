using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
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

			if(diffIgnore.IgnoreSets.FirstOrDefault(s => CompareVersion(baseVersion, s.BaseVersion)) is IgnoreSet set)
			{
				return set;
			}

			return new IgnoreSet();
		}

		private static bool CompareVersion(string baseVersion, string ignoreSetVersion)
		{
			var ignoreSetVersionDotCount = ignoreSetVersion.Count(c => c == '.');
			var baseVersion2 = new Version(baseVersion);
			var ignoreSetVersion2 = new Version(ignoreSetVersionDotCount == 0 ? ignoreSetVersion + ".0" : ignoreSetVersion);

			return ignoreSetVersionDotCount switch
			{
				0 => baseVersion2.Major == ignoreSetVersion2.Major,
				1 => baseVersion2.Major == ignoreSetVersion2.Major
					&& baseVersion2.Minor == ignoreSetVersion2.Minor,
				_ => ignoreSetVersion == baseVersion
			};
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

		[XmlAttribute("isRegex")]
		public bool IsRegex { get; set; }

		public bool Matches(string member, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return IsRegex
				? Regex.IsMatch(member, FullName)
				: FullName.Equals(member, stringComparison);
		}
	}
}
