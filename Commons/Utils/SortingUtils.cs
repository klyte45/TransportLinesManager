using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;

namespace Klyte.Commons.Utils
{
    public class SortingUtils
    {
        #region Sorting

        public static int NaturalCompare(string left, string right)
        {
            return (int)typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", RedirectorUtils.allFlags).Invoke(null, new object[] { left, right });
        }

        public static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert)
        {
            Quicksort(elements, 0, elements.Count - 1, comp, invert);
        }

        protected static void Quicksort(IList<UIComponent> elements, int left, int right, Comparison<UIComponent> comp, bool invert)
        {
            int i = left;
            int num = right;
            UIComponent y = elements[(left + right) / 2];
            int multiplier = invert ? -1 : 1;
            while (i <= num)
            {
                while (comp(elements[i], y) * multiplier < 0)
                {
                    i++;
                }
                while (comp(elements[num], y) * multiplier > 0)
                {
                    num--;
                }
                if (i <= num)
                {
                    UIComponent value = elements[i];
                    elements[i] = elements[num];
                    elements[i].forceZOrder = i;
                    elements[num] = value;
                    elements[num].forceZOrder = num;
                    i++;
                    num--;
                }
            }
            if (left < num)
            {
                Quicksort(elements, left, num, comp, invert);
            }
            if (i < right)
            {
                Quicksort(elements, i, right, comp, invert);
            }
        }
        #endregion

    }
}
