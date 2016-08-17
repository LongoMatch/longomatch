//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Interfaces
{
	public interface IConfig : VAS.Core.Interfaces.IConfig
	{
		Hotkeys Hotkeys {
			get;
			set;
		}
	}
}

