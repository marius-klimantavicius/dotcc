//options_all:-r -x -tused
//options: --strict;cn

//     V      S
//     |\    /|
//     | \  / |
//     |  \/  |
//     |  /\  |
//     | /  \ |
//     |/    \|
//     A      B         A::i dominates both V::i and S::i
//      \    /
//       \  /
//        \/
//         E
//

struct V { int i, j; };
struct S { int i, j; };

struct A : public virtual V, public virtual S { int i; };
struct B : public virtual V, public virtual S {};

struct E : public A, public B {} e;

int f() { return e.i; }

//  S     V    S
//   \   / \   /
//    \ /   \ /
//     X     Y
//      \   /
//       \ /
//        Z

struct X : public virtual V, public S { int i; };
struct Y : public S, public virtual V { int j; };

struct Z : public X, public Y { } z;

int g() { return z.i + z.j; }

