// Parallelisation In NUnit Is Disabled By Default
[assembly: Parallelizable(ParallelScope.Fixtures)]

// The Maximum Number Of Logical Processors To Use When Running Tests In Parallel
// If Not Specified, NUnit Uses The Logical Processor Count Or "2", Whichever Is Greater
// A Value Of "0" Represents The Default
[assembly: LevelOfParallelism(0)]

// Whether To Instantiate A Test Fixture Once For All The Contained Tests Or Every Time For Every Test Case (Potentially, Multiple Times Per Test)
// This Is The Major Difference Between NUnit And xUnit/MSTest, And (Arguably) This Should Be Set To "LifeCycle.InstancePerTestCase"
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
