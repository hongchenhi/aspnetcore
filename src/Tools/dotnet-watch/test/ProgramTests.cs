// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Tools.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Watcher.Tools.Tests
{
    public class ProgramTests : IDisposable
    {
        private readonly TemporaryDirectory _tempDir;
        private readonly TestConsole _console;

        public ProgramTests(ITestOutputHelper output)
        {
            _tempDir = new TemporaryDirectory();
            _console = new TestConsole(output);
        }

        [Fact]
        [QuarantinedTest("https://github.com/dotnet/aspnetcore/issues/23394")] // Failure on OSX https://dev.azure.com/dnceng/public/_build/results?buildId=706059&view=ms.vss-test-web.build-test-results-tab
        public async Task ConsoleCancelKey()
        {
            _tempDir
                .WithCSharpProject("testproj")
                .WithTargetFrameworks("net6.0")
                .Dir()
                .WithFile("Program.cs")
                .Create();

            using (var app = new Program(_console, _tempDir.Root))
            {
                var run = app.RunAsync(new[] { "run" });

                await _console.CancelKeyPressSubscribed.TimeoutAfter(TimeSpan.FromSeconds(30));
                _console.ConsoleCancelKey();

                var exitCode = await run.TimeoutAfter(TimeSpan.FromSeconds(30));

                Assert.Contains("Shutdown requested. Press Ctrl+C again to force exit.", _console.GetOutput());
                Assert.Equal(0, exitCode);
            }
        }

        public void Dispose()
        {
            _tempDir.Dispose();
        }
    }
}
