using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Paymetheus.Framework
{
    public static class Messenger
    {
        static Dictionary<Type, MessageHandler> instances = new Dictionary<Type, MessageHandler>();

        public delegate void MessageHandler(ViewModelMessageBase message);

        public static void RegisterSingleton<T>(MessageHandler handler) where T : ViewModelBase
        {
            instances[typeof(T)] = handler;
        }

        public static void MessageSingleton<T>(ViewModelMessageBase message) where T : ViewModelBase
        {
            instances[typeof(T)](message);
        }
    }
}
