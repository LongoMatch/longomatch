// 
//  Copyright (C) 2011 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using System;
using System.Collections.Generic;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Interfaces
{
	public interface IRenderingJobsManager
	{
		void RetryJobs (List<Job> retryJobs);

		void DeleteJob (Job job);

		void ClearDoneJobs ();

		void CancelJobs (List<Job> cancelJobs);

		void CancelCurrentJob ();

		void CancelJob (Job job);

		void CancelAllJobs ();

		void AddJob (Job job);

		List<Job> Jobs { get; }

		List<Job> PendingJobs { get; }
	}
}

