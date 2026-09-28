using SemiWare.Utils;

namespace Tests.Utils
{
    [TestClass]
    public sealed class AttemptTest
    {
        private readonly TimeSpan intervalMs = TimeSpan.FromMilliseconds(10);


        [TestMethod]
        [TestCategory("Param: action")]
        public void ToDo_VoidAction_ActionIsInvoked_NoExceptionIsThrown()
        {
            Exception? testEx = null;
            try
            {
                Attempt.ToDo(() => Console.WriteLine("Print this!"), intervalMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsNull(testEx);
            }
        }
        [TestMethod]
        [TestCategory("Param: action")]
        public void ToDo_NoExplicitGenericType_ReturnableActionRuns_NoExceptionIsThrown()
        {
            Exception? testEx = null;
            int? result = null;
            try
            {
                result = Attempt.ToDo(() => int.Parse("12"), intervalMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
            }
            finally
            {
                Assert.IsNull(testEx);
                Assert.IsNotNull(result);
            }
        }
        [TestMethod]
        [TestCategory("Param: action")]
        public void ToDo_ErrorProneAction_ActionThrows_ThrowAggregateException()
        {
            Exception? testEx = null;
            try
            {
                Attempt.ToDo<int>(() => int.Parse("12a"), intervalMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsInstanceOfType<AggregateException>(testEx);
            }
        }


        [TestMethod]
        [TestCategory("Param: interval")]
        public void ToDo_NegativeInterval_ActionRuns_ThrowArgumentOutOfRangeException()
        {
            Exception? testEx = null;
            int? result = null;
            try
            {
                result = Attempt.ToDo<int>(() => int.Parse("12"), interval: TimeSpan.FromMilliseconds(-100), maxAttempts: 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsNull(testEx);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("Param: maxAttempts")]
        [DataRow(0)]
        [DataRow(-10)]
        public void ToDo_NonPositiveMaxAttempts_ActionRuns_ExceptionIsNotThrown(int attempts)
        {
            Exception? testEx = null;
            int? result = null;
            try
            {
                result = Attempt.ToDo<int>(() => int.Parse("12"), intervalMs, maxAttempts: attempts);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsNull(testEx);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("Param: maxAttempts")]
        public void ToDo_OverflowMaxAttempts_ActionRuns_ExceptionIsNotThrown()
        {
            int? result = null;
            Exception? testEx = null;
            try
            {
                int maxValue = int.MaxValue;
                result = Attempt.ToDo<int>(
                    () => int.Parse("12"), intervalMs, maxAttempts: maxValue + 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsNull(testEx);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("Param: maxAttempts")]
        public void ToDo_UnderflowMaxAttempts_ActionRuns_ExceptionIsNotThrown()
        {
            int? result = null;
            Exception? testEx = null;
            try
            {
                int minValue = int.MinValue;
                result = Attempt.ToDo<int>(
                    () => int.Parse("12"), intervalMs, maxAttempts: minValue - 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wow! Exception is found: " + ex);
                testEx = ex;
            }
            finally
            {
                Assert.IsNull(testEx);
                Assert.IsNotNull(result);
            }
        }


        [TestMethod]
        public void ToDoRepeatedly_ActionThatCanThrowInnerException_ActionRuns_DoNotThrowOuterException()
        {
            Exception? exception = null;
            var results = new List<Tuple<int?, Exception?>>();
            var rng = new Random();
            try
            {
                results = Attempt.ToDoRepeatedly<int?>(() =>
                {
                    var number = rng.Next(1, 5);
                    if (number > 2)
                    {
                        throw new ArgumentOutOfRangeException(nameof(number));
                    }
                    return number;
                }, intervalMs, 50).ToList();
            }
            catch (Exception ex)
            {
                exception = ex;
                Console.WriteLine("Wow! Exception is found: " + ex);
            }
            finally
            {
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void ToDoRepeatedly_TwoDifferentInterval_TwoActionRuns_OneActionIsSlower()
        {
            var timer1 = new System.Diagnostics.Stopwatch();
            timer1.Start();
            _ = Attempt.ToDoRepeatedly(() => Console.WriteLine("Yeay"), TimeSpan.FromMilliseconds(10), 20);
            timer1.Stop();

            var timer2 = new System.Diagnostics.Stopwatch();
            timer2.Start();
            _ = Attempt.ToDoRepeatedly(() =>
            {
                Console.WriteLine("Yeay");
            }, TimeSpan.FromSeconds(10), 20);
            timer2.Stop();

            Console.WriteLine($"Timer2: {timer2.Elapsed.TotalMilliseconds} | Timer1: {timer1.Elapsed.TotalMilliseconds}");
            Assert.IsGreaterThan(timer2.Elapsed.TotalMilliseconds, timer1.Elapsed.TotalMilliseconds);
        }

    }
}
