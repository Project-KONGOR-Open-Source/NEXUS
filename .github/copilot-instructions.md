### Syntax And Formatting

 - never use "var", always use explicit type names

 - always use uppercase for acronyms and initialisms in PascalCase, but only use uppercase for acronyms and initialisms in camelCase if it is not at the start of a symbol name
   - RIGHT: UserID, GetGUID, HTMLParser, userGUID, accountID, httpStatusCode
   - WRONG: UserId, GetGuid, HtmlParser, userGuid, accountId, hTTPStatusCode

 - always use full variable names for delegates and lambda parameters; never use abbreviations or single-letter names like "x", "y", "z", etc.
   - RIGHT: numbers.Select(number => number * number);
   - WRONG: numbers.Select(x => x * x);

 - always use four spaces for indentation, and always terminate files with a newline character

 - always keep vertical whitespace usage consistent; if there is any already-existing code in the proximity, inspect it and use the same vertical whitespace usage conventions

 - always keep syntax and formatting styles consistent with those of already-existing code; if there is any already-existing code in the proximity, inspect it and use the same syntax and formatting styles

 - always align lambda operators for switch expressions, as in the following example:
    ```
    return UserType switch
    {
        UserType.Staff  => "internal",
        UserType.Client => "external",
        _               => throw new ArgumentOutOfRangeException(@$"Unsupported User Type ""{UserType}""")
    };
    ```

 - don't add comments for obvious code; only add comments when they add value by explaining why something is done a certain way or providing context that isn't immediately clear from the code itself

 - when adding code comments use StartCase, but not if that would change the letter case for acronyms, initialisms, symbol names, etc.
   - COMMENT: // This Is A Sample Comment Explaining The Purpose Of The "request" Variable In The HTTP Handler

 - when adding summaries for code elements (like classes, methods, properties, etc.) use sentence case with correct indentation and punctuation, including a punctuation mark at the end of each sentence
   - SUMMARY LINE #1: /// <summary>
   - SUMMARY LINE #2: ///     This method retrieves user information based on the provided user ID.
   - SUMMARY LINE #3: ///     It does do by using the "request" variable from the HTTP handler.
   - SUMMARY LINE #4: /// </summary>

 - always enclose references to symbol names, branch names, etc. in double quotation marks unless it is possible to use a <see cref="..."/> tag instead
   - COMMENT: The "GetUserData" Method Is Responsible For Fetching User Information
   - SUMMARY LINE #1: /// <summary>
   - SUMMARY LINE #2: ///     The <see cref="GetUserData"/> method is responsible for fetching user information.
   - SUMMARY LINE #3: /// </summary>

 - when using a <see cref="..."/> tag to refer to a method or another code construct which takes parameters, do not include the parameters
   - RIGHT: <see cref="CalculateTotal"/>
   - WRONG: <see cref="CalculateTotal(int, int)"/>

 - never use the null-forgiving operator (!) to suppress null handling; then only times where this is acceptable are the following:
   - when using Include or ThenInclude in Entity Framework queries to reference navigation properties that are known to be not null according to the data model, but are reference types or marked as nullable in the code
   - when assigning "null!" to a nullable variable, field, or property is unavoidable without making the code overly-complex, and it is absolutely certain that the value will not be null at runtime

 - when creating asynchronous methods, never use the "Async" suffix in the method name unless there is also a synchronous version of the same method in the same scope

 - always keep any and all code and comments in British English, and never use American English

 - always use full words for symbols, never use abbreviations
   - RIGHT: configuration, administrator, implementation
   - WRONG: config, admin, impl

 - in comments, use "TRUE", "FALSE", and "NULL" when referring to the boolean and null literals, to clearly distinguish them from their natural-language meanings

### Code Generation

 - always ensure that the generated code is syntactically correct and adheres to best practices for the target programming language
   - if necessary and possible, check relevant online sources or documentation to verify compliance with the latest standards

 - always ensure that the generated code is efficient and optimized for performance, avoiding unnecessary computations or memory usage, unless readability and maintainability are significantly compromised
   - if necessary and possible, check relevant online sources or documentation to verify compliance with the latest performance optimization techniques

 - always ensure that the generated code is secure, following best practices to prevent vulnerabilities such as injection attacks, data leaks, and unauthorized access
   - if necessary and possible, check relevant online sources or documentation to verify compliance with the latest security standards

 - always keep the generated code consistent with the existing codebase in terms of style, conventions, and architecture
   - if necessary and possible, inspect the existing codebase to identify relevant patterns and practices to follow

 - always keep generated code targeted towards the objective, avoiding unnecessary features or complexity that do not directly contribute to the specified goal
   - if necessary and possible, review the prompt to ensure alignment with the specified objectives and confirm with the user if there is any ambiguity
   - when in doubt, present the available options to the user in a clear manner and let them decide on the best approach

 - prefer simplicity and clarity over complexity and unnecessary features
   - avoid over-engineering or adding features that are not explicitly requested or required
   - keep the code as straightforward and easy to understand as possible while still meeting the specified requirements
   - keep the code minimalistic and to the point, avoiding unnecessary abstractions, layers, or components that do not add significant value to the overall design or functionality

 - always prefer global using directives, and add or move them to a "Internals/UsingDirectives.cs" file in the project root
   - keep using directives in lexicographic order ascending, grouped by assembly
   - use global using directive syntax rather than the "global::" alias qualifier
     - RIGHT: global using Aspire.Hosting;
     - WRONG: using global::Aspire.Hosting;
