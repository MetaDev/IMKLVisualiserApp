using UnityEngine;
using System.Collections;
using UniRx;
using System;
using UnityEngine.Networking;
using UI;
using Utility;
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
                        reportProgress.Report(www.downloadProgress);
                    }
                }
                catch (Exception ex)
                {
                    var message = Localization.WebErrorMessage + "error: " + ex.Message;
                    observer.OnError(ExceptionExtension.Rethrow(ex,message));
                    yield break;
                }
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (www.error != null)
            {
                var message = Localization.WebErrorMessage + "error: " + www.error + " " + www.responseCode;
                observer.OnError(new Exception(message));
            }
            else
            {
                observer.OnNext(www);
                observer.OnCompleted();
            }
        }
    }
}
