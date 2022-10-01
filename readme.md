## Functional: A Functional Programming Library for C#

***A word of caution:*** *Functional* is currently a work in progress, and development is being done directly on the master branch, so the API *can* and *will* change at any time, without notice. Should you use this library at the present time, do so at your own risk!

### What is Functional?
*Functional* is my personal library of extensions, functions, objects and structures for writing functional code in C#. It's an amalgamation of the LaYumba.Functional,language-ext and CSharpFunctionalExtensions libraries copied, modified and extended to suit my own needs.

### Why?

After reading the wonderful *Functional Programming in C#* by Enrico Buonanno (unaffiliated) I saw how functional programming concepts, techniques and structures could be extremely useful in my code bases. There are several libraries that exist currently that implement FP concepts in .NET for C#, however I found that not all of them quite had the structure or functionality that I was looking for, so instead I am creating, combining, modifying and extending these libraries, along with implementing my own structures and functions, to create a functional utility library that I can use in my own projects.

*Functional* is very "opinionated" and is used in several of my projects. It is not optimized for performance, but rather for readability and maintainability. It is not intended to be a "drop-in" replacement for the existing libraries, but rather a combination of them, with some of my own ideas and implementations.

### What's in it?

Some of the features from *Functional* that I use most often are:

* `Result` and `Result<T>` types for better handling of errors, expected, unexpected, exceptional and otherwise compared to throwing exceptions
* `Option` and `Option<T>` types for better null and empty handling
* `ValueObject<T>` for creating typed value objects
* `Enumeration<T>` and `FlaggedEnumeration<T>` for creating strongly typed static enumeration value types, similar to `ValueObject<T>` but for enumerations
  * `DynamicEnumeration<T>` for creating dynamic enumeration value types which can add new values at runtime
* `Functional.F` static class
* Several useful structures and monads from functional programming, like `Identity<T>`, `Option<T>`, `Either<TLeft, TRight>`
* Extensions to `IEumerable<T>` and `IEnumerable`
* And more!

## Installation

This library is not current available on NuGet, however once a stable version is released it will be.

At the moment, I choose to `git submodule` this library into my projects, but you can also just copy the source files into your project.

## Usage

Reference `Functional.Core` in your project, and you're good to go!

Should you want to use the static functions, add:

```csharp
using static Functional.Core.F;
```

to your file.

If using C# 10, you can optionally add a `global using static Functional.Core.F;` to your project file to make the static available everywhere.
If using C# 10, you can optionally add a `Usings.cs` file and add:

```csharp
global using static Functional.Core.F;
```

to the file to make the static methods available everywhere within your project.

## Basic Feature Overview:

This is a quick overview of some of the features of *Functional*. For more information, please see the [wiki]().

Most of the structures of *Functional* have some sort of "base" class (`Result` for `Result<T>`, `Option` for `Option<T>`, etc) which define many implicit operations to make working with them easier. For example, `Option<T>` as a conversion to and from `T`. This allows for flexable and clear code, such as:
    
```csharp
Option<int> option = Some(5);
int value = option;

public Option<int> GetSome() => 5; // 5 -> Some(5)
// or
public Option<int> GetNone() => Option.None; // None -> Option<int>

int value = GetSome().Value; // Some(5) -> 5

int value = GetNone(); // None -> default(int) -> 0
int value = GetNone().ValueOrThrow(); // None -> throws InvalidOperationException
```

### `Result` and `Result<T>`

#### Why use `Result` instead of returning `null`? Why not Try...(.., ref T result)? Throw an exception?

In traditional C# programming, we have a few options for handling errors:

* Return `null` or `default(T)` and hope that the caller checks for it
* Return a `bool` or `enum` to indicate success or failure, and return the result via an `out` or `ref` parameter (Try...(.., ref T result) pattern)
* Throw an exception

All of these options have their own problems:

* Returning `null` or `default(T)` can lead to ambiguity as to the actual result of the operation (is `null` a valid result, or an error?) and can lead to `NullReferenceException`s if the caller doesn't check for it.
* Returning a `bool` or `enum` is not descriptive can throw away useful information about the error
* Throwing an exception is costly, and exceptions should (ideally) only be used for actual *exceptional* circumstances, not for expected errors.
* None of these options allow for the composition of errors.

The `Result` and various `Error` fixes many of these problems:

* A `Result` is either a `Success` or an `Failure`, and is composed of a list of `Error`s
  * Any given Result is a `Success` if and only if it has no `Error`s which are not expected
  * Functions which define a sequence of operations no longer need to check for errors after each operation, but can instead compose the results of each operation into a single `Result`.
  * A `Result` is a monad, which allows for functional composition of operations using `Bind`, `Map`, `Apply`, etc.

#### Sample Usage of `Result` and `Result<T>`

```csharp
// Create a Result
var result = Result.Success;

// Create a Result<T>
var result = Result.Success(42);

// Create a Result with an Error
var result = Result.Failure("Something went wrong"); // <- implicit conversion from string -> Error

// Create a Result<T> with an Error
var result = Result.Failure<int>("Something went wrong"); // <- implicit conversion from string -> Error

// Create a Result with multiple Errors
var result = Result.Failure("Something went wrong", "Something else went wrong"); // <- implicit conversion from string -> Error

// Combine Results
var result = Result.Combine(Result.Success(), Result.Success()); // <- Success
var result = Result.Combine(Result.Success(), "Something went wrong"); // <- Failure

// Add a Result to another Result
var result = Result.Success; // <- Success
result += Result.Success(43); // <- Success
result += Result.Failure("Something went wrong"); // <- Failure

// Add an Error to a Result
var result = Result.Success; // <- Success
result += "Something went wrong"; // <- Failure
result.WithError("Something else went wrong"); // <- Failure

// Implicit conversion within method to Result<T>
public Result<int> GetNumber() => 42; // <- Success(42)

public Result<int> GreaterThanZero(int number)
{
    // Implicit conversion from int to Result<T>
    if (number > 0) return number; // <- Success(number)
    else return "Number must be greater than zero"; // <- Implicit conversion from string -> Result<int> (Failure)
}
```