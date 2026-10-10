namespace MemberSourceTypes {
struct Owner {
    int value;
    int read(int) const & noexcept;
    int write(int) &&;
};
struct OtherOwner { int value; };
struct Nontrivial {
    int value;
    Nontrivial(const Nontrivial&);
    ~Nontrivial();
};
using DataMember = int Owner::*;
using OtherDataMember = int OtherOwner::*;
using ReadMember = int (Owner::*)(int) const & noexcept;
using WriteMember = int (Owner::*)(int) &&;
using UnmatchedMember = long (Owner::*)(short) volatile && noexcept;
using SourceOnlyMember = long (Owner::*)(short, Nontrivial, ...) volatile && noexcept;
using QualifiedDataMember = DataMember const;
struct Catalog {
    DataMember data;
    OtherDataMember other_data;
    ReadMember read;
    WriteMember write;
    UnmatchedMember unmatched;
    SourceOnlyMember source_only;
    QualifiedDataMember qualified_data;
};
#ifdef SOURCE_TYPE_MEMBER_DECLARATIONS_ONLY
int inspect(const Catalog* value);
#else
int inspect(const Catalog* value) { return value == nullptr ? 0 : 1; }
#endif
}
