namespace DotObj.Relational;

internal sealed partial class Emitter
{
    // A fixed integer table can retain its source values directly. The
    // recorded actions still determine every slot, including zeroing/repeats.
    // Other arrays retain the ordinary action writer without a new boundary.
    private string? WriteConstantArrayData(Unit unit,Node node,ProgramType destination,string owner)
    {
        if(node.Kind!="array-initializer" || node.Type is not long arrayId ||
            node.Initialization is not null || node.Lifetime is not null ||
            !unit.ArrayInitializers.TryGetValue(node.Id,out var initialization))return null;
        var array=unit.Original(arrayId);
        // Span lengths and managed array indices use Int32; there is no
        // separate source-data cutoff for otherwise eligible tables.
        if(array.Kind!="array" || array.Id!=destination.Id || array.Count is not (>0 and <=int.MaxValue) || array.Element is not long elementId ||
            unit.IsVolatile(arrayId) || unit.IsVolatile(elementId))return null;
        var element=unit.Original(elementId);
        long width=element.Kind switch
        {
            "i8" or "u8"=>1,"i16" or "u16"=>2,"i32" or "u32"=>4,"i64" or "u64"=>8,_=>0
        };
        if(width==0 || element.Size!=width || array.Size!=array.Count*width)return null;
        var values=new string?[(int)array.Count.Value];
        string type=Type(unit,elementId);
        if(initialization.ZeroInitialize)Array.Fill(values,"(("+type+")0)");
        var writer=new FunctionWriter(this,unit,null);
        foreach(var action in unit.ArrayElements.GetValueOrDefault(node.Id,[]))
        {
            if(action.Index<0 || action.Repeat<=0 || action.Index>array.Count-action.Repeat)
                throw unit.Missing(node,"exporter defect: array initializer range exceeds recorded storage");
            var value=unit.Nodes[action.Value];
            if(value.Kind!="constant" || value.Constant is null || value.Type is not long valueType ||
                value.Initialization is not null || value.Lifetime is not null || value.Symbol is not null ||
                value.EnumValue is not null || value.FloatingValue is not null || value.LiteralObject is not null ||
                value.ValueNormalization is not null || unit.IsVolatile(valueType))return null;
            var selected=unit.Original(value.ExecutionType??valueType);
            if(selected.Kind!=element.Kind || selected.Size!=width)return null;
            string literal=writer.Expression(value);
            for(long index=action.Index;index<action.Index+action.Repeat;++index)values[(int)index]=literal;
        }
        // A missing slot without an explicit zero action is not reconstructed.
        if(values.Any(value=>value is null))return null;
        string data=names.AllocateProperty("__"+owner.TrimStart('@')+"_data","constant_array_data");
        Line(1,"private static global::System.ReadOnlySpan<"+type+"> "+data+" =>");
        Line(1,"[");
        for(int index=0;index<values.Length;index+=16)
            Line(2,string.Join(", ",values.Skip(index).Take(16))+",");
        Line(1,"];");
        return data;
    }
}
