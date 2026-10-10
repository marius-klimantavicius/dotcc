int constructor_contract_should_throw();
struct ContractVirtualState {
    int value;
    ContractVirtualState() : value(41) {
        if (constructor_contract_should_throw()) throw 17;
    }
};
struct ConstructorContract : virtual ContractVirtualState {
    ConstructorContract() noexcept;
    int own;
};
ConstructorContract *construct_contract(void *);
