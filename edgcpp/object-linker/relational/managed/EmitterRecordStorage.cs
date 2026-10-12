namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private static string CompleteRecordAlignment(ProgramType record)
    {
        if(record.Size is not (>0 and <=int.MaxValue) || record.Alignment is not (1 or 2 or 4 or 8) ||
            record.Size<record.Alignment || record.Size%record.Alignment!=0)
            throw new InvalidDataException("Complete virtual-base record storage requires recorded size and supported alignment 1/2/4/8: type "+record.Id+" "+record.Name);
        return record.Alignment switch {1=>"byte",2=>"ushort",4=>"uint",8=>"ulong",_=>throw new InvalidDataException("Recorded alignment missing")};
    }

    private void WriteCompleteRecordStorage(Unit unit,ProgramType record,string owner,string alignment)
    {
        var members=new Names();members.Reserve(owner);
        foreach(var field in unit.Fields.Values.Where(field=>field.Owner==record.Id))
        {
            string name=fieldNames[(unit,field.Id)];members.Reserve(name);
            if(field.BitWidth is not null || IsZeroExtentArray(unit,field.Type))members.Reserve("get_"+name.TrimStart('@'));
            if(field.BitWidth is not null)members.Reserve("set_"+name.TrimStart('@'));
        }
        string storage=members.Allocate("__record_bytes","complete_record_bytes");
        string anchor=members.Allocate("__record_alignment","record_alignment");
        Line(2,"// Complete recorded object bytes retain base/vptr state during value copies.");
        Line(2,"[global::System.Runtime.InteropServices.FieldOffset(0)]");
        Line(2,"private fixed byte "+storage+"["+record.Size+"];");
        Line(2,"[global::System.Runtime.InteropServices.FieldOffset(0)]");
        Line(2,"private "+alignment+" "+anchor+";");
    }
}
