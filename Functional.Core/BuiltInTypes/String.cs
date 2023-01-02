﻿using System;

namespace Meigs2.Functional.BuiltInTypes;

public static class String
{
    public static Func<string, string> Trim = s => s.Trim();
    public static Func<string, string> ToLower = s => s.ToLower();
    public static Func<string, string> ToUpper = s => s.ToUpper();
}