//type:fp
//options:--c++11:--c++17:--c++26
//options_all:-A -W

int main() noexcept             // OK
{}

//cwg: 3060
//title: Change in behavior for noexcept main
//meeting: Kona 11/25
//edg_status: EDGcpfe/28543
