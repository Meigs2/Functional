using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Functional.Core.Enumeration;

namespace Functional.Core
{
    public abstract record FlaggedEnumerationBase<TObject, TValue> : 
        Enumeration<TObject, TValue>
        where TObject : FlaggedEnumerationBase<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
    {
        protected FlaggedEnumerationBase(string name, TValue value) : base(name, value)
        { }

        /// <summary>
        /// Returns an <see cref="IEnumerable{TObject}"/> representing a given <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value to retrieve.</param>
        /// <param name="allEnumList">an <see cref="IEnumerable{T}"/> of <see cref="FlaggedEnumeration{TObject,TValue}"/> from which to retrieve values.</param>
        /// <returns></returns>
        protected static Option<IEnumerable<TObject>> GetFlagEnumValues(TValue value, IEnumerable<TObject> allEnumList)
        {
            if (value == null)
                return Option.None;
                
            if (!int.TryParse(value.ToString(), out _))
                return Option.None;
            
            AllowNegativeInputValuesAttribute attribute = (AllowNegativeInputValuesAttribute)
                Attribute.GetCustomAttribute(typeof(TObject), typeof(AllowNegativeInputValuesAttribute));

            if (attribute == null && int.Parse(value.ToString()) < -1)
            {
                return Option.None;
            }

            var inputValueAsLong = long.Parse(value.ToString());
            var enumFlagStateDictionary = new Dictionary<TObject, bool>();
            var inputObjectList = allEnumList.ToList();

            ApplyUnsafeFlagEnumAttributeSettings(inputObjectList);

            var maximumAllowedValue = CalculateHighestAllowedFlagValue(inputObjectList);
            
            var typeMaxValue = GetMaxValue();

            foreach (var enumValue in inputObjectList)
            {
                var currenTObjectValueAsInt = int.Parse(enumValue.Value.ToString());

                if (currenTObjectValueAsInt < -1)
                    ThrowHelper.ThrowContainsNegativeValueException<TObject, TValue>();

                if (currenTObjectValueAsInt == inputValueAsLong)
                    return new List<TObject> { enumValue };

                if (inputValueAsLong == -1 || value.Equals(typeMaxValue))
                {
                    return inputObjectList.Where(x => long.Parse(x.Value.ToString()) > 0).ToOption();
                }

                AssignFlagStateValuesToDictionary(inputValueAsLong, currenTObjectValueAsInt, enumValue, enumFlagStateDictionary);
            }

            return inputValueAsLong > maximumAllowedValue ? default : CreateSmartObjectReturnList(enumFlagStateDictionary).ToOption();
        }

        private static long CalculateHighestAllowedFlagValue(List<TObject> inputObjectList)
        {
            return (HighestFlagValue(inputObjectList) * 2) - 1;
        }

        private static void AssignFlagStateValuesToDictionary(long inputValueAsLong, int currenTObjectValue, TObject enumValue, IDictionary<TObject, bool> enumFlagStateDictionary)
        {
            if (enumFlagStateDictionary.ContainsKey(enumValue) || currenTObjectValue == 0)
            {
                return;
            }

            bool flagState = (inputValueAsLong & currenTObjectValue) == currenTObjectValue;
            enumFlagStateDictionary.Add(enumValue, flagState);
        }

        private static IEnumerable<TObject> CreateSmartObjectReturnList(Dictionary<TObject, bool> enumFlagStateDictionary)
        {
            var outputList = new List<TObject>();

            foreach (var entry in enumFlagStateDictionary)
            {
                if (entry.Value)
                {
                    outputList.Add(entry.Key);
                }
            }

            return outputList.DefaultIfEmpty();
        }

        private static void ApplyUnsafeFlagEnumAttributeSettings(IEnumerable<TObject> list)
        {
            AllowUnsafeFlagEnumValuesAttribute attribute = (AllowUnsafeFlagEnumValuesAttribute)
                Attribute.GetCustomAttribute(typeof(TObject), typeof(AllowUnsafeFlagEnumValuesAttribute));

            if (attribute == null)
            {
                CheckEnumListForPowersOfTwo(list);
            }
        }

        private static void CheckEnumListForPowersOfTwo(IEnumerable<TObject> enumEnumerable)
        {
            var enumList = enumEnumerable.ToList();
            var enumValueList = new List<int>();
            foreach (var smartFlagEnum in enumList)
            {
                enumValueList.Add(int.Parse(smartFlagEnum.Value.ToString()));
            }
            var firstPowerOfTwoValue = 0;
            if (int.Parse(enumList[0].Value.ToString()) == 0)
            {
                enumList.RemoveAt(0);
            }

            foreach (var flagEnum in enumList)
            {
                var x = int.Parse(flagEnum.Value.ToString());
                if (IsPowerOfTwo(x))
                {
                    firstPowerOfTwoValue = x;
                    break;
                }
            }

            var highestValue = HighestFlagValue(enumList);
            var currentValue = firstPowerOfTwoValue;

            while (currentValue != highestValue)
            {
                var nextPowerOfTwoValue = currentValue * 2;
                var result = enumValueList.BinarySearch(nextPowerOfTwoValue);
                if (result < 0)
                {
                    ThrowHelper.ThrowDoesNotContainPowerOfTwoValuesException<TObject, TValue>();
                }

                currentValue = nextPowerOfTwoValue;
            }
        }

        private static bool IsPowerOfTwo(long input)
        {
            return input != 0 && ((input & (input - 1)) == 0);
        }

        private static long HighestFlagValue(IReadOnlyList<TObject> enumList)
        {
            var highestIndex = enumList.Count - 1;
            var highestValue = long.Parse(enumList.Last().Value.ToString());
            if (IsPowerOfTwo(highestValue))
            {
                return highestValue;
            }

            for (var i = highestIndex; i >= 0; i--)
            {
                var currentValue = long.Parse(enumList[i].Value.ToString());
                if (!IsPowerOfTwo(currentValue))
                {
                    continue;
                }

                highestValue = currentValue;
                break;
            }

            return highestValue;
        }

        /// <summary>
        /// Gets the largest possible value of the underlying type for the SmartFlagEnum.
        /// </summary>
        /// <exception cref="NotSupportedException">If the underlying type <see cref="TValue"/>
        /// does not define a <c>MaxValue</c> field, this exception is thrown.
        /// </exception>
        /// <returns>The value of the constant <c>MaxValue</c> field defined by the underlying type <see cref="TValue"/>.</returns>
        private static TValue GetMaxValue()
        {
            FieldInfo maxValueField = typeof(TValue).GetField("MaxValue", BindingFlags.Public
                | BindingFlags.Static);
            if (maxValueField == null)
                throw new NotSupportedException(typeof(TValue).Name);

            TValue maxValue = (TValue)maxValueField.GetValue(null);

            return maxValue;
        }
    }
}
