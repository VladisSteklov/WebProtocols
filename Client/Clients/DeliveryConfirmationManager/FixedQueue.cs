using System;
using System.Collections.Generic;

namespace Client.Clients.DeliveryConfirmationManager;

public class FixedSizedQueue<T> : Queue<T>
{
	private readonly int _size;

	public FixedSizedQueue(int size)
	{
		_size = size;
	}

	public new void Enqueue(T value)
	{
		base.Enqueue(value);

		while (Count > _size)
		{
			Dequeue();
		}
	}

	public void FillQueue(T value)
	{
		Clear();

		while (Count < _size)
		{
			base.Enqueue(value);
		}
	}
}