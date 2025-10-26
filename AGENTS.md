# AGENTS.md

## Code style

### File headers and documentation
- Every C# source file starts with the project StyleCop-generated copyright banner. Preserve it verbatim on new files and keep it synchronized with the existing wording. 
- `public` and `internal` types and members are documented with XML comments (do *not* document `private` entities). Use `<summary>`, `<param>`, `<returns>`, and `<exception>` elements as demonstrated in the code base, and prefer `<inheritdoc/>` when overriding base members. Keep comment text in complete sentences ending with periods. Use `c` and `see` tags wherever it is applicable. Break lines longer than 100 symbols to another line. Avoid `example`, `remarks`, and `para` elements.
- The text must be explanatory, but reasonably short.
-  Use correct American English grammar and punctuation.
### Namespaces and using directives
- Use file-scoped namespaces (e.g., `namespace FastImageViewer;`). Omit indentation at the top level because braces are not used with file-scoped namespaces.
- Keep using directives sorted alphabetically within logical groups separated by blank lines: `System.*` first, then third-party packages, then project namespaces. Place `using` directives outside the namespace declaration per StyleCop configuration.
- Avoid redundant `using` directives (`<ImplicitUsings>enable</ImplicitUsings>`).
- Prefer namespace-qualified types instead of aliases unless clarity demands otherwise.
- Namespaces must be synchronized with the folder structure.
### Formatting and layout
- Indent with four spaces and never use tabs. Braces follow the Allman style with the opening brace on its own line and the closing brace aligned with the statement that opened it. Always include braces even for single-line conditionals or loops.
- Wrap long argument lists and fluent calls vertically with one argument per line, indented once from the method call (except if there is only one argument – in this case, keep it on a single line).
- Chain members on separate lines to emphasize call structure (except if there is only one member – in this case, keep it on a single line).
- Maintain one blank line between logically distinct code sections (e.g., field declarations, constructors, event handlers) but avoid multiple consecutive blank lines.
- Keep trailing whitespace out of files and intentionally omit the final newline at the end of each file, matching the StyleCop configuration (`layoutRules.newlineAtEndOfFile = omit`).
### Naming and member design
- Use PascalCase for classes, methods, properties, events, constants, and enums. Prefix private instance fields with an underscore and camelCase (e.g., `_controller`). Parameters and local variables use camelCase. Boolean fields typically start with `is`/`has` prefixes.
- Use `const` and `readonly` appropriately; constants remain PascalCase. Prefer explicit access modifiers on all members (e.g., `internal sealed`, `private static`).
- Sort elements in the following order and priority: constants, properties, fields, events, `private` enums, `private` structs, `private` records, constructors, methods; `public`, `internal`, `protected`, `private`; `static`, non-static; `async`, non-async; lexicographically ascending.
- All `public` and `internal` classes, structures, records, and enums must be placed in separate .cs files. The file name must be the same as the entity name.
- Preserve the concise single-line style (`[]`) for empty array expressions and tuple/record parameter lists when they fit on one line.
### Coding patterns
- Use object and collection initializers where possible. Leverage `record` types for immutable data carriers and keep constructor or parameter lists vertically aligned when spanning multiple lines.
- Prefer immutable collections over mutable.
- Defer expensive work and I/O to asynchronous APIs; propagate cancellation tokens and respect `ConfigureAwait` defaults consistent with the rest of the codebase.
- Use code patterns: "Clean Code", "S.O.L.I.D.", DRY, KISS, YAGNI, "Fail Fast", "Graceful Degradation".
- Code must be ready for unit testing with any "dirty" hacks.
- Async methods end with the `Async` postfix and accept `CancellationToken` as the final required parameter. Propagate cancellation with `ThrowIfCancellationRequested()` if required.
- Perform early returns for guard clauses and null checks before continuing with the main logic.
- Prefer `var` for local variables when the type is evident from the right-hand side.
### StyleCop analyzers
- The repository uses StyleCop Analyzers with the default rule set. Expect warnings and/or errors for violations of documentation, layout, ordering, readability, maintainability, naming, and spacing rules (SA1xxx–SA7xxx categories). Ensure new code compiles cleanly under these analyzers.
- Customized StyleCop settings require omitting trailing newlines at the end of files and keeping using directives outside namespaces (`orderingRules.usingDirectivesPlacement = outsideNamespace`).
### SonarCube analyzers
- The repository uses `SonarAnalyzer.CSharp` with the default rule set. Expect warnings and/or errors for violations of documentation, layout, ordering, readability, maintainability, naming, and spacing rules (Sxxxx categories). Ensure new code compiles cleanly under these analyzers.

## Dev environment tips

Please note that the first ("cold") solution build (using `dotnet build`, for example) may take up to two minutes, even in the case of a successful build. In case of an unsuccessful build, when it is caused by errors that you cannot fix (*only* in this case), for example, by the invalid image/environment config, provide me with the **full complete** log of such a build.