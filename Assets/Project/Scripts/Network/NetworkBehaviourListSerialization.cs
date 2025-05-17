// 1) Створюємо утилітний клас, де реєструємо наші кастомні writer/reader:

using System.Collections.Generic;
using Mirror;

namespace Project.Scripts.Network
{
    public static class NetworkBehaviourListSerialization
    {
        public static void WriteNetworkBehaviourList(this NetworkWriter writer, List<NetworkBehaviour> list)
        {
            writer.WriteInt(list.Count);
            foreach (var nb in list)
                // Mirror вміє писати окремий NetworkBehaviour через його NetworkIdentity
                writer.WriteNetworkBehaviour(nb);
        }

        public static List<NetworkBehaviour> ReadNetworkBehaviourList(this NetworkReader reader)
        {
            var count = reader.ReadInt();
            var list = new List<NetworkBehaviour>(count);
            for (var i = 0; i < count; i++) list.Add(reader.ReadNetworkBehaviour());
            return list;
        }

        /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            NetworkWriterExtensions.Register<List<NetworkBehaviour>>(WriteNetworkBehaviourList);
            NetworkReaderExtensions.Register<List<NetworkBehaviour>>(ReadNetworkBehaviourList);
        }*/
    }
}