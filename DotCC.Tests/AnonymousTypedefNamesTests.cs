using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AnonymousTypedefNamesTests
{
    [Fact]
    public void Public_name_resolution_changes_only_complete_identity_tokens()
    {
        var key=AnonymousTypedefNames.TypePrefix+new string('A',64);
        var source=key+" x; "+key+"_suffix y; // "+key+"\n/* "+key+" */ \""+key+"\" @\""+key+"\"";
        AnonymousTypedefNames.Rewrite(source,new Dictionary<string,string>{{key,"Public"}})
            .ShouldBe("Public x; "+key+"_suffix y; // "+key+"\n/* "+key+" */ \""+key+"\" @\""+key+"\"");
    }

    [Fact]
    public void Duplicate_aliases_have_no_order_dependent_public_winner()
    {
        var aliases=new Dictionary<string,string>{{"first","State"},{"second","State"},{"third","Unique"}};
        var plan=AnonymousTypedefNames.Plan(aliases,new[]{"first","second","third"},Array.Empty<string>());
        plan.Count.ShouldBe(1);plan["third"].ShouldBe("Unique");
        AnonymousTypedefNames.Plan(aliases,new[]{"first","second","third","Unique"},Array.Empty<string>()).Count.ShouldBe(0);
    }

    [Fact]
    public void Object_metadata_roundtrips_and_rejects_conflicting_aliases()
    {
        var key=AnonymousTypedefNames.TypePrefix+new string('A',64);
        var names=new Dictionary<string,string>{{key,"State"}};
        var result=new Dictionary<string,string>();
        AnonymousTypedefNames.Read(AnonymousTypedefNames.Serialize(names),result);
        result[key].ShouldBe("State");
        Should.Throw<CompileException>(()=>AnonymousTypedefNames.Read(AnonymousTypedefNames.Serialize(new Dictionary<string,string>{{key,"Other"}}),result));
    }
}
