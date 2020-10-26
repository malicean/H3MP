using System;
using System.Collections.Generic;
using H3MP.Utils;

namespace H3MP.Peers
{
	public class EcsNet<T>
	{
		private Dictionary<Type, Action<EcsNet<T>>> _systems;

		public EcsNet()
		{
			_systems = new Dictionary<Type, Action<EcsNet<T>>>();
		}

		public void Add<TSystem, TComponentTuple, TEntity>(TSystem system) where TSystem : ISystem<TComponentTuple> where TComponentTuple : IComponentTuple<TEntity>
		{
			_systems.Add(typeof(TSystem),
		}
	}
}
