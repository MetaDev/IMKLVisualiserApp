using System;
namespace Utility
{
    public static class ExceptionExtension
    {

        public static Exception Rethrow(Exception ex, string message)
        {
            if (ex == null)
            {
                ex = new Exception("Error rethrowing exception because original exception is <null>.");
            }

            Exception rethrow;

            try
            {
                rethrow = (Exception)Activator.CreateInstance(ex.GetType(), message, ex);
            }
            catch (Exception)
            {
                rethrow = new Exception(message, ex);
            }
            return rethrow;
        }
    }
}


