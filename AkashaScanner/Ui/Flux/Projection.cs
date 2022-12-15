using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AkashaScanner.Ui.StateManagerment
{
    public partial class Flux
    {
        private readonly Dictionary<Type, Dictionary<ISubject, List<(Projection, PropertyInfo)>>> Subscribers = new();
        private readonly object Mutex = new(); // Probably should not use this but meh

        private void Notify<T>(Predicate<T>? predicate = null) where T : Projection
        {
            lock (Mutex)
            {
                if (Subscribers.TryGetValue(typeof(T), out var dict))
                {
                    foreach (var (subject, list) in dict)
                    {
                        if (!subject.Disposed)
                        {
                            foreach (var (projection, prop) in list)
                            {
                                if (predicate == null || predicate((T)projection))
                                    projection.SetValue(this, subject, prop);
                            }
                        }
                    }
                }
            }
        }

        public void UnsubscribeAll(ISubject subject)
        {
            lock (Mutex)
            {
                foreach (var dict in Subscribers.Values)
                {
                    dict.Remove(subject);
                }
            }
        }

        private void Subscribe(Projection projection, ISubject subject, PropertyInfo prop)
        {
            Logger.LogDebug("Subscribe {subject} {prop}", subject, prop);
            var projectionType = projection.GetType();
            lock (Mutex)
            {
                if (!Subscribers.ContainsKey(projectionType))
                {
                    Subscribers[projectionType] = new();
                }
                var subjectProjections = Subscribers[projectionType];
                if (!subjectProjections.ContainsKey(subject))
                {
                    subjectProjections[subject] = new();
                }
                subjectProjections[subject].Add((projection, prop));
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public abstract class Projection : Attribute
        {
            private object? CachedValue;

            public abstract object? GetValue(Flux flux);

            public void SetValue(Flux flux, ISubject subject, PropertyInfo prop)
            {
                var value = GetValue(flux);
                prop.SetValue(subject, value);
                if (value != CachedValue)
                {
                    CachedValue = value;
                    subject.OnFluxStateChanged(prop, value);
                }
            }

            public void Subscribe(Flux flux, ISubject subject, PropertyInfo prop)
            {
                flux.Logger.LogDebug("Subscribe {subject} {prop}", subject, prop);
                SetValue(flux, subject, prop);
                flux.Subscribe(this, subject, prop);
            }
        }

        public interface ISubject
        {
            bool Disposed { get; }
            void OnFluxStateChanged(PropertyInfo property, object? value);
        }
    }
}
