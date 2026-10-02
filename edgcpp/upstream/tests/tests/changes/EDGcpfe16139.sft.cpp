//type:fp
//remark:[4.10.1] Invalid ranking of builtin operator called with differing enumeration types
// 5/15/15  [EDGcpfe/16139]
//
// Invalid ranking of builtin operator called with differing enumeration types
//
// In somewhat unusual cases where certain binary operators were applied to
// an expression of enumeration type and another expression of a class type
// convertible to a different enumeration type, the front end incorrectly
// ranked the conversion of the class type to an arithmetic type.  This could
// lead to overload resolution errors.
//
// Previously, when considering the conversion of w to an arithmetic type for
// the builtin operator !=, the front end incorrectly ranked that conversion
// as if it were a conversion to type E.  That made the conversion of w worse
// for the builtin operator that for the operator declared at (1), which in
// turn caused an ambiguity between those two candidate operators.  Now, the
// conversion of w is weighed equally for both operators, and since the
// promotion of e for the builtin operator is a better conversion than its
// conversion to I, the builtin operator is now selected.
enum E { e };
enum F { f };

struct WF { operator F() const; } w;
struct I { I(int); };

bool operator!=(int, I const&);  // (1)

bool b = w != e;  // Previously ambiguous; now the builtin operator
                  // is selected.
