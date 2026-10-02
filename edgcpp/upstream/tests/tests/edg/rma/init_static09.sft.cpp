//options_all:-r -x -tused
//options: --strict;cn

static int i1;
static int i2 = 0;
static int i3 = int();

static const int ci1;
static const int ci2 = 0;
static const int ci3 = int();

struct S { int i; };
static S s1;
static S s2 = { 0 };
static S s3 = S();

struct T { const int i; };
static T t1;
static T t2 = { 0 };
static T t3 = T();


