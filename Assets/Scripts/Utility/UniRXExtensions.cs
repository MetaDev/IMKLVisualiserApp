using UnityEngine;
using System.Collections;
using UniRx;
using System;
using UnityEngine.Networking;

namespace UniRx
{

    public static class UniRXExtensions
    {

        public static IObservable<UnityWebRequest> GetWWW(UnityWebRequest www, IProgress<float> reportProgress=null)
        {
            // convert coroutine to IObservable
            return Observable.FromCoroutine<UnityWebRequest>((observer, cancellationToken) => GetWWWCore(www, observer,reportProgress, cancellationToken));
        }

        static IEnumerator GetWWWCore(UnityWebRequest www, IObserver<UnityWebRequest> observer, IProgress<float> reportProgress,CancellationToken cancellationToken)
        {
            yield return www.Send();
            while (!www.isDone && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (reportProgress != null)
                    {
						Debug.Log(www.downloadProgress);
                        reportProgress.Report(www.downloadProgress);
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    yield break;
                }
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (www.error != null)
            {
                observer.OnError(new Exception(www.error + " " + www.responseCode));
            }
            else
            {
                observer.OnNext(www);
                observer.OnCompleted();
            }
        }
    }
}
