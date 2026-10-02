//remark:Capture changes for P3579
//options:--c++20;fp

void g() {
int x;
[=]<decltype(x)* p>  // error: unqualified-id names a local entity that would be captured by copy
                      // but not from the function parameter scope
    (decltype(x) y)   // error: unqualified-id names a local entity that would be captured by copy
                      // from within the function parameter scope, but it's in the parameter-declaration-clause
    -> decltype((x))  // ok: unqualified-id names a local entity that would be captured by copy
                      // in the function parameter scope, transformed into class access. Yields int const&.
{
        return x;     // ok: lvalue of type int const
};

int j;
[=](){
    []<decltype(j)* q> // ok: the innermost lambda that would capture j by copy is the outer lambda
                       // and we are in the outer's lambda's function parameter scope, this is int*
    (decltype((j)) w)  // ok: as above, 'w' is a parameter of type int const&
    {};
};

}
