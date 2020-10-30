using System.Collections.Generic;

namespace H3MP.IO
{
	public interface IBuffer<T>
	{
		int Length { get; }

		int Capacity { get; }

		void Push(T value);

		void Clear();
	}
}
