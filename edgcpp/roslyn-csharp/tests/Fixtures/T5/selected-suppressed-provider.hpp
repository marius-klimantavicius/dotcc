#ifndef SELECTED_SUPPRESSED_PROVIDER_HPP
#define SELECTED_SUPPRESSED_PROVIDER_HPP

extern "C" {
extern int selected_suppressed_live;
extern int selected_suppressed_constructed;
extern int selected_suppressed_destroyed;
void selected_suppressed_note(int value) noexcept;
}

template <class Value> struct SelectedProviderCell {
    Value value;

    explicit SelectedProviderCell(Value initial) noexcept : value(initial) {
        ++selected_suppressed_live;
        ++selected_suppressed_constructed;
        selected_suppressed_note(100 + value);
    }

    ~SelectedProviderCell() noexcept {
        selected_suppressed_note(300 + value);
        --selected_suppressed_live;
        ++selected_suppressed_destroyed;
    }
};

extern template struct SelectedProviderCell<int>;

extern "C" {
int fixture() noexcept;
int lifetime_value_count() noexcept;
int lifetime_value(int index) noexcept;
int lifetime_count() noexcept;
int lifetime_event(int index) noexcept;
int lifetime_overflow() noexcept;
int lifetime_live() noexcept;
int lifetime_constructed() noexcept;
int lifetime_destroyed() noexcept;
}

#endif
