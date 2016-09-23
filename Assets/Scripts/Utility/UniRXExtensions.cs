using UnityEngine;
using System.Collections;
using UniRx;
using System;
using UnityEngine.Networking;

namespace UniRx
{

	public static class UniRXExtensions
	{

		public static IObservable<byte[]> GetWWW (UnityWebRequest www)
		{
			// convert coroutine to IObservable
			return Observable.FromCoroutine<byte[]> ((observer, cancellationToken) => GetWWWCore (www, observer, cancellationToken));
		}

		static IEnumerator GetWWWCore (UnityWebRequest www, IObserver<byte[]> observer, CancellationToken cancellationToken)
		{
			yield return www.Send();
			while (!www.isDone && !cancellationToken.IsCancellationRequested) {
				yield return null;
			}

			if (cancellationToken.IsCancellationRequested)
				yield break;

			if (www.error != null) {
				observer.OnError (new Exception (www.error+ " " +www.responseCode));
			} else {
				observer.OnNext (www.downloadHandler.data);
				observer.OnCompleted ();
			}
		}
	}
}
