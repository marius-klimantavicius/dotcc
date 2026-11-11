using Raw = global::Managed.Forms.Cui;

namespace Cui.Managed;

internal enum ScopeKind
{
    Id,
    Disabled,
    Accordion,
    Box,
    ButtonGroup,
    Card,
    CardStack,
    CardStackItem,
    Carousel,
    CarouselSlide,
    ContextMenu,
    DataGrid,
    Dialog,
    DialogFooter,
    Dock,
    ExpandableCard,
    HoverCard,
    Kanban,
    KanbanCard,
    KanbanColumn,
    Menu,
    Node,
    NodeEditor,
    Popover,
    Reasoning,
    Reorder,
    ReorderItem,
    Scroll,
    Sheet,
    Spotlight,
    SwipeItem,
    Table,
    Tree,
    UserMenu,
    VirtualList,
}

public sealed unsafe partial class Ui
{
    private void NativeEnd(ScopeKind kind)
    {
        switch (kind)
        {
            case ScopeKind.Id: Raw.cui_pop_id(_ctx); break;
            case ScopeKind.Disabled: Raw.cui_pop_disabled(_ctx); break;
            case ScopeKind.Accordion: Raw.cui_accordion_end(_ctx); break;
            case ScopeKind.Box: Raw.cui_box_end(_ctx); break;
            case ScopeKind.ButtonGroup: Raw.cui_button_group_end(_ctx); break;
            case ScopeKind.Card: Raw.cui_card_end(_ctx); break;
            case ScopeKind.CardStack: Raw.cui_card_stack_end(_ctx); break;
            case ScopeKind.CardStackItem: Raw.cui_card_stack_item_end(_ctx); break;
            case ScopeKind.Carousel: Raw.cui_carousel_end(_ctx); break;
            case ScopeKind.CarouselSlide: Raw.cui_carousel_slide_end(_ctx); break;
            case ScopeKind.ContextMenu: Raw.cui_context_menu_end(_ctx); break;
            case ScopeKind.DataGrid: Raw.cui_data_grid_end(_ctx); break;
            case ScopeKind.Dialog: Raw.cui_dialog_end(_ctx); break;
            case ScopeKind.DialogFooter: Raw.cui_dialog_footer_end(_ctx); break;
            case ScopeKind.Dock: Raw.cui_dock_end(_ctx); break;
            case ScopeKind.ExpandableCard: Raw.cui_expandable_card_end(_ctx); break;
            case ScopeKind.HoverCard: Raw.cui_hover_card_end(_ctx); break;
            case ScopeKind.KanbanCard: Raw.cui_kanban_card_end(_ctx); break;
            case ScopeKind.KanbanColumn: Raw.cui_kanban_column_end(_ctx); break;
            case ScopeKind.Kanban:
                Raw.cui_kanban_move move;
                Raw.cui_kanban_end(_ctx, &move);
                break;
            case ScopeKind.Menu: Raw.cui_menu_end(_ctx); break;
            case ScopeKind.NodeEditor: Raw.cui_node_editor_end(_ctx); break;
            case ScopeKind.Node: Raw.cui_node_end(_ctx); break;
            case ScopeKind.Popover: Raw.cui_popover_end(_ctx); break;
            case ScopeKind.Reasoning: Raw.cui_reasoning_end(_ctx); break;
            case ScopeKind.Reorder:
                Raw.cui_reorder_end(_ctx);
                _arraysDirty = true;
                break;
            case ScopeKind.ReorderItem: Raw.cui_reorder_item_end(_ctx); break;
            case ScopeKind.Scroll: Raw.cui_scroll_end(_ctx); break;
            case ScopeKind.Sheet: Raw.cui_sheet_end(_ctx); break;
            case ScopeKind.Spotlight: Raw.cui_spotlight_end(_ctx); break;
            case ScopeKind.SwipeItem: Raw.cui_swipe_item_end(_ctx); break;
            case ScopeKind.Table: Raw.cui_table_end(_ctx); break;
            case ScopeKind.Tree: Raw.cui_tree_end(_ctx); break;
            case ScopeKind.UserMenu: Raw.cui_user_menu_end(_ctx); break;
            case ScopeKind.VirtualList: Raw.cui_virtual_list_end(_ctx); break;
        }

        SyncState();
    }
}