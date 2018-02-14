//
//  Copyright (C) 2017 Fluendo S.A.
//
//
using System;
using VAS.Core;

namespace LongoMatch.Core.Resources
{
	public class Strings : VAS.Core.Resources.Strings
	{
		public static string TrialExpireInMultiple => Catalog.GetString ("Your trial period will expire in <b>{0} days</b>");
		public static string TrialExpireInOneDay => Catalog.GetString ("Your trial period will expire in <b>1 day</b>");
		public static string TrialExpireToday => Catalog.GetString ("Your trial period will expire <b>today</b>");
		public static string UpgradeToUnlock => Catalog.GetString ("Upgrade to unlock more features!");
		
		public static string NoName => Catalog.GetString ("No Name");
		public static string NoLeague => Catalog.GetString ("No League");
		public static string NoSeason => Catalog.GetString ("No Season");
		public static string NoMatchDate => Catalog.GetString ("No Match Date");
	}
}
