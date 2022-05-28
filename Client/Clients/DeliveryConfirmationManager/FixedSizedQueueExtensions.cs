using System;
using System.Linq;

namespace Client.Clients.DeliveryConfirmationManager;

internal static class FixedSizedQueueExtensions
{
	public static TimeSpan CalculateRtt(this FixedSizedQueue<TimeSpan> fixedSizedQueue)
	{
		var average = new TimeSpan(Convert.ToInt64(fixedSizedQueue.Average(timeSpan => timeSpan.Ticks)));

		TimeSpan jitter = default;
		var arr = fixedSizedQueue.ToArray();
		for (var i = 0; i < arr.Length - 1; i++)
		{
			var diff = arr[i + 1] - arr[i];
			jitter += diff.Duration();
		}

		return average + new TimeSpan(jitter.Ticks / fixedSizedQueue.Count);
	}
}
