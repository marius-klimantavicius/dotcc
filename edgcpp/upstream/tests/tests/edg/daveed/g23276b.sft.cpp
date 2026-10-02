//remark:C-mode conditional operator and null-pointer constants
//options:--c17;fp

_Static_assert( _Generic(1 ? (void*)(long)0 : (int*)0, int*: 1, void*: 0),
                "Unexpected");
