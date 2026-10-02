//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

class Operand;
class Operand_ptrList;  // Try commenting out this line
typedef Operand*        Operand_ptr;
class Operand_ptrLink
{
        friend Operand_ptrList;
        Operand_ptr             an_entry;
};

class Operand_ptrList
{
        Operand_ptrLink*        a_last;
public:
        Operand_ptr last()      { return a_last ? a_last->an_entry : 0; }
};

