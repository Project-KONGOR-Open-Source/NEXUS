### Syntax And Formatting

 - never use "var", always use explicit type names

 - always use uppercase for acronyms and initialisms in PascalCase, but only use uppercase for acronyms and initialisms in camelCase if it is not at the start of a symbol name
   - RIGHT: UserID, GetGUID, HTMLParser, userGUID, accountID, httpStatusCode
   - WRONG: UserId, GetGuid, HtmlParser, userGuid, accountId, hTTPStatusCode

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
