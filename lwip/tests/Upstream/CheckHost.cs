using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Managed.Networking.Tests;

/// <summary>Test-only Check registration/assertion services; protocol and fixtures remain C.</summary>
public static unsafe class CheckHost
{
    public sealed class AssertionFailure(string message) : Exception(message);
    private sealed class TestCase(string name)
    {
        public string Name = name;
        public nint Setup, Teardown;
        public readonly List<(nint Function, string Name, int Start, int End)> Tests = [];
    }
    private sealed class TestSuite(string name)
    {
        public string Name = name;
        public readonly List<TestCase> Cases = [];
    }
    private sealed class Runner
    {
        public readonly List<TestSuite> Suites = [];
        public int Failed, Passed;
        public string? Xml;
    }
    private sealed class Registry
    {
        public readonly Dictionary<nint, object> Objects = [];
        public nint Next = 1;
        public uint RandomState = 1;
        public void* Add(object value) { nint id = Next++; Objects.Add(id, value); return (void*)id; }
        public T Get<T>(void* handle) where T : class => (T)Objects[(nint)handle];
    }
    private static readonly ConditionalWeakTable<Lwip, Registry> Registries = new();
    private static Registry State(Lwip owner) => Registries.GetOrCreateValue(owner);
    private static string Text(byte* value) => Marshal.PtrToStringUTF8((nint)value) ?? "";
    public static uint Random(Lwip owner) { var s=State(owner); uint x=s.RandomState; x ^= x<<13; x ^= x>>17; x ^= x<<5; return s.RandomState=x; }
    public static void Assert(Lwip owner, byte* message, byte* file, int line) => throw new AssertionFailure($"{Text(file)}:{line}: {Text(message)}");
    public static void Failure(Lwip owner, byte* expression, byte* file, int line, byte* message) => throw new AssertionFailure($"{Text(file)}:{line}: {Text(expression)}: {Text(message)}");
    public static void* SuiteCreate(Lwip owner, byte* name) => State(owner).Add(new TestSuite(Text(name)));
    public static void* CaseCreate(Lwip owner, byte* name) => State(owner).Add(new TestCase(Text(name)));
    public static void SuiteAddCase(Lwip owner, void* suite, void* testCase) => State(owner).Get<TestSuite>(suite).Cases.Add(State(owner).Get<TestCase>(testCase));
    public static void CaseFixture(Lwip owner, void* testCase, delegate*<Lwip,void> setup, delegate*<Lwip,void> teardown)
    {
        var c=State(owner).Get<TestCase>(testCase); c.Setup=(nint)setup; c.Teardown=(nint)teardown;
    }
    public static void CaseAdd(Lwip owner, void* testCase, delegate*<Lwip,int,void> test, byte* name, int signal, int allowedExit, int start, int end)
    {
        if(signal!=0 || allowedExit!=0 || start<0 || end<start) throw new NotSupportedException("Unsupported Check test registration");
        State(owner).Get<TestCase>(testCase).Tests.Add(((nint)test,Text(name),start,end));
    }
    public static void* RunnerCreate(Lwip owner, void* suite)
    {
        var runner=new Runner(); runner.Suites.Add(State(owner).Get<TestSuite>(suite)); return State(owner).Add(runner);
    }
    public static void RunnerAddSuite(Lwip owner, void* runner, void* suite) => State(owner).Get<Runner>(runner).Suites.Add(State(owner).Get<TestSuite>(suite));
    public static void RunnerXml(Lwip owner, void* runner, byte* path) => State(owner).Get<Runner>(runner).Xml=Text(path);
    public static void RunnerFork(Lwip owner, void* runner, int status) { if(status!=0)throw new NotSupportedException("Managed Check runner supports CK_NOFORK only"); }
    public static void RunnerRun(Lwip owner, void* handle, int verbosity)
    {
        var runner=State(owner).Get<Runner>(handle); runner.Failed=runner.Passed=0;
        var results=new XElement("testsuites");
        foreach(var suite in runner.Suites)
        {
            var suiteXml=new XElement("testsuite",new XAttribute("name",suite.Name));results.Add(suiteXml);
            foreach(var c in suite.Cases) foreach(var test in c.Tests) for(int i=test.Start;i<test.End;i++)
            {
                Exception? failure=null; bool setupCompleted=false;
                try
                {
                    if(c.Setup!=0)((delegate*<Lwip,void>)c.Setup)(owner);
                    setupCompleted=true;
                    ((delegate*<Lwip,int,void>)test.Function)(owner,i);
                }
                catch(Exception error){failure=error;}
                finally
                {
                    try{if(setupCompleted && c.Teardown!=0)((delegate*<Lwip,void>)c.Teardown)(owner);}
                    catch(Exception error){failure ??= error;}
                }
                var result=new XElement("testcase",new XAttribute("name",test.Name),new XAttribute("iteration",i));suiteXml.Add(result);
                if(failure==null){runner.Passed++;Console.WriteLine($"PASS {suite.Name}/{test.Name}");}
                else{runner.Failed++;result.Add(new XElement("failure",failure.ToString()));Console.WriteLine($"FAIL {suite.Name}/{test.Name}: {failure}");}
            }
        }
        results.SetAttributeValue("passed",runner.Passed);results.SetAttributeValue("failed",runner.Failed);
        if(runner.Xml is not null)new XDocument(results).Save(runner.Xml);
        Console.WriteLine($"CHECK passed={runner.Passed} failed={runner.Failed}");
    }
    public static int RunnerFailed(Lwip owner, void* runner) => State(owner).Get<Runner>(runner).Failed;
    public static void RunnerFree(Lwip owner, void* runner) => State(owner).Objects.Clear();
}
