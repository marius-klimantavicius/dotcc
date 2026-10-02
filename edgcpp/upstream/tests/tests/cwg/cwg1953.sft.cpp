//options_all:--c++20
void g(int i) 
{
    union U { int x, y; } u;
    (u.x = 1, 0) + (u.y = 2, 0);   // undefined behavior
}

//cwg: 1953
//title: Data races and common initial sequence
//meeting: Wroclaw 11/24
//edg_status: Passes
