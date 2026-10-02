using System;using System.Threading;using Edg.Runtime;
unsafe class Program {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  using var errno=new ThreadLocal<int>(()=>71);
  using var owner=new LibcText(x=>errno.Value=x);using var other=new LibcText(x=>errno.Value=x);
  byte* name=stackalloc byte[]{67,46,85,84,70,45,56,0};void* locale=owner.Locales.NewLocale(63,(sbyte*)name,null);
  Check(locale!=null,"locale creation");
  Check(owner.IsAlphaL('a',locale)==1 && owner.ToUpperL('a',locale)=='A' && errno.Value==71,"locale classifier");
  Check(other.IsAlphaL('a',locale)==0 && errno.Value==22,"foreign locale rejected");
  Check(other.ToUpperL('a',locale)=='a' && errno.Value==22,"foreign locale case preserves value");
  Check(owner.IsAlphaL('a',null)==0 && owner.IsAlphaL('a',(void*)(nint)(-1))==0 && errno.Value==22,"invalid locale contracts");
  sbyte* source=stackalloc sbyte[]{97,98,99,0};sbyte* destination=stackalloc sbyte[8];
  for(ulong size=0;size<8;++size){
    for(int i=0;i<8;++i)destination[i]=33;
    Check(LibcText.Strxfrm(destination,source,size)==3,"transform length");
    for(int i=0;i<8;++i)Check(destination[i]==(size>3 && i<4?source[i]:33),"musl transform writes only complete strings");
  }
  Check(owner.StrcollL(null,null,(void*)(nint)(-1))==0 && owner.StrxfrmL(null,null,ulong.MaxValue,null)==0 && errno.Value==22,"reject before dereference");
  errno.Value=71;for(int i=0;i<10000;++i)owner.IsAlphaL(i&255,locale);
  long before=GC.GetAllocatedBytesForCurrentThread();int count=0;
  for(int i=0;i<100000;++i){count+=owner.IsAlphaL(i&255,locale);LibcText.Strxfrm(destination,source,8);}
  Check(count>0 && GC.GetAllocatedBytesForCurrentThread()==before && errno.Value==71,"steady-state classification/collation allocation and errno");
  owner.Locales.FreeLocale(locale);Check(owner.IsAlphaL('a',locale)==0 && errno.Value==22,"freed locale rejected");
  Console.WriteLine("ctype locale isolation, invalid handles, transform write bounds and zero-allocation steady state passed");
 }
}
