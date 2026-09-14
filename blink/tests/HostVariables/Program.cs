using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("private variable assertion failed");}
static void Reject(Dictionary<string,string> values){try{using var owner=new HostVariables(values);throw new Exception("invalid environment accepted");}catch(ArgumentException){}}
static Dictionary<string,string> Values()=>new(){["BLINK_TEST_VALUE"]="private-value",["BLINK_TEST_EMPTY"]="",["BLINK_TEST_UTF8"]="é/λ"};
using(var empty=new HostVariables()){
  Check(empty.AllocatedBytes==0);Blink.BindHostVariables(empty);Check(Blink.VariablesDefaultEmpty()==0);Blink.UnbindHostVariables();
}
var dictionary=Values();var owner=new HostVariables(dictionary);int bytes=owner.AllocatedBytes;
Check(bytes==14+1+6); // value bytes including terminators: 14, 1, 6
Blink.BindHostVariables(owner);dictionary["BLINK_TEST_VALUE"]="changed";dictionary.Clear();
Check(Blink.VariablesProbe()==0 && Blink.VariablesPrivate()==0);
unsafe{void* saved=Blink.VariablesSave();Check(saved!=null);GC.Collect(2,GCCollectionMode.Forced,true,true);Check(Blink.VariablesSaved(saved,0)==0);}
Blink.UnbindHostVariables();Check(Blink.VariablesUnavailable(19)==0);owner.Dispose();owner.Dispose();Check(owner.AllocatedBytes==0);
Blink.BindHostVariables(owner);Check(Blink.VariablesUnavailable(9)==0);Blink.UnbindHostVariables();
Reject(new(){[""]="x"});Reject(new(){["A=B"]="x"});Reject(new(){["A\0B"]="x"});Reject(new(){["A"]="x\0y"});
Reject(new(){["A"]="\ud800"});Reject(new(){["\ud800"]="x"});Reject(new(){[new string('x',1025)]=""});
Reject(new(){[new string('é',513)]=""});Reject(new(){["A"]=new string('x',32768)});
var entries=new Dictionary<string,string>();for(int i=0;i<128;++i)entries.Add("K"+i,"");
using(var maximum=new HostVariables(entries)){Check(maximum.AllocatedBytes==128);}
entries.Add("overflow","");Reject(entries);
using(var maximum=new HostVariables(new Dictionary<string,string>{["A"]=new string('x',32767)})){Check(maximum.AllocatedBytes==32768);}
using(var maximumName=new HostVariables(new Dictionary<string,string>{[new string('x',1024)]=""})){Check(maximumName.Lookup(new string('x',1024)).Value!=0);}
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(bool second){var worker=new Thread(()=>{
  var values=Values();if(second)values["BLINK_TEST_VALUE"]="other-value";
  using var local=new HostVariables(values);
  try{
    Blink.BindHostVariables(local);
    unsafe{void* saved=Blink.VariablesSave();barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();Check(Blink.VariablesSaved(saved,second?1:0)==0);}
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostVariables();}
});worker.Start();return worker;}
var a=Start(false);var b=Start(true);a.Join();b.Join();if(failure!=null)throw failure;
