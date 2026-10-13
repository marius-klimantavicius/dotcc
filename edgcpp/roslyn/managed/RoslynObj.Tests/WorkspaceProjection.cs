using RoslynObj;

internal static class WorkspaceProjection
{
    internal static void Verify(string nativePath,string sourcePath,string temporaryRoot,Action<bool,string> check)
    {
        var copied=Path.Combine(temporaryRoot,"same-artifact.roslynobj");File.Copy(nativePath,copied);
        using var workspace=RoslynWorkspace.Open(new[]{nativePath,nativePath,copied,sourcePath},new(){PageSize=3,CachedRowCount=32,CachedPageCount=2});
        var objects=workspace.Objects;var units=workspace.TranslationUnits;
        check(objects.Count==4&&units.Count==4&&!ReferenceEquals(objects[0],objects[1])&&!ReferenceEquals(objects[0],objects[2]),"A workspace must keep separate snapshot owners for repeated paths and copied artifacts without deduplicating its collections.");
        check(objects[0].Manifest.ArtifactIdentity==objects[1].Manifest.ArtifactIdentity&&objects[0].Manifest.ArtifactIdentity==objects[2].Manifest.ArtifactIdentity&&objects[0].Manifest.ArtifactIdentity!=objects[3].Manifest.ArtifactIdentity,"Reopening and copying one immutable artifact must preserve identity; an independent compilation must retain its different identity.");
        var add=units.Select(u=>u.SemanticModel.FindSymbols("add").OfType<FunctionSymbol>().Single()).ToArray();
        check(add[0]==add[1]&&add[0]==add[2]&&add[0].GetHashCode()==add[1].GetHashCode()&&add[0].GetHashCode()==add[2].GetHashCode()&&!ReferenceEquals(add[0],add[1]),"The same captured function reopened through distinct owners must compare equal with stable hashes while retaining separate objects.");
        check(add[0].Name==add[3].Name&&add[0]!=add[3]&&new HashSet<SemanticEntity>(add).Count==2&&units[0]==units[1]&&units[0]==units[2]&&units[0]!=units[3],"Same-named entities and translation units from independent artifacts must remain unequal; a workspace performs no linking or name-based merge.");
        var calls=units.Select(u=>u.Functions.Single(f=>f.Name=="main").Body!.DescendantOperations().OfType<CallOperation>().Single(c=>c.TargetMethod?.Name=="add")).ToArray();
        check(units[0].SemanticModel.GetSymbol(calls[2])==add[0]&&units[0].SemanticModel.GetReferences(add[2]).Count==units[0].SemanticModel.GetReferences(add[0]).Count,"A semantic model may accept entities from another open copy of the same artifact through their exact captured identity.");
        var model=units[0].SemanticModel;
        check(Rejected(()=>_=model.GetSymbol(calls[3]))&&Rejected(()=>_=model.GetType(calls[3]))&&Rejected(()=>_=model.GetOperation(calls[3]))&&Rejected(()=>_=model.GetReferences(add[3]))&&Rejected(()=>_=model.GetAvailability(add[3])),"Every entity-taking semantic query must reject a different artifact before navigation, even when its names match.");
        objects[0].Dispose();
        check(Disposed(()=>_=units[0].Functions.Count)&&Disposed(()=>_=add[0].Signature)&&add[0]==add[2]&&add[0].GetHashCode()==add[2].GetHashCode(),"Disposing one workspace owner must reject its lazy reads while immutable captured identity remains usable.");
        check(units.Skip(1).All(u=>u.Functions.Single(f=>f.Name=="main").Body!.DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod?.Name=="add")==1),"Independent owners, including an equal copied artifact, must continue normal navigation after another owner is disposed.");
        workspace.Dispose();workspace.Dispose();
        check(units.All(u=>Disposed(()=>_=u.Functions.Count))&&workspace.TranslationUnits.SequenceEqual(units)&&objects.Count==4,"Workspace disposal must close every retained owner idempotently while detached owner and translation-unit handles retain their immutable identities.");
    }

    private static bool Rejected(Action read) { try {read();return false;}catch(ArgumentException){return true;} }
    private static bool Disposed(Action read) { try {read();return false;}catch(ObjectDisposedException){return true;} }
}
