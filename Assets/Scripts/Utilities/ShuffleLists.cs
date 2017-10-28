using System;
using System.Collections.Generic;
using System.Threading;

public static class ThreadSafeRandom
{
	[ThreadStatic] private static Random Local;

	public static Random ThisThreadsRandom
	{
		get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
	}
}

static class ShuffleLists
{
	public static void Shuffle<T>(this IList<T> list, Random RNG)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
            int k = RNG.Next(n + 1);//ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
