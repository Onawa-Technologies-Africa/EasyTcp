using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyTcp3.Actions.ActionsCore
{
    public class Action
    {
        /// <summary>
        /// Different EasyTcpAction delegate types
        /// </summary>
        private delegate void EasyTcpActionDelegate(object sender, Message message);

        private delegate void EasyTcpActionDelegate1(Message message);

        private delegate void EasyTcpActionDelegate2();

        private delegate Task EasyTcpActionDelegate3(object sender, Message message);

        private delegate Task EasyTcpActionDelegate4(Message message);

        private delegate Task EasyTcpActionDelegate5();

        /// <summary>
        /// Instance of any EasyTcpActionDelegate
        /// </summary>
        private Delegate EasyTcpAction;

        /// <summary>
        /// Create new action
        /// </summary>
        /// <param name="method">method that matches any EasyTcpActionDelegate</param>
        /// <param name="classInstances">dictionary with instances of already initialized classes</param>
        public Action(MethodInfo method, Dictionary<Type, object> classInstances)
        {
            var classInstance = GetClassInstance(method, classInstances);
            var methodType = GetDelegateType(method);

            if (classInstance == null) EasyTcpAction = Delegate.CreateDelegate(methodType, method);
            EasyTcpAction = Delegate.CreateDelegate(methodType, classInstance, method);
        }

        /// <summary>
        /// Executes action
        /// </summary>
        /// <param name="sender">instance of EasyTcpClient or EasyTcpServer</param>
        /// <param name="message">received message</param>
        public async Task Execute(object sender = null, Message message = null)
        {
            var type = EasyTcpAction.GetType();
            if (type == typeof(EasyTcpActionDelegate))
                ((EasyTcpActionDelegate) EasyTcpAction)(sender, message);
            else if (type == typeof(EasyTcpActionDelegate1))
                ((EasyTcpActionDelegate1) EasyTcpAction)(message);
            else if (type == typeof(EasyTcpActionDelegate2))
                ((EasyTcpActionDelegate2) EasyTcpAction)();
            else if (type == typeof(EasyTcpActionDelegate3))
                await ((EasyTcpActionDelegate3) EasyTcpAction)(sender, message);
            else if (type == typeof(EasyTcpActionDelegate4))
                await ((EasyTcpActionDelegate4) EasyTcpAction)(message);
            else if (type == typeof(EasyTcpActionDelegate5))
                await ((EasyTcpActionDelegate5) EasyTcpAction)();
        }

        /// <summary>
        /// Get instance of declaring class
        /// gets instance from classInstances when possible,
        /// else add new instance to classInstances
        /// </summary>
        /// <param name="method">method that matches any EasyTcpActionDelegate</param>
        /// <param name="classInstances">list with initialized classes</param>
        /// <returns>null if method is static, else instance of declaring class</returns>
        private static object GetClassInstance(MethodInfo method, Dictionary<Type, object> classInstances)
        {
            if (method.IsStatic) return null;

            var classType = method.DeclaringType;
            if (!classInstances.TryGetValue(classType, out object instance))
            {
                instance = Activator.CreateInstance(classType);
                classInstances.Add(classType, instance);
            }

            return instance;
        }

        /// <summary>
        /// Get EasyTcpActionDelegate type from methodInfo 
        /// </summary>
        /// <param name="m"></param>
        /// <returns>type of any EasyTcpActionDelegate or null when none</returns>
        private static Type GetDelegateType(MethodInfo m)
        {
            var p = m.GetParameters();

            if (m.ReturnType == typeof(void))
            {
                if (p.Length == 2 && p[0].ParameterType == typeof(object) && p[1].ParameterType == typeof(Message))
                    return typeof(EasyTcpActionDelegate);
                if (p.Length == 1 && p[0].ParameterType == typeof(Message))
                    return typeof(EasyTcpActionDelegate1);
                if (p.Length == 0 && m.ReturnType == typeof(void)) return typeof(EasyTcpActionDelegate2);
            }
            else if (m.ReturnType == typeof(Task))
            {
                if (p.Length == 2 && p[0].ParameterType == typeof(object) && p[1].ParameterType == typeof(Message))
                    return typeof(EasyTcpActionDelegate3);
                if (p.Length == 1 && p[0].ParameterType == typeof(Message))
                    return typeof(EasyTcpActionDelegate4);
                if (p.Length == 0) return typeof(EasyTcpActionDelegate5);
            }
            return null;
        }

        /// <summary>
        /// Determines whether a method is a valid action
        /// </summary>
        /// <param name="m"></param>
        /// <returns>true if method is a valid action</returns>
        public static bool IsValidAction(MethodInfo m) =>
            m.GetCustomAttributes().OfType<EasyTcpAction>().Any() && GetDelegateType(m) != null;
    }
}