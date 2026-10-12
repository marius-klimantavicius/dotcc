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
foreach(var unit in units)((IDisposable)unit).Dispose();
foreach(string family in new[]{"Types","Symbols","Fields"})
 if(units.Any(unit=>{var catalog=unitType.GetProperty(family)!.GetValue(unit)!;return Convert.ToInt32(catalog.GetType().GetProperty("CachedRows")!.GetValue(catalog))!=0;}))throw new Exception("disposed owner cache retained");
Console.WriteLine("PASS disposed units release every cached row");
