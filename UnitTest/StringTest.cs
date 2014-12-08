﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class StringTest
    {
        public void StringConstructorTest()
        {
            var value = "0x01".GetValue<string>();
            var character = (char)0.GetValue<char>();
            var size = 8.GetValue<int>();
            var str = new string(character, size - value.Length) + value;
            Console.WriteLine(str);
        }

        public void CharConvertTest()
        {
            char[] chars = new char[4];

            chars[0] = 'X';
            chars[1] = '\x0058';
            chars[2] = (char)88;
            chars[3] = '\u0058';

            foreach (var c in chars)
            {
                Console.WriteLine(c + " ");
            }
        }
    }

    public static class ObjectExtension
    {
        public static T GetValue<T>(this object value)
        {
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                return (T)Enum.Parse(typeof(T), Convert.ToString(value));
            }
            if (value is IConvertible)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return (T)value;
        }
    }
}
