"""Shared isolated service-test bootstrap; every unregistered callback fails fast."""
import re
def splittypes(text):
 result=[];depth=0;start=0
 for i,ch in enumerate(text):
  if ch=='<':depth+=1
  elif ch=='>':depth-=1
  elif ch==',' and depth==0:result.append(text[start:i].strip());start=i+1
 result.append(text[start:].strip());return result

def write_test_host(generated, project, register_calls="RegisterPlatform(ref table);", method_name="CreatePlatformTable"):
 sources=(generated/'Dotcc.SourceFiles.txt').read_text().splitlines()
 bodies=[]
 for name in sources:
  match=re.search(r'(?m)^(?P<indent>[ \t]*)public unsafe struct MSQUIC_HOST_TABLE\s*\{(?P<body>.*?)\n(?P=indent)\}', (generated/name).read_text(), re.S)
  if match:bodies.append(match.group('body'))
 if len(bodies)!=1:raise RuntimeError('Expected exactly one generated host table')
 body=bodies[0]
 fields=re.findall(r'public delegate\*<(.+)> (\w+);',body)
 methods=[];fills=[]
 for signature,name in fields:
  types=splittypes(signature);ret=types.pop();params=', '.join(t+' p'+str(i) for i,t in enumerate(types))
  methods.append(f'private static {ret} Missing_{name}({params}) {{ FatalInvariant("Unexpected non-platform callback: {name}");'+(' return default;' if ret!='void' else '')+' }')
  fills.append(f'if(table.{name}==null)table.{name}=&Missing_{name};')
 text=('using System;\nusing Managed.Transport;\nusing static Managed.Transport.MsQuic;\nusing static Managed.Transport.MsQuic.Libc;\nnamespace Managed.Transport.Hosting;\npublic sealed unsafe partial class MsQuicHost : IDisposable {\ninternal uint ProcessorCount => 2;\npublic MsQuicHost(){InitializeResources();}\ninternal MSQUIC_HOST_TABLE METHOD_NAME(){MSQUIC_HOST_TABLE table=default;table.Size=(uint)sizeof(MSQUIC_HOST_TABLE);table.Version=1;table.Context=ContextPointer;table.ProcessorCount=ProcessorCount;table.TotalMemory=1UL<<32;REGISTER_CALLS\n'+'\n'.join(fills)+'\nreturn table;}\ninternal void CleanupDeferred(CXPLAT_EVENTQ* queue,CXPLAT_SQE* sqe,Action done){PlatformSqeCleanupDeferred(ContextPointer,queue,sqe,done); }\npublic void Dispose(){if(OutstandingResources!=0||OutstandingPlatformAllocations!=0)throw new InvalidOperationException("Undrained platform host");ReleaseContext();}\n'+'\n'.join(methods)+'\n}\n')
 (project/'TestHost.cs').write_text(text.replace("METHOD_NAME",method_name).replace("REGISTER_CALLS",register_calls))
