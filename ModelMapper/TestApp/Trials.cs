using System;
using System.Linq.Expressions;

namespace TestApp
{
    class Trials
    {
        static Func<object, object> Convert<T1, T2>(Func<T1, T2> f)
        {
            return o => f((T1)o);
        }

        static Func<T1, object> Pass<T1, T2>(Expression<Func<T1, T2>> expr)
        {
            var obj = expr.Compile();
            var newObj = Convert(obj);

            //var param = Expression.Parameter(typeof(T1), expr.Parameters[0].Name);
            var lmda = Expression.Lambda<Func<T1, object>>(expr.Body, expr.Parameters[0]);
            var lmdaF = lmda.Compile();
            return lmdaF;

            //var expr2 = Expression.Lambda(expr.);
            //var dlgt = expr2.Compile();
            //var x = dlgt.DynamicInvoke();
            //var lamda = Expression.Lambda(expr.Body);
            //var dgt = Delegate.CreateDelegate(obj.GetType(), obj.Method);
            //var dgt = Delegate.CreateDelegate(obj.GetType(), obj, "some");
            //string r = (string)dgt.DynamicInvoke(5);
        }

        static void TestExpr()
        {
            int x;
            //var f = Pass<int, string>(x => x.ToString());
            var f = Pass<int, string>(x => "x");
            var m = f(5);
        }
    }
}
