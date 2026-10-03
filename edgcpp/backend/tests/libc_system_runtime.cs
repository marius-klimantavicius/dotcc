using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Edg.Runtime;
unsafe class Program
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static sbyte* Copy(string value){byte[] bytes=Encoding.UTF8.GetBytes(value);byte* p=(byte*)NativeMemory.Alloc((nuint)bytes.Length+1);bytes.CopyTo(new Span<byte>(p,bytes.Length));p[bytes.Length]=0;return(sbyte*)p;}
 sealed class NativeEnvironment:IDisposable {
  internal sbyte** Data;
  internal NativeEnvironment(params string[] values){Data=(sbyte**)NativeMemory.AllocZeroed((nuint)(values.Length+1),(nuint)sizeof(sbyte*));for(int i=0;i<values.Length;++i)Data[i]=Copy(values[i]);}
  public void Dispose(){for(sbyte**p=Data;*p!=null;++p)NativeMemory.Free(*p);NativeMemory.Free(Data);Data=null;}
 }
 static int Call(LibcCommand service,string? command,sbyte** env){sbyte*p=command is null?null:Copy(command);try{return service.System(p,env);}finally{NativeMemory.Free(p);}}
 static int Main(string[]args)
 {
  string root=Path.GetFullPath(args[0]),aDir=Path.Combine(root,"owner-a"),bDir=Path.Combine(root,"owner-b");Directory.CreateDirectory(aDir);Directory.CreateDirectory(bDir);
  bool windows=OperatingSystem.IsWindows();string old=Environment.CurrentDirectory;int error=71;
  Environment.SetEnvironmentVariable("EDG_HOST_LEAK","must-not-leak");
  Environment.CurrentDirectory=aDir;using var anchor=new LibcFiles(e=>error=e);Environment.CurrentDirectory=bDir;using var a=new LibcCommand(e=>error=e,anchor.WorkingDirectory);using var b=new LibcCommand(e=>error=e);
  using var aEnv=new NativeEnvironment("EDG_STATUS=7","EDG_VALUE="+(windows?"first value":"écho & \"x\" $literal"));using var bEnv=new NativeEnvironment("EDG_STATUS=19","EDG_VALUE=second value");
  try {
   Check(Call(a,null,aEnv.Data)==1&&error==71,"real processor query preserves successful errno");
   string exit=windows?"exit /b %EDG_STATUS%":"exit \"$EDG_STATUS\"";
   Check(Call(a,exit,aEnv.Data)==7&&Call(b,exit,bEnv.Data)==19,"source environment isolation/status");
   Check(Call(a,windows?"exit /b 137":"exit 137",aEnv.Data)==137,"raw exit137 remains exit137");
   string write=windows?"<nul set /p \"=%EDG_VALUE%\" > value.txt":"printf '%s' \"$EDG_VALUE\" > value.txt";
   Check(Call(a,write,aEnv.Data)==0&&Call(b,write,bEnv.Data)==0,"command working directories");
   Check(File.ReadAllText(Path.Combine(aDir,"value.txt"))==(windows?"first value":"écho & \"x\" $literal")&&File.ReadAllText(Path.Combine(bDir,"value.txt"))=="second value","captured owner cwd/UTF8/quoting");
   Check(Call(a,windows?"if defined EDG_HOST_LEAK (exit /b 9) else (exit /b 0)":"test -z \"$EDG_HOST_LEAK\"",aEnv.Data)==0,"host environment excluded");
   Check(Call(a,windows?"exit /b 0":"exit 0",null)==0,"empty environment");
   using(var missing=new LibcCommand(e=>error=e,aDir,Path.Combine(aDir,"absent-shell"))){Check(Call(missing,null,aEnv.Data)==0&&error==2,"missing processor query");Check(Call(missing,"exit 0",aEnv.Data)==-1&&error==2,"missing processor launch");}
   using(var badDirectory=new LibcCommand(e=>error=e,Path.Combine(root,"no-such-dir"))){Check(Call(badDirectory,"exit 0",aEnv.Data)==-1&&error==2,"missing working directory");}
   sbyte* invalid=stackalloc sbyte[2];invalid[0]=-1;invalid[1]=0;Check(a.System(invalid,aEnv.Data)==-1&&error==84,"strict command UTF8");
   using(var invalidEnv=new NativeEnvironment("missing-equals")){Check(Call(a,"exit 0",invalidEnv.Data)==-1&&error==22,"unrepresentable environment diagnosed");}
   using(var output=new MemoryStream())using(var files=new LibcFiles(e=>error=e,output:output,workingDirectory:aDir)){
    sbyte* path=Copy("source-stdout.txt"),mode=Copy("w");try{Check(files.Freopen(path,mode,files.Stdout)==files.Stdout,"source stdout replacement");}finally{NativeMemory.Free(path);NativeMemory.Free(mode);}
    Check(Call(a,windows?"echo EDG_HOST_STDOUT":"printf 'EDG_HOST_STDOUT\\n'",aEnv.Data)==0,"host standard handle command");
    Check(files.Fflush(files.Stdout)==0&&File.ReadAllBytes(Path.Combine(aDir,"source-stdout.txt")).Length==0&&output.Length==0,"explicit host-handle profile does not claim source FILE redirection");
   }
   a.Dispose();Check(Call(a,"exit 0",aEnv.Data)==-1&&error==9,"disposed service");
  } finally{Environment.CurrentDirectory=old;}
  Console.WriteLine("system processor query/status/environment/cwd/UTF8/errors and explicit host stdio profile passed");return 0;
 }
}
