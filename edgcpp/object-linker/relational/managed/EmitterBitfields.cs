namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed record BitfieldSlice(string Backing,int LocalShift,int Width,int ValueShift);
    private sealed record BitfieldPlan(Field Field,bool Signed,List<BitfieldSlice> Slices);

    private Dictionary<long,BitfieldPlan> WriteBitfieldBacking(Unit unit,ProgramType record)
    {
        var plans=new Dictionary<long,BitfieldPlan>();
        var fields=unit.Fields.Values.Where(field=>field.Owner==record.Id && field.BitWidth is >0).OrderBy(field=>field.Ordinal).ToArray();
        if(fields.Length==0)return plans;
        if(unit.TargetCharBit is null || unit.TargetLittleEndian is null)
            throw new InvalidDataException("Bitfield generation requires recorded target char_bit and little_endian facts: "+record.Name);
        if(unit.TargetCharBit!=8 || unit.TargetLittleEndian!=true)
            throw new InvalidDataException("Bitfield generation currently supports recorded 8-bit little-endian targets: "+record.Name);
        var members=new Names();members.Reserve(typeNames[(unit,record.Id)]);
        foreach(var field in unit.Fields.Values.Where(field=>field.Owner==record.Id))
        {
            string member=fieldNames[(unit,field.Id)];members.Reserve(member);
            if(field.BitWidth is not null){members.Reserve("get_"+member.TrimStart('@'));members.Reserve("set_"+member.TrimStart('@'));}
        }
        var backing=new SortedDictionary<long,string>();
        foreach(var field in fields)
        {
            string description=record.Name+"."+field.Name;
            bool signed=unit.BitfieldIsSigned(field.Id)??throw new InvalidDataException("Bitfield generation requires recorded bit_is_signed: "+description);
            if(unit.IsVolatile(field.Type))throw new InvalidDataException("Volatile bitfield access emission is not yet implemented: "+description);
            var type=unit.Original(field.Type);
            if(type.Kind is not ("bool" or "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "enum") ||
               field.BitWidth is not long recordedWidth || recordedWidth is <=0 or >64 ||
               field.BitOffset is not long recordedOffset || recordedOffset is <0 or >=8)
                throw new InvalidDataException("Bitfield generation requires a recorded integer/bool/enum field of at most 64 bits and an in-byte offset: "+description);
            if(type.Size is not (>0 and <=8) || recordedWidth>type.Size*8 || type.Kind=="bool" && recordedWidth!=1)
                throw new InvalidDataException("Bitfield generation does not yet support width beyond recorded base value storage: "+description);
            int width=checked((int)recordedWidth),offset=checked((int)recordedOffset);
            int bytes=(offset+width+7)/8;
            if(field.Offset<0 || checked(field.Offset+bytes)>record.Size)
                throw new InvalidDataException("Recorded bitfield byte extent exceeds its record: "+description);
            var slices=new List<BitfieldSlice>();
            for(int index=0;index<bytes;++index)
            {
                long address=checked(field.Offset+index);
                if(!backing.TryGetValue(address,out string? name))
                {
                    name=members.Allocate("__bits_"+address,"bitfield_byte");backing.Add(address,name);
                }
                int first=Math.Max(offset,index*8),end=Math.Min(offset+width,index*8+8);
                slices.Add(new(name,first-index*8,end-first,first-offset));
            }
            plans.Add(field.Id,new(field,signed,slices));
        }
        // Each backing occupies exactly one recorded byte, even when several
        // properties share it. No source base-type allocation unit is loaded.
        foreach(var item in backing)
        {
            Line(2,"[global::System.Runtime.InteropServices.FieldOffset("+item.Key+")]");
            Line(2,"private byte "+item.Value+";");
        }
        return plans;
    }

    private void WriteBitfieldProperty(Unit unit,BitfieldPlan plan)
    {
        Field field=plan.Field;
        string type=Type(unit,field.Type),property=fieldNames[(unit,field.Id)];
        int width=checked((int)field.BitWidth!.Value);
        var terms=new List<string>();
        foreach(var slice in plan.Slices)
        {
            int mask=(1<<slice.Width)-1;
            string part="((ulong)"+slice.Backing+(slice.LocalShift==0?"":" >> "+slice.LocalShift)+")";
            part="("+part+" & "+mask+"UL)";
            if(slice.ValueShift!=0)part="("+part+" << "+slice.ValueShift+")";
            terms.Add(part);
        }
        string bits="("+string.Join(" | ",terms)+")";
        string read;
        if(type=="bool")read=bits+" != 0";
        else
        {
            if(plan.Signed && width<64)bits="(unchecked((long)("+bits+" << "+(64-width)+")) >> "+(64-width)+")";
            read="unchecked(("+type+")"+bits+")";
        }
        Line(2,"public "+type+" "+property);Line(2,"{");Line(3,"get => "+read+";");
        Line(3,"set");Line(3,"{");
        string value=type=="bool"?"(value ? 1UL : 0UL)":"unchecked((ulong)value)";
        foreach(var slice in plan.Slices)
        {
            int mask=((1<<slice.Width)-1)<<slice.LocalShift;
            string incoming=slice.ValueShift==0?value:"("+value+" >> "+slice.ValueShift+")";
            if(slice.LocalShift!=0)incoming="("+incoming+" << "+slice.LocalShift+")";
            Line(4,slice.Backing+" = unchecked((byte)(((ulong)"+slice.Backing+" & "+(255^mask)+"UL) | ("+incoming+" & "+mask+"UL)));");
        }
        Line(3,"}");Line(2,"}");
    }

    private static Field? SelectedBitfield(Unit unit,Node node)
    {
        if(node.Kind!="operation")return null;
        if(node.Operator is "lvalue-adjust" or "lvalue-cast")
            return unit.Children(node,"operand").FirstOrDefault() is Node operand?SelectedBitfield(unit,operand):null;
        if(node.Operator is not ("dot-field" or "points-to-field"))return null;
        long? id=node.Field??unit.Children(node,"operand").Skip(1).FirstOrDefault()?.Field;
        return id is long field && unit.Fields[field].BitWidth is not null?unit.Fields[field]:null;
    }

    private sealed partial class FunctionWriter
    {
        private readonly HashSet<long> discardedBitfieldAssignments=[];

        private bool WriteBitfieldAssignmentStatement(Node node,int indent)
        {
            if(node.Kind!="operation" || node.Operator!="assign")return false;
            var operands=unit.Children(node,"operand").ToArray();
            if(operands.Length!=2 || SelectedBitfield(unit,operands[0]) is null)return false;
            // Only this discarded root can use C# property assignment, whose
            // expression result precedes bitfield truncation. Its recorded
            // operand-order expansion still saves an effectful RHS first.
            discardedBitfieldAssignments.Add(node.Id);
            try {Line(indent,StatementExpression(node,ExpandedExpression(node,indent))+";");}
            finally {discardedBitfieldAssignments.Remove(node.Id);}
            return true;
        }

        private void RequireBitfieldOperationSupport(Node node,Node[] operands)
        {
            if(operands.Length==0 || SelectedBitfield(unit,operands[0]) is null)return;
            if(node.Operator=="assign" && !discardedBitfieldAssignments.Contains(node.Id))
                throw unit.Missing(node,"Value-producing bitfield assignment emission is not yet implemented");
            if(IsCompound(node.Operator) || node.Operator is "pre-increment" or "post-increment" or "pre-decrement" or "post-decrement")
                throw unit.Missing(node,"Bitfield compound/increment result emission is not yet implemented");
            if(node.Operator is "address-of" or "reference-to")
                throw unit.Missing(node,"Bitfield address emission is not supported");
        }
    }
}
