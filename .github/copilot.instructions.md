### Syntax And Formatting

 - never use "var", always use explicit type names
 - always use uppercase for acronyms and initialisms in PascalCase, but only use uppercase for acronyms and initialisms in camelCase if it is not at the start of a symbol name
   - RIGHT: UserID, GetGUID, HTMLParser, userGUID, accountID, httpStatusCode
   - WRONG: UserId, GetGuid, HtmlParser, userGuid, accountId, hTTPStatusCode
 - always keep vertical whitespace usage consistent; if there is any already-existing code in the proximity, inspect it and use the same vertical whitespace usage conventions
 - always keep syntax and formatting styles of generated code consistent with syntax and formatting styles of already-existing code; if there is any already-existing code in the proximity, inspect it and use the same syntax and formatting styles
 - always align lambda operators for switch expressions, as in the following example:
    ```
    return UserType switch
    {
        UserType.Staff  => "internal",
        UserType.Client => "external",
        _               => throw new ArgumentOutOfRangeException(@$"Unsupported User Type ""{UserType}""")
    };
    ```
