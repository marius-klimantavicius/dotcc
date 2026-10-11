namespace DotObj.Relational;

internal sealed partial class Unit
{
    private bool bitfieldFactsRead;
    private long? targetCharBit;
    private bool? targetLittleEndian;
    private readonly Dictionary<long,bool?> bitfieldSigned=[];

    public long? TargetCharBit {get {ReadBitfieldFacts();return targetCharBit;}}
    public bool? TargetLittleEndian {get {ReadBitfieldFacts();return targetLittleEndian;}}
    public bool? BitfieldIsSigned(long field) {ReadBitfieldFacts();return bitfieldSigned.GetValueOrDefault(field);}

    private void ReadBitfieldFacts()
    {
        if(bitfieldFactsRead)return;
        bool hasCharBit,hasEndian,hasSigned;
        using(var q=database.Prepare("SELECT 1 FROM pragma_table_info('object_info') WHERE name='char_bit'"))hasCharBit=q.Read();
        using(var q=database.Prepare("SELECT 1 FROM pragma_table_info('object_info') WHERE name='little_endian'"))hasEndian=q.Read();
        using(var q=database.Prepare("SELECT 1 FROM pragma_table_info('fields') WHERE name='bit_is_signed'"))hasSigned=q.Read();
        using(var q=database.Prepare("SELECT "+(hasCharBit?"char_bit":"NULL")+","+(hasEndian?"little_endian":"NULL")+" FROM object_info WHERE singleton=1"))
            if(q.Read())
            {
                targetCharBit=q.OptionalInteger(0);
                targetLittleEndian=q.OptionalInteger(1) is long endian?endian!=0:null;
            }
        if(hasSigned)
            using(var q=database.Prepare("SELECT id,bit_is_signed FROM fields WHERE bit_width IS NOT NULL"))
                while(q.Read())bitfieldSigned.Add(q.Integer(0),q.OptionalInteger(1) is long signed?signed!=0:null);
        bitfieldFactsRead=true;
    }
}
