namespace DotObj;

internal sealed partial class ObjectFile
{
    private void ReadRoutineExceptionResolutions()
    {
        var origins = new Dictionary<long, string>();
        using (var source = database.Prepare("SELECT type_id,exception_policy_origin FROM source_routine_types ORDER BY type_id"))
            while (source.Read())
            {
                string origin = source.Text(1);
                if (origin is not ("captured" or "generated-resolution" or "implicit-destructor-resolution") || !SourceRoutines.ContainsKey(source.Integer(0)))
                    throw new InvalidDataException("Invalid source routine exception policy origin");
                origins.Add(source.Integer(0), origin);
            }
        var observed = new HashSet<long>();
        using var rows = database.Prepare("""
            SELECT source_routine_type_id,final_source_routine_type_id,source_function_entity_id,
                   initial_policy,resolved_policy,
                   initial_present,initial_indeterminate,initial_arg_cached,initial_copy_from_prototype,initial_is_noexcept,initial_throw_any,
                   final_present,final_indeterminate,final_arg_cached,final_copy_from_prototype,final_is_noexcept,final_throw_any,
                   special_member_kind,is_defaulted,is_compiler_generated,is_inheriting_constructor,capture_proof
            FROM source_routine_exception_resolutions ORDER BY source_routine_type_id
            """);
        while (rows.Read())
        {
            long source = rows.Integer(0), final = rows.Integer(1);
            long? function = rows.OptionalInteger(2);
            string initialPolicy = rows.Text(3), resolvedPolicy = rows.Text(4), kind = rows.Text(17);
            bool Flag(int index)
            {
                long value = rows.Integer(index);
                if (value is not (0 or 1)) throw new InvalidDataException("Invalid exception specification flag");
                return value == 1;
            }
            string Policy(int offset)
            {
                bool present = Flag(offset), indeterminate = Flag(offset + 1), cached = Flag(offset + 2),
                    prototype = Flag(offset + 3), noExcept = Flag(offset + 4), any = Flag(offset + 5);
                if (!present && (indeterminate || cached || prototype || noExcept || any))
                    throw new InvalidDataException("Absent original exception specification carries flags");
                return !present || any ? "unwind" : noExcept ? "terminate" :
                    indeterminate || cached || prototype ? "unsupported-dynamic" : "dynamic";
            }
            bool defaulted = Flag(18), generated = Flag(19), inherited = Flag(20);
            bool implicitDestructor = origins.GetValueOrDefault(source) == "implicit-destructor-resolution";
            string proof = implicitDestructor ? "observed-implicit-destructor-exception-specification" : "observed-generated-exception-specification";
            if (SourceLanguage != "c++" || origins.GetValueOrDefault(source) is not ("generated-resolution" or "implicit-destructor-resolution") ||
                !observed.Add(source) || !SourceRoutines.TryGetValue(source, out var originalRoutine) ||
                !SourceRoutines.TryGetValue(final, out var finalRoutine) ||
                Policy(5) != initialPolicy || Policy(11) != resolvedPolicy ||
                resolvedPolicy is not ("unwind" or "terminate" or "dynamic") || Flag(12) || Flag(13) || Flag(14) ||
                originalRoutine.ExceptionPolicy != resolvedPolicy || finalRoutine.ExceptionPolicy != resolvedPolicy ||
                (implicitDestructor ? kind != "destructor" || defaulted || generated || inherited : !defaulted && !generated && !inherited) || inherited && kind != "constructor" ||
                kind is not ("constructor" or "destructor" or "assignment" or "equality" or "inequality" or
                    "less" or "greater" or "less-equal" or "greater-equal" or "three-way-comparison") ||
                kind is "constructor" or "destructor" or "assignment" && originalRoutine.ObjectTypeId is null ||
                rows.Text(21) != proof)
                throw new InvalidDataException("Generated routine exception specification lacks its exact original and resolved policy");
            // A stock replacement type may change exception policy, never the
            // already-selected source result, receiver or explicit parameters.
            if (source != final && SourceRoutineSignature(originalRoutine, 0) != SourceRoutineSignature(finalRoutine, 0))
                throw new InvalidDataException("Generated exception specification changed the original routine signature");
            if (function is long entity &&
                (!EntitySourceRoutines.TryGetValue(entity, out var selected) || selected.TypeId != source && selected.TypeId != final ||
                 selected.ExceptionPolicy != resolvedPolicy ||
                 SourceFunctionKinds.GetValueOrDefault(entity) != (kind is "constructor" or "destructor" ? kind : "ordinary") ||
                 RoutineTypes.Signature(EntityRoutine(entity)).ExceptionPolicy != resolvedPolicy))
                throw new InvalidDataException("Generated exception specification source entity disagrees with its selected type and ABI");
        }
        if (!observed.SetEquals(origins.Where(pair => pair.Value is "generated-resolution" or "implicit-destructor-resolution").Select(pair => pair.Key)))
            throw new InvalidDataException("Missing generated routine exception specification provenance");
    }
}
