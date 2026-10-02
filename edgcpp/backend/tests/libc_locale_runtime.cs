using System;using System.Runtime.InteropServices;using System.Text;using System.Threading;using Edg.Runtime;
unsafe class Program {
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static string Text(sbyte* value)=>Marshal.PtrToStringUTF8((nint)value)!;
 static void Main(){
  using var error=new ThreadLocal<int>(()=>71);using var a=new LibcText(x=>error.Value=x);using var b=new LibcText(x=>error.Value=x);
  byte* c=stackalloc byte[]{67,0};byte* utf=stackalloc byte[]{67,46,85,84,70,45,56,0};byte* bad=stackalloc byte[]{110,111,0};
  void* global=(void*)(nint)(-1);Check(a.Locales.UseLocale(null)==global,"initial global selection");
  void* locale=a.Locales.NewLocale(1,(sbyte*)utf,null);Check(locale!=null && a.MbCurMax()==1,"creation does not select");
  Check(a.Locales.UseLocale(locale)==global && a.MbCurMax()==4,"thread select");
  Check(a.Locales.UseLocale(null)==locale && b.MbCurMax()==1,"owner separation");
  byte* buffer=stackalloc byte[4];Check(a.Wctomb((sbyte*)buffer,0x20ac)==3 && buffer[0]==0xe2,"selected UTF8 conversion");
  Check(a.SetLocale(6,(sbyte*)c)!=null && a.MbCurMax()==4,"thread overrides owner global");
  a.Locales.FreeLocale(locale);Check(error.Value==16 && a.MbCurMax()==4,"selected free busy");
  Check(a.Locales.NewLocale(1,(sbyte*)c,locale)==null && error.Value==16,"selected mutation busy");
  void* copy=a.Locales.DupLocale(locale);Check(copy!=null && copy!=locale,"duplicate snapshot");
  Check(a.Locales.UseLocale(global)==locale && a.MbCurMax()==1,"restore global");
  Check(a.Locales.NewLocale(1,(sbyte*)c,locale)==locale,"transactional base mutation");
  Check(a.Locales.UseLocale(copy)==global && a.MbCurMax()==4,"duplicate independent");a.Locales.UseLocale(global);
  Check(a.Locales.NewLocale(63,(sbyte*)bad,null)==null && error.Value==2,"unknown name ENOENT");
  Check(a.Locales.NewLocale(64,(sbyte*)c,null)==null && error.Value==22,"unknown category EINVAL");
  Check(b.Locales.UseLocale(copy)==null && error.Value==22,"foreign handle");
  a.Locales.FreeLocale(locale);a.Locales.FreeLocale(locale);Check(error.Value==22,"double free");
  Check(a.Locales.Validate(locale)==0 && a.Locales.Validate(copy)==1,"stale validation");
  a.SetLocale(0,(sbyte*)utf);string mixed=Text(a.SetLocale(6,null));Check(mixed=="C.UTF-8;C;C;C;C;C","mixed category query");
  byte[] saved=Encoding.ASCII.GetBytes(mixed+"\0");a.SetLocale(6,(sbyte*)c);fixed(byte* p=saved)Check(Text(a.SetLocale(6,(sbyte*)p))==mixed,"roundtrip mixed query");
  void* snapshot=a.Locales.DupLocale(global);a.SetLocale(6,(sbyte*)c);a.Locales.UseLocale(snapshot);Check(a.MbCurMax()==4,"global duplicate frozen");a.Locales.ReleaseThread();Check(a.MbCurMax()==1,"thread release resets");
  Exception? failure=null;Thread t=new(()=>{try {Check(a.MbCurMax()==1,"thread initial global");a.Locales.UseLocale(copy);Check(a.MbCurMax()==4,"worker UTF8");a.ReleaseThread();Check(a.MbCurMax()==1,"worker release");}catch(Exception x){failure=x;}});t.Start();t.Join();if(failure!=null)throw failure;
  Check(a.MbCurMax()==1,"worker isolation");
  byte* convention=(byte*)a.Locales.LocaleConv();Check(Text(*(sbyte**)convention)==".","decimal point");for(int i=1;i<10;++i)Check(Text(((sbyte**)convention)[i])=="","C monetary strings");for(int i=80;i<94;++i)Check(convention[i]==127,"C monetary CHAR_MAX");
  sbyte* message=stackalloc sbyte[64];error.Value=71;
  Check(a.StrerrorR(22,message,64)==0 && Text(message)=="Invalid argument" && error.Value==71,"POSIX strerror_r success");
  message[3]=99;Check(a.StrerrorR(22,message,3)==34 && message[0]=='I' && message[1]=='n' && message[2]==0 && message[3]==99 && error.Value==71,"POSIX strerror_r truncation");
  message[0]=99;Check(a.StrerrorR(22,message,1)==34 && message[0]==0 && a.StrerrorR(22,null,0)==34 && error.Value==71,"POSIX strerror_r zero/one bound");
  Check(a.StrerrorR(99999,message,64)==0 && Text(message)=="No error information","POSIX strerror_r fallback");
  nint pinned=(nint)convention;GC.Collect(2,GCCollectionMode.Forced,true,true);Check((nint)a.Locales.LocaleConv()==pinned && Text(*(sbyte**)convention)==".","lconv pinning");Check(b.Locales.LocaleConv()!=convention,"owner lconv");
  a.Locales.FreeLocale(copy);a.Locales.FreeLocale(snapshot);a.ReleaseThread();a.Dispose();bool disposed=false;try {a.Locales.UseLocale(null);}catch(ObjectDisposedException){disposed=true;}Check(disposed,"dispose lifecycle");
  Console.WriteLine("locale names/categories, owner/thread handles, errno/lifetime, lconv and forced-GC pinning passed");
 }
}
