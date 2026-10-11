struct Guard {
    int id;
    explicit Guard(int value);
    ~Guard();
};
extern long long trace;
extern int alive;
extern int constructions;
extern int completions;
extern bool throw_holder;
void reset();
int lookup(bool found);
int same_scope(bool early);
