using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        // A retained string/aggregate constant denotes exact target storage. Its
        // ordinary expression reads that storage; selected array decay/address
        // operations still execute through their captured typed plan.
        private bool TryPlanStoredConstant(ConstantOperation operation, out ExpressionPlan plan)
        {
            plan = null!;
            if (operation.Constant.Value is not (StringValue or AggregateValue)) return false;
            var constantType = operation.Constant.Type ?? throw Limit("A stored constant has no retained type.", "constant");
            if (!Unwrap(constantType).Equals(Unwrap(operation.Type)) || constantType.Size != operation.Type.Size ||
                constantType.Alignment != operation.Type.Alignment)
                throw Limit("A constant occurrence changes the retained stored representation.", "constant");
            if (operation.Constant.Value is StringValue && (Unwrap(operation.Type) is not ArrayType || operation.ValueCategory.Name != "lvalue") ||
                operation.Constant.Value is AggregateValue && (Unwrap(operation.Type) is not RecordType || operation.ValueCategory.Name != "prvalue"))
                throw Limit("A stored constant has an unexercised retained category/type combination.", "constant");
            _ = TryStoredConstant(operation.Constant, out var rendered);
            var place = new LValuePlan(operation.Type, LValueKind.Slot, new CsLeaf(CsType.Value(operation.Type), rendered, false));
            if (operation.ValueCategory.Name == "prvalue")
            {
                var prefix = new List<CsPlanStatement>();
                var local = DeclareTemporary(CsType.Value(operation.Type), new CsLoad(place), prefix);
                var materialized = new LValuePlan(operation.Type, LValueKind.Slot, local);
                plan = new(operation.Type, operation.ValueCategory, prefix, new CsLoad(materialized), ExpressionEffects.ReadStorage, materialized);
            }
            else plan = new(operation.Type, operation.ValueCategory, [], new CsLoad(place), ExpressionEffects.ReadStorage, place);
            return true;
        }

        private bool TryStoredConstant(Constant constant, out string value)
        {
            value = "";
            if (constant.Value is not (StringValue or AggregateValue)) return false;
            var type = constant.Type ?? throw Limit("A stored constant has no retained type.", "constant");
            SemanticEntity entity = constant;
            if (constant.AssociatedStorageVariable is { } variable)
            {
                _ = TryAdmitSelectedConstantStorageCompanion(variable);
                entity = CanonicalVariable(variable);
                if (!storage.TryGetValue(entity, out var item) || item.Type.Size != type.Size)
                    throw Limit("A constant's associated storage has no same-sized selected allocation.", "constant");
            }
            else AddStorage(constant, type, constant, $"literal_{storageOrder.Count + 1}");
            value = $"(*(({TypeName(type)}*){OwnedName(entity)}))";
            return true;
        }
    }
}
