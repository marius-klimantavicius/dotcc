int destructor_contract_should_throw();
struct DestructorVirtualState {
    int value;
    DestructorVirtualState() noexcept : value(41) {}
    ~DestructorVirtualState() noexcept(false) {
        if (destructor_contract_should_throw()) throw 19;
    }
};
struct DestructorContract : virtual DestructorVirtualState {
    DestructorContract() noexcept : own(1) {}
    ~DestructorContract() noexcept;
    int own;
};
void destroy_contract(DestructorContract *);
void *operator new(__SIZE_TYPE__, void *) noexcept;
void operator delete(void *, void *) noexcept;
