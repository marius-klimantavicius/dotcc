using System.Text.Json;

namespace DotObj;

internal sealed partial class ObjectFile
{
    private void ReadAndVerifyMacroProvenance()
    {
        static bool Name(string value) => value.Length != 0 && !value.Any(ch => char.IsWhiteSpace(ch) || char.IsControl(ch));
        static void Location(string origin, long? file, long line, long column)
        {
            if (origin == "source")
            {
                if (file is null or <= 0 || line <= 0 || column <= 0)
                    throw new InvalidDataException("Source macro provenance requires a source file and positive location");
            }
            else if (origin is not ("command-line" or "predefined") || file is not null || line != 0 || column != 0)
                throw new InvalidDataException("Invalid macro origin or synthetic location");
        }
        long nextDefinition = 1;
        using (var query = database.Prepare("SELECT id,name,form,variadic,dynamic,definition_text,origin,source_file_id,line,column FROM macro_definitions ORDER BY id"))
            while (query.Read())
            {
                string name = query.Text(1), form = query.Text(2), text = query.Text(5), origin = query.Text(6);
                bool dynamic = query.Integer(4) == 1, variadic = query.Integer(3) == 1;
                if (query.Integer(0) != nextDefinition++ || !Name(name) || form is not ("object" or "function") ||
                    query.Integer(3) is not (0 or 1) || query.Integer(4) is not (0 or 1))
                    throw new InvalidDataException("Invalid macro definition identity, name, form or flags");
                if (dynamic ? origin != "predefined" || text.Length != 0 :
                    !text.StartsWith("#define " + name + (form == "object" ? " " : "("), StringComparison.Ordinal) || text.Contains('\0'))
                    throw new InvalidDataException("Invalid canonical macro definition text or dynamic builtin");
                if (variadic && form != "function") throw new InvalidDataException("Object macro cannot be variadic");
                Location(origin, query.OptionalInteger(7), query.Integer(8), query.Integer(9));
            }

        // Parameter checks stream one definition at a time. Canonical replacement
        // text is preserved, not reparsed as C/C++ or converted into C# here.
        long owner = 0, ordinal = 0;
        var parameterNames = new HashSet<string>(StringComparer.Ordinal);
        using (var query = database.Prepare("SELECT p.definition_id,p.ordinal,p.name,d.form,d.dynamic,d.origin FROM macro_parameters p JOIN macro_definitions d ON d.id=p.definition_id ORDER BY p.definition_id,p.ordinal"))
            while (query.Read())
            {
                long definition = query.Integer(0);
                if (definition != owner) { owner = definition; ordinal = 0; parameterNames.Clear(); }
                string name = query.Text(2);
                if (query.Integer(1) != ordinal++ || query.Text(3) != "function" ||
                    !(Name(name) || name.Length == 0 && query.Integer(4) == 1 && query.Text(5) == "predefined") ||
                    name.Length != 0 && !parameterNames.Add(name))
                    throw new InvalidDataException("Invalid macro parameter ownership, ordinal or name");
            }
        RejectRows("SELECT d.id FROM macro_definitions d WHERE d.variadic=1 AND NOT EXISTS(SELECT 1 FROM macro_parameters p WHERE p.definition_id=d.id)",
            "Variadic macro must own a final variadic parameter");

        // Effective installations are ordered; each definition belongs to one
        // define event. An undef of an unknown name is a valid preprocessor event.
        // Identical discarded redefinitions have no installation event, so this
        // is not a complete directive or expansion history.
        long nextEvent = 1, nextInstalledDefinition = 1;
        var installed = new Dictionary<string, long?>(StringComparer.Ordinal);
        var saved = new Dictionary<string, Stack<long?>>(StringComparer.Ordinal);
        using (var query = database.Prepare("""
            SELECT e.id,e.kind,e.name,e.definition_id,e.origin,e.source_file_id,e.line,e.column,
                   d.name,d.origin,d.source_file_id,d.line,d.column
            FROM macro_events e LEFT JOIN macro_definitions d ON d.id=e.definition_id ORDER BY e.id
            """))
            while (query.Read())
            {
                string kind = query.Text(1), name = query.Text(2), origin = query.Text(4);
                long? file = query.OptionalInteger(5);
                long line = query.Integer(6), column = query.Integer(7);
                if (query.Integer(0) != nextEvent++ || !Name(name))
                    throw new InvalidDataException("Invalid chronological macro event identity or name");
                Location(origin, file, line, column);
                if (kind == "define")
                {
                    if (query.OptionalInteger(3) != nextInstalledDefinition++ || query.OptionalText(8) != name ||
                        query.OptionalText(9) != origin || query.OptionalInteger(10) != file ||
                        query.OptionalInteger(11) != line || query.OptionalInteger(12) != column)
                        throw new InvalidDataException("Macro installation event does not match its definition");
                    installed[name] = query.OptionalInteger(3);
                }
                else if (kind == "undef")
                {
                    if (!query.IsNull(3)) throw new InvalidDataException("Macro undefinition references a definition");
                    installed.Remove(name);
                }
                else if (kind is "push" or "pop")
                {
                    long? definition = query.OptionalInteger(3);
                    if (origin != "source" || definition is not null &&
                        (definition >= nextInstalledDefinition || query.OptionalText(8) != name))
                        throw new InvalidDataException("Macro stack event references an unavailable definition");
                    if (!saved.TryGetValue(name, out var stack)) saved[name] = stack = new();
                    if (kind == "push")
                    {
                        if (definition != installed.GetValueOrDefault(name))
                            throw new InvalidDataException("Macro push does not save its current definition");
                        stack.Push(definition);
                    }
                    else
                    {
                        if (stack.Count == 0 || stack.Pop() != definition)
                            throw new InvalidDataException("Macro pop does not restore the saved definition");
                        installed[name] = definition;
                    }
                }
                else throw new InvalidDataException("Invalid macro event kind or definition reference");
            }
        if (nextInstalledDefinition != nextDefinition) throw new InvalidDataException("Macro definition has no installation event");
    }

    public void WriteMacroMap(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("unitId", UnitId);
        writer.WriteString("primarySource", PrimarySource);
        writer.WriteString("semanticDigest", Digest);
        void Table(string name, string sql, params string[] columns)
        {
            writer.WriteStartArray(name);
            using var query = database.Prepare(sql);
            while (query.Read())
            {
                writer.WriteStartObject();
                for (int index = 0; index < columns.Length; index++)
                {
                    string column = columns[index];
                    switch (query.ColumnType(index))
                    {
                        case 1:
                            if (column is "variadic" or "dynamic") writer.WriteBoolean(column, query.Integer(index) == 1);
                            else writer.WriteNumber(column, query.Integer(index));
                            break;
                        case 3: writer.WriteString(column, query.Text(index)); break;
                        case 5: writer.WriteNull(column); break;
                        default: throw new InvalidDataException("Unsupported macro map cell type");
                    }
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        Table("sourceFiles", "SELECT id,path,parent_id FROM source_files ORDER BY id", "id", "path", "parentId");
        Table("macroDefinitions", "SELECT id,name,form,variadic,dynamic,definition_text,origin,source_file_id,line,column FROM macro_definitions ORDER BY id",
            "id", "name", "form", "variadic", "dynamic", "definitionText", "origin", "sourceFileId", "line", "column");
        Table("macroParameters", "SELECT definition_id,ordinal,name FROM macro_parameters ORDER BY definition_id,ordinal", "definitionId", "ordinal", "name");
        Table("macroEvents", "SELECT id,kind,name,definition_id,origin,source_file_id,line,column FROM macro_events ORDER BY id",
            "id", "kind", "name", "definitionId", "origin", "sourceFileId", "line", "column");
        writer.WriteEndObject();
    }
}
