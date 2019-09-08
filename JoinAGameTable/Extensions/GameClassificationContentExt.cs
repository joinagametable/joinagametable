using System.Collections.Generic;
using JoinAGameTable.Enumerations;

namespace JoinAGameTable.Extensions
{
    public static class GameClassificationContentExt
    {
        /// <summary>
        /// Convert a list of integer to a list of <see cref="GameClassificationContentEnum"/>.
        /// </summary>
        /// <param name="values">A list of integer</param>
        /// <returns>A list of <see cref="GameClassificationContentEnum"/></returns>
        public static List<GameClassificationContentEnum> ToEnumList(List<int> values)
        {
            return values.ConvertAll(input => (GameClassificationContentEnum) input);
        }

        /// <summary>
        /// Convert a list of <see cref="GameClassificationContentEnum"/> to a list of integer. The
        /// returned list is sorted.
        /// </summary>
        /// <param name="values">A list of integer</param>
        /// <returns>A list of integer</returns>
        public static List<int> ToIntegerList(List<GameClassificationContentEnum> values)
        {
            var list = values.ConvertAll(input => (int) input);
            list.Sort();
            return list;
        }
    }
}
