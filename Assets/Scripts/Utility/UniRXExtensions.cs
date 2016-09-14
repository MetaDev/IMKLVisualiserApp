using UnityEngine;
using System.Collections;
using UniRx;
using System;

namespace Utility
{

	public static class UniRXExtensions
	{

		public static IObservable<byte[]> GetWWW (string url)
		{
			// convert coroutine to IObservable
			return Observable.FromCoroutine<byte[]> ((observer, cancellationToken) => GetWWWCore (url, observer, cancellationToken));
		}

		static IEnumerator GetWWWCore (string url, IObserver<byte[]> observer, CancellationToken cancellationToken)
		{
			var www = new UnityEngine.WWW (url);
			while (!www.isDone && !cancellationToken.IsCancellationRequested) {
				yield return null;
			}

			if (cancellationToken.IsCancellationRequested)
				yield break;

			if (www.error != null) {
				observer.OnError (new Exception (www.error));
			} else {
				observer.OnNext (www.bytes);
				observer.OnCompleted ();
			}
		}
	}
}
