struct V181e
{
    virtual ~V181e() = default; // Without this, or without virtual, no problem.
};

struct V3726;

template <typename V365e>
struct V3691
{
    enum class V1082 {
        V3693 = 0,
        V3694,
        V3695
    };

    V3691(const V3726 &V3699);
    void V10fd();
    void V1165();

protected:
    V1082 V1086 = V1082::V3693;
    V181e V36a0{*this};

    void V36a1();
    virtual void V365a(); // Without virtual, no problem.
};

struct V3726
{
    void V10fd();
    void V1165();

protected:
    V3691<int> V372d[1] = {{*this}};
    void V3733();
};

template <typename V365e>
void V3691<V365e>::V36a1()
{
    V1086 = V1082::V3694;
}

template <typename V365e>
void V3691<V365e>::V10fd()
{
    V1086 = V1082::V3693;
}

template <typename V365e>
void V3691<V365e>::V1165()
{
    V1086 = V1082::V3695;
    V36a1();
}

// Without this function, no problem.
template <typename V365e>
void V3691<V365e>::V365a()
{
    switch (V1086)
    {
    case V1082::V3694:
        V1086 = V1082::V3695;
        break;

    case V1082::V3695:
        break;

    default:
        break;
    }
}
