using System;
using System.Collections.Generic;

namespace JCUnityTeam.AdsImplementation
{
    public class SubscriberHandler<T>
    {
        private List<T> subscriptions = new List<T>();

        public void Subscribe(T listener) => subscriptions.Add(listener);
        public void Unsubscribe(T listener) => subscriptions.Remove(listener);

        public void Call(Action<T> callback)
        {
            foreach (T listener in subscriptions)
            {
                callback.Invoke(listener);
            }
        }
    }
}