struct Point {
    int x;
    int y;
    int total() const { return x + y; }
};
template<class T> T combine(T a, T b) { return a * 2 + b; }
int accumulate(const Point *point, int count);
extern int initial_bonus;
