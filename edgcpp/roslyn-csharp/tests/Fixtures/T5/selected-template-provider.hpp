#ifndef SELECTED_TEMPLATE_PROVIDER_HPP
#define SELECTED_TEMPLATE_PROVIDER_HPP

extern "C" {
extern int selected_template_live;
extern int selected_template_constructed;
extern int selected_template_destroyed;
void selected_template_note(int value) noexcept;
}

template <class Value> struct SelectedTemplateCell {
    Value value;

    explicit SelectedTemplateCell(Value initial) noexcept;
    ~SelectedTemplateCell() noexcept;
};

template <class Value>
SelectedTemplateCell<Value>::SelectedTemplateCell(Value initial) noexcept : value(initial) {
    ++selected_template_live;
    ++selected_template_constructed;
    selected_template_note(100 + value);
}

template <class Value>
SelectedTemplateCell<Value>::~SelectedTemplateCell() noexcept {
    selected_template_note(300 + value);
    --selected_template_live;
    ++selected_template_destroyed;
}

extern template struct SelectedTemplateCell<int>;

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
