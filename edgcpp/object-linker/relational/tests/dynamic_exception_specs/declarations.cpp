extern "C" int imported_empty(int) throw();
extern int imported_types(int) throw(int, float);
extern int terminate_decl(int) noexcept;
extern int unwind_decl(int);
extern int false_noexcept_decl(int) noexcept(false);

struct MemberOwner {
    int field;
    int method(int) const;
};
typedef MemberOwner MemberAlias;
extern void data_member_decl(int MemberAlias::*);
extern void function_member_decl(int (MemberAlias::*)(int) const);

int main() { return 0; }
