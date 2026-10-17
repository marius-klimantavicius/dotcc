#ifndef ROSLYNOBJ_COMPOSED_SCALAR_CALL_CHAIN_HPP
#define ROSLYNOBJ_COMPOSED_SCALAR_CALL_CHAIN_HPP

// The native oracle constructs these inputs. Captured execution only borrows
// their storage; generated validation supplies independent aligned layout bytes.
enum class ScalarCode : unsigned char {
    missing = 0,
    first = 5,
    second = 201,
    changed_first = 128,
    changed_second = 255
};

struct ScalarInputPrefix {
    long long first;
    long long second;
};

struct ScalarInput : ScalarInputPrefix {
    virtual int unused() const = 0;
    ScalarCode kind;
    long long tail[3];

    ScalarCode read() const;
};

using ScalarElement = ScalarInput;
using StoredScalarElement = ScalarElement;

struct ScalarStorage;

struct ScalarAccess {
    ScalarElement* base_pointer() const noexcept;
    ScalarElement* operator->() const noexcept;
};

struct ScalarRefCount {
    ScalarInput* cookie;
};

struct ScalarStorage : ScalarAccess {
    StoredScalarElement* pointer;
    ScalarRefCount refcount;

    StoredScalarElement* get() const noexcept;
    explicit operator bool() const noexcept;
};

struct ScalarDerivedStorage : ScalarStorage {};
using ScalarStorageAlias = ScalarDerivedStorage;

struct ScalarProxy {
    ScalarStorageAlias internal;
    bool use_direct;

    explicit operator bool() const noexcept;
    ScalarInput* operator->() const;
};

using ScalarProxyAlias = ScalarProxy;

// This unrelated polymorphic field is never accessed by captured execution.
// Its virtual definition and all construction occur in the native harness.
struct ScalarOwnerPrefix {
    virtual int unused() const;
    long long first;
    long long second;
};

struct ScalarOwner {
    ScalarOwnerPrefix prefix;
    mutable ScalarProxyAlias payload;
    long long suffix;

    const ScalarProxyAlias& view() const;
    ScalarCode composed() const;
};

extern "C" int fixture(const ScalarOwner* first, const ScalarOwner* second,
    ScalarInput* first_input, ScalarInput* second_input);
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
// This slot is the shared scalar protocol, not C++ lifetime evidence.
extern "C" int lifetime_live();
extern "C" int lifetime_event(int index);
extern "C" int lifetime_value(int index);

#endif
