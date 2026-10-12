using size_type = decltype(sizeof(0));
struct Root { int value; };
struct Padding { long long padding; };
struct Middle : Padding, Root { int middle; };
struct PublicIndirect : Middle { int last; };
struct PrivateBase : private Root {
    void set(int input) { value = input; }
    int get() const { return value; }
};
struct BranchOne : Root {};
struct BranchTwo : Root {};
struct Ambiguous : BranchOne, BranchTwo {};

struct Shared { int value; };
struct Left : virtual Shared { int left; };
struct Right : virtual Shared { int right; };
struct Diamond : Left, Right {
    int last;
    Diamond() noexcept;
    Diamond(const Diamond&) noexcept;
};
int public_indirect();
int private_base();
int ambiguous_base();
int virtual_diamond(const Diamond&, size_type expected_offset);
