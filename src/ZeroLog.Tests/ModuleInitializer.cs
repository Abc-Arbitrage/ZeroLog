﻿using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace ZeroLog.Tests;

#pragma warning disable CA2255

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DiffRunner.Disabled = true;
        VerifyDiffPlex.Initialize();
    }
}
