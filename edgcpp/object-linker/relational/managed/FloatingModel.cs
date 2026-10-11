namespace DotObj.Relational;

internal sealed record FloatingFormat(long Type,string Format,long Precision,long MinimumExponent,long MaximumExponent);
internal sealed record FloatingValue(long Id,long Type,string Bits);

internal sealed partial class Unit
{
    public bool HasFloatingValues {get;private set;}
    public Dictionary<long,FloatingFormat> FloatingFormats {get;}=[];
    public Dictionary<long,FloatingValue> FloatingValues=>Body.FloatingValues;

    private void ReadFloatingFormats()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='floating_types'"))
            if(q.Read())
            {
                using var formats=database.Prepare("SELECT type_id,format,precision_bits,min_exponent,max_exponent FROM floating_types ORDER BY type_id");
                while(formats.Read())FloatingFormats.Add(formats.Integer(0),new(formats.Integer(0),formats.Text(1),formats.Integer(2),formats.Integer(3),formats.Integer(4)));
            }
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='floating_values'"))
            HasFloatingValues=q.Read();
    }

    private void ReadFloatingValues(long? function)
    {
        if(!HasFloatingValues)return;
        using var q=database.Prepare("SELECT DISTINCT v.id,v.type_id,v.bits_hex FROM nodes n JOIN floating_values v ON v.id=n.floating_value_id WHERE "+FunctionFilter(function,"n.")+" ORDER BY v.id");
        while(q.Read())FloatingValues.TryAdd(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Text(2)));
    }
}
