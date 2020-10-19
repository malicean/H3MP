using System;
using Discord;

namespace H3MP
{
	public class StatefulActivity
	{
		public delegate Activity ActivityUpdater(Activity activity);

		private ActivityManager _manager;
		private Activity _activity;

		private ActivityManager.ClearActivityHandler _callbackClear;
		private ActivityManager.UpdateActivityHandler _callbackUpdate;

		public StatefulActivity(ActivityManager manager, Action<Result> callback, Activity activity = default)
		{
			_manager = manager;
			_activity = activity;

			_callbackClear = result => callback(result);
			_callbackUpdate = result => callback(result);
		}

		public void Update(ActivityUpdater updater)
		{
			_activity = updater(_activity);
			_manager.UpdateActivity(_activity, _callbackUpdate);
		}

		public void Clear()
		{
			_activity = default;
			_manager.ClearActivity(_callbackClear);
		}
	}
}
