namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly HashSet<(Unit Unit,long Id)> usedLiterals=[];
    private readonly Dictionary<(Unit Unit,long Id),string> literalNames=[];
    private readonly Dictionary<(Unit Unit,long Id),(LiteralObject Literal,string Name)> literalStorage=[];

    private (Unit Unit,LiteralObject Literal) SelectedLiteral(Unit unit,long id)
    {
        var literal=unit.Literals[id];
        if(literal.Symbol is not long symbol)return(unit,literal);
        var selected=program.Resolve(unit,symbol);
        var definition=selected.Unit.Literals.Values.FirstOrDefault(value=>value.Symbol==selected.Id);
        if(definition is null)throw new InvalidDataException("Exporter defect: selected literal storage missing for "+selected.Symbol.LinkName);
        return(selected.Unit,definition);
    }

    private void UseLiteral(Unit unit,long id)
    {
        if(!usedLiterals.Add((unit,id)))return;
        var selected=SelectedLiteral(unit,id);
        UseType(selected.Unit,selected.Literal.Type);
    }

    private void PrepareLiteralStorage()
    {
        foreach(var source in usedLiterals)
        {
            var selected=SelectedLiteral(source.Unit,source.Id);
            var key=(selected.Unit,selected.Literal.Id);
            if(!literalStorage.TryGetValue(key,out var storage))
            {
                storage=(selected.Literal,names.Allocate("__literal_"+selected.Literal.Id,"literal_storage"));
                literalStorage.Add(key,storage);
            }
            literalNames.Add(source,storage.Name);
        }
    }

    private void WriteLiteralStorage()
    {
        foreach(var pair in literalStorage)
        {
            var unit=pair.Key.Unit;var literal=pair.Value.Literal;string name=pair.Value.Name;
            var type=unit.Original(literal.Type);
            if(type.Alignment is not >0 || literal.ByteCount<=0)
                throw new InvalidDataException("Literal storage requires recorded positive extent and alignment");
            long bytes=checked(literal.ByteCount+type.Alignment.Value-1);
            if(bytes>int.MaxValue)throw new InvalidDataException("Pinned literal exceeds supported managed storage");
            string backing=names.Allocate(name+"_storage","literal_bytes"),create=names.Allocate(name+"_initialize","initialize_literal");
            Line(1,"private static readonly byte[] "+backing+" = "+create+"();");
            Line(1,"private static byte* "+name+" => (byte*)"+AlignedArrayAddress(backing,type.Alignment.Value)+";");
            Line(1,"private static byte[] "+create+"()");Line(1,"{");
            Line(2,"var storage = global::System.GC.AllocateArray<byte>("+bytes+", pinned: true);");
            Line(2,"byte* destination = (byte*)"+AlignedArrayAddress("storage",type.Alignment.Value)+";");
            // Literal bytes are target storage, not CLR strings. Stream them
            // directly so no TU-sized payload remains in the reader catalog.
            Line(2,"global::System.ReadOnlySpan<byte> data =");Line(2,"[");
            var row=new List<string>(16);long offset=0;
            foreach(var value in unit.LiteralBytes(literal.Id))
            {
                if(value.Offset!=offset++)throw new InvalidDataException("Exporter defect: literal byte offsets are not contiguous");
                row.Add("0x"+value.Value.ToString("x2",System.Globalization.CultureInfo.InvariantCulture));
                if(row.Count==16){Line(3,string.Join(", ",row)+",");row.Clear();}
            }
            if(offset!=literal.ByteCount)throw new InvalidDataException("Exporter defect: literal byte count does not match its rows");
            if(row.Count!=0)Line(3,string.Join(", ",row)+",");
            Line(2,"];");
            Line(2,"data.CopyTo(new global::System.Span<byte>(destination, "+literal.ByteCount+"));");
            Line(2,"return storage;");Line(1,"}");
        }
    }

    private sealed partial class FunctionWriter
    {
        private void WriteLiteralInitializer(Node node,string destination,int indent)
        {
            var action=unit.LiteralInitializers[node.Id];
            long literal=node.LiteralObject??throw unit.Missing(node,"literal initializer target missing");
            var payload=emitter.SelectedLiteral(unit,literal).Literal;
            if(action.CopyBytes<0 || action.CopyBytes>payload.ByteCount || action.ZeroBytes<0)
                throw unit.Missing(node,"exporter defect: literal initialization extent exceeds its selected payload");
            if(action.ZeroBytes>0)
                Line(indent,"global::System.Runtime.InteropServices.NativeMemory.Clear("+destination+", "+action.ZeroBytes+"U);");
            if(action.CopyBytes>0)
                Line(indent,"global::System.Runtime.InteropServices.NativeMemory.Copy("+emitter.literalNames[(unit,literal)]+", "+destination+", "+action.CopyBytes+"U);");
        }

        private string LiteralAddress(Node node)
        {
            long literal=node.LiteralObject??throw unit.Missing(node,"literal address target missing");
            long offset=node.LiteralByteOffset??throw unit.Missing(node,"literal address offset missing");
            string address=emitter.literalNames[(unit,literal)];
            if(offset!=0)address="("+address+" + ("+offset+"))";
            return "(("+NodeType(node)+")"+address+")";
        }
    }
}
