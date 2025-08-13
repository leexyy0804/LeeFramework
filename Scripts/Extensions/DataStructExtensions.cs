using System.Collections.Generic;

namespace LeeFramework.Scripts.Extensions
{
    public static class DataStructExtensions
    {
        public static void AddRange<T>(this HashSet<T> ori, List<T> list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                ori.Add(list[i]);
            }
        }
    }
}
