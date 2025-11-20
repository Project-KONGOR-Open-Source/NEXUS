namespace ASPIRE.SourceGenerator.Analysers;

/// <summary>
///     Generates NotImplementedException-throwing implementations for all unimplemented interface members in classes decorated with <c>[AutoImplementMissingMembers]</c>.
/// </summary>
/// <remarks>
///     This generator inspects each class with the attribute, and determines which interface it implements.
///     Then finds all members not explicitly implemented by the class, and generates stub implementations which throw <see cref="System.NotImplementedException"/>.
/// </remarks>
[Generator]
public class AutoImplementMissingMembersGenerator : IIncrementalGenerator
{
    private static readonly string AttributeFullName = typeof(AutoImplementMissingMembersAttribute).FullName;

    /// <summary>
    ///     Holds class symbol and syntax information for code generation.
    /// </summary>
    private sealed class ClassData(INamedTypeSymbol symbol, ClassDeclarationSyntax syntax)
    {
        public INamedTypeSymbol Symbol { get; } = symbol;

        public ClassDeclarationSyntax Syntax { get; } = syntax;
    }

    // Diagnostic Descriptors
    private static readonly DiagnosticDescriptor ClassMustBePartialRule = new
    (
        id: "NX0001",
        title: "Class Must Be Partial",
        messageFormat: @"Class ""{0}"" has the [AutoImplementMissingMembers] attribute but is not declared as partial. Add the ""partial"" keyword to the class declaration.",
        category: "NEXUS.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes decorated with [AutoImplementMissingMembers] must be declared as partial to allow the source generator to add missing interface implementations."
    );

    /// <summary>
    ///     Initialises the incremental source generator by registering syntax providers and source outputs with the specified context.
    /// </summary>
    /// <remarks>
    ///     Call this method from the generator's initialization entry point to set up the necessary syntax providers and output registrations.
    ///     This method should be invoked only once per generator instance.
    /// </remarks>
    /// <param name="context">The context used to configure the incremental generator and register source outputs.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Identify Classes Annotated With [AutoImplementMissingMembers]
        IncrementalValuesProvider<ClassData> classDeclarations = context.SyntaxProvider // Access Syntax Provider API
            .ForAttributeWithMetadataName( // Filter Nodes By Attribute Metadata Name
                fullyQualifiedMetadataName: AttributeFullName, // Specify Fully Qualified Attribute Name
                predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax, // Restrict Matches To Class Declarations
                transform: static (attributeContext, _) => new ClassData // Create ClassData Wrapper For Match
                (
                    symbol: (INamedTypeSymbol) attributeContext.TargetSymbol, // Capture The Class Symbol
                    syntax: (ClassDeclarationSyntax) attributeContext.TargetNode) // Capture The Class Syntax Node
                );

        // Register Source Output For Each Discovered Class Declaration
        context.RegisterSourceOutput(classDeclarations, // Provide Sequence Of Annotated Classes
            static (sourceProductionContext, classData) => Execute(classData, sourceProductionContext)); // Register Source Generation Callback Invoked For Each Annotated Class
    }

    /// <summary>
    ///     Analyses a class annotated with <c>[AutoImplementMissingMembers]</c>, determines interface members not yet implemented, enforces the partial class requirement when stubs are needed, and emits a companion partial class containing <see cref="System.NotImplementedException"/>-throwing stubs for missing methods and properties.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         1. Gathers all interface members (methods and properties).
    ///         2. Filters out members already implemented (including explicit interface implementations).
    ///         3. If at least one member is missing and the class is not declared <c>partial</c>, reports diagnostic NX0001 and aborts generation.
    ///         4. Otherwise generates a source file named <c>{ClassName}.g.cs</c> with a partial class implementing the missing members by throwing <see cref="System.NotImplementedException"/>.
    ///     </code>
    /// </remarks>
    /// <param name="classData">Symbol and syntax data for the target class.</param>
    /// <param name="context">Source production context for reporting diagnostics and adding generated source.</param>
    private static void Execute(ClassData classData, SourceProductionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        // Extract Class Symbol
        INamedTypeSymbol classSymbol = classData.Symbol;

        // Extract Class Declaration Syntax
        ClassDeclarationSyntax classDeclaration = classData.Syntax;

        // Check If There Are Unimplemented Members
        ImmutableArray<ISymbol> unimplementedMembers = GetUnimplementedInterfaceMembers(classSymbol);

        // Only Enforce Partial Requirement If There Are Unimplemented Members
        if (unimplementedMembers.IsEmpty is false)
        {
            // Check If Class Is Declared As Partial
            bool isPartial = classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

            if (isPartial is false)
            {
                // Report Diagnostic Error
                Diagnostic diagnostic = Diagnostic.Create
                (
                    ClassMustBePartialRule,
                    classDeclaration.Identifier.GetLocation(),
                    classSymbol.Name
                );

                context.ReportDiagnostic(diagnostic);

                // Skip Code Generation For This Class
                return;
            }
        }

        // Generate Source Code
        string source = GenerateImplementations(classSymbol, classDeclaration, unimplementedMembers);

        // Add Generated Source To Source Production Context
        context.AddSource($"{classSymbol.Name}.g.cs", source);
    }

    /// <summary>
    ///     Generates the partial class with all missing interface member implementations.
    /// </summary>
    private static string GenerateImplementations(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration, ImmutableArray<ISymbol> unimplementedMembers)
    {

        StringBuilder sourceBuilder = new ();

        // Add File Header
        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine();

        // Disable Nullability Warnings
        sourceBuilder.AppendLine("# nullable disable");
        sourceBuilder.AppendLine();

        // Suppress Pragma Warnings
        sourceBuilder.AppendLine("# pragma warning disable SER001 // Suppress StackExchange.Redis Experimental API Warnings");
        sourceBuilder.AppendLine();

        // Add Namespace
        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        sourceBuilder.AppendLine($"namespace {namespaceName};");
        sourceBuilder.AppendLine();

        // Start Partial Class
        string accessibility = GetAccessibilityString(classSymbol.DeclaredAccessibility);
        sourceBuilder.AppendLine($"{accessibility} partial class {classSymbol.Name}");
        sourceBuilder.AppendLine("{");

        // Generate Each Missing Member
        foreach (ISymbol member in unimplementedMembers)
        {
            GenerateMemberImplementation(sourceBuilder, member);
        }

        // Ensure Exactly One Newline Character Before Closing Brace
        string body = sourceBuilder.ToString().TrimEnd('\r', '\n');
        sourceBuilder.Clear();
        sourceBuilder.Append(body);
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("}");

        return sourceBuilder.ToString();
    }

    /// <summary>
    ///     Finds all interface members that are not implemented by the class.
    /// </summary>
    private static ImmutableArray<ISymbol> GetUnimplementedInterfaceMembers(INamedTypeSymbol classSymbol)
    {
        ImmutableHashSet<ISymbol> implementedMembers = classSymbol.GetMembers() // Get All Class Members
            .Where(member => member.IsImplicitlyDeclared is false) // Exclude Implicit Compiler Members
            .ToImmutableHashSet(SymbolEqualityComparer.Default); // Materialise Immutable Set For Fast Lookup

        Dictionary<string, ISymbol> uniqueInterfaceMembers = [];

        // Deduplicate Interface Members By Name And Signature
        foreach (INamedTypeSymbol interfaceSymbol in classSymbol.AllInterfaces)
        {
            foreach (ISymbol member in interfaceSymbol.GetMembers())
            {
                if (member.Kind != SymbolKind.Method && member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Event)
                {
                    continue;
                }

                // Skip Property Accessor Methods As They Are Generated With The Property
                if (member is IMethodSymbol accessorMethod && (accessorMethod.MethodKind == MethodKind.PropertyGet || accessorMethod.MethodKind == MethodKind.PropertySet))
                {
                    continue;
                }

                // Skip Event Accessor Methods As They Are Generated With The Event
                if (member is IMethodSymbol eventAccessorMethod && (eventAccessorMethod.MethodKind == MethodKind.EventAdd || eventAccessorMethod.MethodKind == MethodKind.EventRemove))
                {
                    continue;
                }

                // Create Unique Key From Member Name And Signature
                string memberKey = member.Name + "|" + member.Kind.ToString();

                if (member is IMethodSymbol methodSymbol)
                {
                    memberKey += "|" + string.Join(",", methodSymbol.Parameters.Select(parameter => parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

                    if (methodSymbol.IsGenericMethod)
                    {
                        memberKey += "|" + methodSymbol.Arity.ToString();
                    }
                }

                else if (member is IPropertySymbol propertySymbol)
                {
                    memberKey += "|" + propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                else if (member is IEventSymbol eventSymbol)
                {
                    memberKey += "|" + eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                if (uniqueInterfaceMembers.ContainsKey(memberKey) is false)
                {
                    uniqueInterfaceMembers[memberKey] = member;
                }
            }
        }

        ImmutableArray<ISymbol> allInterfaceMembers = uniqueInterfaceMembers.Values.ToImmutableArray();

        IEnumerable<ISymbol> unimplemented = allInterfaceMembers // Begin Filtering For Unimplemented Members
            .Where(interfaceMember => IsImplemented(interfaceMember, implementedMembers, classSymbol) is false); // Exclude Already Implemented Members

        List<ISymbol> finalUniqueMembers = [];

        // Additional Deduplication Using Signature Matching
        foreach (ISymbol member in unimplemented)
        {
            bool isDuplicate = false;

            foreach (ISymbol existingMember in finalUniqueMembers)
            {
                if (existingMember.Name == member.Name && SignaturesMatch(existingMember, member))
                {
                    isDuplicate = true;

                    break;
                }
            }

            if (isDuplicate is false)
            {
                finalUniqueMembers.Add(member);
            }
        }

        return finalUniqueMembers.ToImmutableArray(); // Materialise Unimplemented Members Into Immutable Array
    }

    /// <summary>
    ///     Checks if an interface member is already implemented by the class.
    /// </summary>
    private static bool IsImplemented(ISymbol interfaceMember, ImmutableHashSet<ISymbol> implementedMembers, INamedTypeSymbol classSymbol)
    {
        ISymbol? implementation = classSymbol.FindImplementationForInterfaceMember(interfaceMember);

        // Check Explicit Implementations
        if (implementation is not null && implementation.IsAbstract is false)
        {
            return true;
        }

        IEnumerable<ISymbol> membersByName = classSymbol.GetMembers(interfaceMember.Name);

        // Check All Members By Name (Including Partial Class Members)
        foreach (ISymbol member in membersByName)
        {
            if (member.IsImplicitlyDeclared || member.IsAbstract)
            {
                continue;
            }

            if (SignaturesMatch(member, interfaceMember))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks if two members have matching signatures.
    /// </summary>
    private static bool SignaturesMatch(ISymbol memberOne, ISymbol memberTwo)
    {
        if (memberOne.Kind != memberTwo.Kind)
        {
            return false;
        }

        if (memberOne is IMethodSymbol methodOne && memberTwo is IMethodSymbol methodTwo)
        {
            return methodOne.Parameters.Length == methodTwo.Parameters.Length &&
                methodOne.Parameters.Zip(methodTwo.Parameters, (parameterOne, parameterTwo) =>
                    SymbolEqualityComparer.Default.Equals(parameterOne.Type, parameterTwo.Type)).All(parametersMatch => parametersMatch);
        }

        if (memberOne is IPropertySymbol propertyOne && memberTwo is IPropertySymbol propertyTwo)
        {
            return SymbolEqualityComparer.Default.Equals(propertyOne.Type, propertyTwo.Type);
        }

        if (memberOne is IEventSymbol eventOne && memberTwo is IEventSymbol eventTwo)
        {
            return SymbolEqualityComparer.Default.Equals(eventOne.Type, eventTwo.Type);
        }

        return false;
    }

    /// <summary>
    ///     Generates the implementation code for a single member.
    /// </summary>
    private static void GenerateMemberImplementation(StringBuilder builder, ISymbol member)
    {
        switch (member)
        {
            case IMethodSymbol method:
                GenerateMethodImplementation(builder, method);
                break;

            case IPropertySymbol property:
                GeneratePropertyImplementation(builder, property);
                break;

            case IEventSymbol eventSymbol:
                GenerateEventImplementation(builder, eventSymbol);
                break;
        }
    }

    /// <summary>
    ///     Generates a method implementation that throws NotImplementedException.
    /// </summary>
    private static void GenerateMethodImplementation(StringBuilder builder, IMethodSymbol method)
    {
        string returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string parameters = string.Join(", ", method.Parameters.Select(parameter =>
            $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {parameter.Name}"));

        string typeParameters = "";

        // Handle Generic Type Parameters
        if (method.IsGenericMethod)
        {
            string typeParameterList = string.Join(", ", method.TypeParameters.Select(typeParameter => typeParameter.Name));

            typeParameters = $"<{typeParameterList}>";
        }

        builder.AppendLine($"    public {returnType} {method.Name}{typeParameters}({parameters})");
        builder.AppendLine("    {");
        builder.AppendLine($"        throw new System.NotImplementedException(@\"Method \"\"{method.Name}\"\" Is Not Implemented In Test Double\");");
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    /// <summary>
    ///     Generates a property implementation that throws NotImplementedException.
    /// </summary>
    private static void GeneratePropertyImplementation(StringBuilder builder, IPropertySymbol property)
    {
        string propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        builder.AppendLine($"    public {propertyType} {property.Name}");
        builder.AppendLine("    {");

        if (property.GetMethod is not null)
        {
            builder.AppendLine($"        get => throw new System.NotImplementedException(@\"Property \"\"{property.Name}\"\" Getter Is Not Implemented In Test Double\");");
        }

        if (property.SetMethod is not null)
        {
            builder.AppendLine($"        set => throw new System.NotImplementedException(@\"Property \"\"{property.Name}\"\" Setter Is Not Implemented In Test Double\");");
        }

        builder.AppendLine("    }");
        builder.AppendLine();
    }

    /// <summary>
    ///     Generates an event implementation with empty add/remove accessors.
    /// </summary>
    private static void GenerateEventImplementation(StringBuilder builder, IEventSymbol eventSymbol)
    {
        string eventType = eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        builder.AppendLine($"    public event {eventType}? {eventSymbol.Name}");
        builder.AppendLine("    {");
        builder.AppendLine("        add { }");
        builder.AppendLine("        remove { }");
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    /// <summary>
    ///     Converts accessibility to a string keyword.
    /// </summary>
    private static string GetAccessibilityString(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public    => "public",
            Accessibility.Internal  => "internal",
            Accessibility.Protected => "protected",
            Accessibility.Private   => "private",
            _                       => "internal"
        };
    }
}
