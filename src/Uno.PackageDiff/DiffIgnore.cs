using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Uno.PackageDiff
{
	[DataContract(Name = "DiffIgnore", Namespace = "")]
	public class DiffIgnore
	{
		public IgnoreSet[] IgnoreSets { get; set; }
	}

	[DataContract(Name = "IgnoreSet", Namespace = "")]
	public class IgnoreSet
	{
		[DataMember()]
		public string BaseVersion { get; set; }

		[DataMember()]
		public string[] Properties { get; set; }

		[DataMember()]
		public string[] Methods { get; set; }

		[DataMember()]
		public string[] Fields { get; set; }

		[DataMember()]
		public string[] Events { get; set; }
	}
}
