namespace SemiWare.Utils
{
    /// <summary>
    /// Provides methods for repeatedly invoking an action.
    /// <para>
    ///	Useful for exception testing during development.
    /// </para>
    /// </summary>
    public static class Attempt
    {
        private static readonly int minAttempts = 1;

        /// <inheritdoc cref="ToDo{T}(Func{T}, TimeSpan, int)"/>
        public static void ToDo(Action action, TimeSpan interval, int maxAttempts = 3)
        {
            ToDo<object?>(() =>
            {
                action();
                return null;
            }, interval, maxAttempts);
        }

        /// <summary>
        /// Invokes a supplied action muliple times until either the action succeeds, or when invokation attempts have reached <paramref name="maxAttempts"/> amount of times. 
        /// Any exceptions that occured during the process will be logged on console, and later thrown together as an <see cref="AggregateException"/>.
        /// </summary>
        /// <typeparam name="T">The return type of the supplied <paramref name="action"/>.</typeparam>
        /// <param name="action">The method that will be retried.</param>
        /// <param name="interval">The time between each invoke attempt.</param>
        /// <param name="maxAttempts">The maximum amount of times the method will be invoked.</param>
        /// <exception cref="AggregateException"></exception>
        public static T ToDo<T>(Func<T> action, TimeSpan interval, int maxAttempts = 3)
        {
            maxAttempts = (maxAttempts < minAttempts) ? minAttempts : maxAttempts;
            var exceptions = new List<Exception>();
            for (int attempt = minAttempts; attempt <= maxAttempts; attempt++)
            {
                Console.WriteLine($"\nAttempt ({attempt}/{maxAttempts}):");
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Console.WriteLine($"\n'[{ex.GetType()}]' has occured!\n{ex.Message}");
                }
                Task.Delay(interval);
            }
            throw new AggregateException(exceptions);
        }


        /// <inheritdoc cref="ToDoRepeatedly{T}(Func{T}, TimeSpan, int)"/>
        public static IList<Tuple<Exception?>> ToDoRepeatedly(Action action, TimeSpan interval, int maxAttempts = 3)
        {
            var attemptResult = ToDoRepeatedly<object?>(() =>
            {
                action();
                return null;
            }, interval, maxAttempts);
            var exceptions = new List<Tuple<Exception?>>();
            foreach (var attempt in attemptResult)
            {
                exceptions.Add(new(attempt.Item2));
            }
            return exceptions;
        }

        /// <summary>
        /// Repeatedly invokes a supplied action <paramref name="maxAttempts"/> amount of times, then finally returns the result of each attempt.
        /// <para>
        /// This method should only be used during testing. Do NOT use in Production.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The return type of the supplied <paramref name="action"/>.</typeparam>
        /// <param name="action">The method that will be retried.</param>
        /// <param name="interval">The time between each invoke attempt.</param>
        /// <param name="maxAttempts">The maximum amount of times the method will be invoked.</param>
        /// <returns>
        /// A read only list containing the result of each invokation attempt. 
        /// <list type="bullet">
        /// <item>If an invokation attempt succeeded, the <see cref="Exception"/> in the <see cref="Tuple"/> will be <see langword="null"/>.</item>
        /// <item>If an invokation attempt failed, the <see cref="Exception"/> in the <see cref="Tuple"/> will be not be null.</item>
        /// </list>
        /// </returns>
        public static IList<Tuple<T?, Exception?>> ToDoRepeatedly<T>(Func<T> action, TimeSpan interval, int maxAttempts = 3)
        {
            maxAttempts = (maxAttempts < minAttempts) ? minAttempts : maxAttempts;
            var results = new List<Tuple<T?, Exception?>>();
            for (int attempt = minAttempts; attempt <= maxAttempts; attempt++)
            {
                T? returnedValue = default;
                Exception? exception = null;
                Console.WriteLine($"\nAttempt ({attempt}/{maxAttempts}): ");
                try
                {
                    returnedValue = action();
                    Console.Write($"Success! Retrieved: {returnedValue}");
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Console.Write($"Failed! An '[{ex.GetType()}]' has occured.\n{ex.Message}");
                }
                results.Add(new(returnedValue, exception));
                Task.Delay(interval);
            }
            return results;
        }
    }

}
