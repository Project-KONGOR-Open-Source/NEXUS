// Parallelisation In NUnit Is Disabled By Default
[assembly: Parallelizable(ParallelScope.Fixtures)]

// This Attribute Is Optional; If It Is Not Specified, NUnit Uses The Processor Count Or 2, Whichever Is Greater
// For Example, On A Four Processor Machine, The Default Value Is 4
// Enable This Attribute To Disable Parallelisation But Keep The Execution Scope Defined By The "Parallelizable" Attribute
// [assembly: LevelOfParallelism(1)]
