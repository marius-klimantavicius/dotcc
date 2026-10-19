using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedefCommaShadowTests
{
    [Theory]
    [InlineData("int f(void) { int a, digit, b=0; digit=3; return digit+b; } digit after;")]
    [InlineData("int g(int,int); int f(void) { int a=g(1,sizeof(digit)), digit=(a,3); return digit; } digit after;")]
    [InlineData("int f(int first, int digit) { return first+digit; } digit after;")]
    [InlineData("int f(void) { { int a=0, digit=2; a+=digit; } digit restored=3; return restored; }")]
    [InlineData("struct S { int a, digit; digit next; }; digit after;")]
    [InlineData("int f(void) { int values[2]={sizeof(digit),sizeof(digit)}, digit=3; return values[0]+digit; }")]
    [InlineData("int f(void) { int n=0; for(int a=0,digit=1;a<2;++a) {n+=digit;} digit restored=2; return n+restored; }")]
    [InlineData("int f(void) { int n=0; for(int a=0,digit=1;a<2;++a) n+=digit; digit restored=2; return n+restored; }")]
    [InlineData("int f(void) { int n=0; for(int a=0,digit=1;a<2;++a) if(a) n+=digit; else n+=2; digit restored=2; return n+restored; }")]
    [InlineData("int f(void) { int n=0; for(int a=0,digit=1;a<2;++a) for(int j=0;j<2;++j) n+=digit; digit restored=2; return n+restored; }")]
    [InlineData("int f(void) { for(int a=0,digit=1;a<2;++a) do { const char *text=\"; for }\"; digit+=text[0]; } while(0); digit restored=2; return restored; }")]
    [InlineData("typedef struct S { int a, digit; digit next; } S; digit after;")]
    public void Declaration_commas_preserve_typedef_shadow_scope(string source)
    {
        var path=Path.Combine(Path.GetTempPath(), $"dotcc-comma-shadow-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,"typedef unsigned long digit; "+source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
