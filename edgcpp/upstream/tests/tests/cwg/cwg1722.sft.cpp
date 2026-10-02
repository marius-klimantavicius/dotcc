//type:fp
//options_all:--c++17 -tused -A
auto GL = [](auto a) { return a; };
int (*GL_int)(int) = GL; // OK: through conversion function template
static_assert(noexcept(GL_int));

//cwg: 1722
//title: Should lambda to function pointer conversion function be noexcept?
//meeting: Kona 10/15
//edg_status: EDGcpfe/24306
