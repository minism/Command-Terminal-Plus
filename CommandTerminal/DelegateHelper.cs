using System;
using System.Reflection;

/**
 * Really clever performant dynamic delegate system ripped off from
 * https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
 *
 * Adapted/simplified to work with static functions of void return type.
 */
namespace CommandTerminalPlus {

  public static class DelegateHelper {
    public static Action<object> MagicMethod1(MethodInfo method) {
      return (Action<object>)MagicMethod<Action<object>>(method, "MagicMethodHelper1", 1);
    }

    public static Action<object, object> MagicMethod2(MethodInfo method) {
      return (Action<object, object>)MagicMethod<Action<object, object>>(method, "MagicMethodHelper2", 2);
    }

    static T MagicMethod<T>(MethodInfo method, string helperName, int numArgs) {
      // First fetch the generic form
      MethodInfo genericHelper = typeof(DelegateHelper).GetMethod(helperName,
          BindingFlags.Static | BindingFlags.NonPublic);

      // Now supply the type arguments
      Type[] paramTypes = new Type[numArgs];
      for (int i = 0; i < numArgs; i++) {
        paramTypes[i] = method.GetParameters()[i].ParameterType;
      }
      MethodInfo constructedHelper = genericHelper.MakeGenericMethod(paramTypes);

      // Now call it. The null argument is because it’s a static method.
      object ret = constructedHelper.Invoke(null, new object[] { method });

      // Cast the result to the right kind of delegate and return it
      return (T)ret;
    }

    static Action<object> MagicMethodHelper1<T1>(MethodInfo method) {
      // Convert the slow MethodInfo into a fast, strongly typed, open delegate
      Action<T1> func = (Action<T1>)Delegate.CreateDelegate
          (typeof(Action<T1>), method);

      // Now create a more weakly typed delegate which will call the strongly typed one
      Action<object> ret = (object p1) => func((T1)p1);
      return ret;
    }

    static Action<object, object> MagicMethodHelper2<T1, T2>(MethodInfo method) {
      Action<T1, T2> func = (Action<T1, T2>)Delegate.CreateDelegate
          (typeof(Action<T1, T2>), method);
      Action<object, object> ret = (object p1, object p2) => func((T1)p1, (T2)p2);
      return ret;
    }
  }

}
