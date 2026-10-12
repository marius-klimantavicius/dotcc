using System.Collections;
using System.Reflection;
var assembly=Assembly.LoadFrom(Path.GetFullPath(args[0]));
var unitType=assembly.GetType("DotObj.Relational.Unit",true)!;
var units=args.Skip(1).Select(path=>Activator.CreateInstance(unitType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Path.GetFullPath(path)},null)!).ToArray();
foreach(string family in new[]{"Types","Symbols","Fields"})
{
 long rows=0;
 foreach(var unit in units)
 {
  var catalog=unitType.GetProperty(family)!.GetValue(unit)!;
  var values=(IEnumerable)catalog.GetType().GetProperty("Values")!.GetValue(catalog)!;
  var item=catalog.GetType().GetProperty("Item")!;
  int expected=Convert.ToInt32(catalog.GetType().GetProperty("Count")!.GetValue(catalog));
  int actual=0;
  foreach(var row in values)
  {
   long id=Convert.ToInt64(row!.GetType().GetProperty("Id")!.GetValue(row));
   if(!Equals(row,item.GetValue(catalog,new object[]{id})))throw new Exception(family+" point/stream row mismatch "+id);
   ++actual;
  }
  if(actual!=expected)throw new Exception(family+" count mismatch");rows+=actual;
 }
 int cached=units.Sum(unit=>{var catalog=unitType.GetProperty(family)!.GetValue(unit)!;return Convert.ToInt32(catalog.GetType().GetProperty("CachedRows")!.GetValue(catalog));});
 if(cached>4096)throw new Exception(family+" cache exceeded4096: "+cached);
 Console.WriteLine($"PASS {family}: {rows} point/stream rows, {cached} resident cache rows across {units.Length} distinct objects");
}
var pool=assembly.GetType("DotObj.Relational.ObjectDatabase");
if(pool is not null)
{
 foreach(var unit in units)
 {
  var types=unitType.GetProperty("Types")!.GetValue(unit)!;
  foreach(var row in ((IEnumerable)types.GetType().GetProperty("Values")!.GetValue(types)!).Cast<object>().Take(3))
   foreach(var other in units)
   {
    var symbols=unitType.GetMethod("SymbolsForParent")!.Invoke(other,new object[]{0L})!;
    foreach(var ignored in (IEnumerable)symbols){}
   }
 }
 int peak=Convert.ToInt32(pool.GetProperty("PeakConnections")!.GetValue(null));
 if(peak>4)throw new Exception("connection pool exceeded4");
 long evictions=Convert.ToInt64(pool.GetProperty("Evictions")!.GetValue(null));
 if(units.Length>4 && evictions==0)throw new Exception("five-input test did not exercise eviction");
 Console.WriteLine($"PASS paged nested cross-input queries: peak connections {peak}, evictions {evictions}");
 if(units.Length>4)
 {
  var pinned=new List<IDisposable>();
  try
  {
   foreach(var unit in units.Take(4))
   {
    var database=unitType.GetField("database",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(unit)!;
    pinned.Add((IDisposable)database.GetType().GetMethod("Prepare")!.Invoke(database,new object[]{"SELECT 1"})!);
   }
   var fifth=unitType.GetField("database",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(units[4])!;
   try
   {
    ((IDisposable)fifth.GetType().GetMethod("Prepare")!.Invoke(fifth,new object[]{"SELECT 1"})!).Dispose();
    throw new Exception("all-pinned pool unexpectedly grew");
   }
   catch(TargetInvocationException error) when(error.InnerException is InvalidOperationException inner && inner.Message.Contains("pinned"))
   {Console.WriteLine("PASS all-pinned fifth handle rejects without growing pool");}
  }
  finally {foreach(var statement in pinned)statement.Dispose();}
 }

}
foreach(var unit in units)((IDisposable)unit).Dispose();
if(pool is not null && Convert.ToInt32(pool.GetProperty("OpenConnections")!.GetValue(null))!=0)throw new Exception("disposed units retained connections");
foreach(string family in new[]{"Types","Symbols","Fields"})
 if(units.Any(unit=>{var catalog=unitType.GetProperty(family)!.GetValue(unit)!;return Convert.ToInt32(catalog.GetType().GetProperty("CachedRows")!.GetValue(catalog))!=0;}))throw new Exception("disposed owner cache retained");
Console.WriteLine("PASS disposed units release every cached row");

if(pool is not null)
{
 string copy=Path.Combine(Environment.CurrentDirectory,"immutable-witness.dotobj");
 File.Copy(Path.GetFullPath(args[1]),copy);
 var clone=Activator.CreateInstance(pool,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{copy},null)!;
 var extra=new List<IDisposable>();
 try
 {
  ((IDisposable)pool.GetMethod("Prepare")!.Invoke(clone,new object[]{"SELECT 1"})!).Dispose();
  foreach(string source in args.Skip(1).Take(5))
  {
   var database=Activator.CreateInstance(pool,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Path.GetFullPath(source)},null)!;
   extra.Add((IDisposable)database);
   ((IDisposable)pool.GetMethod("Prepare")!.Invoke(database,new object[]{"SELECT 1"})!).Dispose();
  }
  File.SetLastWriteTimeUtc(copy,File.GetLastWriteTimeUtc(copy).AddSeconds(3));
  try
  {
   ((IDisposable)pool.GetMethod("Prepare")!.Invoke(clone,new object[]{"SELECT 1"})!).Dispose();
   throw new Exception("changed immutable object was reopened");
  }
  catch(TargetInvocationException error) when(error.InnerException is IOException io && io.Message.Contains("changed"))
  {Console.WriteLine("PASS changed immutable input rejects on reopening");}
 }
 finally {((IDisposable)clone).Dispose();foreach(var database in extra)database.Dispose();}
}
