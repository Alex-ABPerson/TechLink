# Algebra Expression System Design

This document describes how the algebra processing system here is designed.

All algebra expressions entered into the system are immediately parsed into trees. These trees are what we operate on internally, and can be rendered into text once we're done. For the sake of simplicity, this document assumes you are familiar with the general concept of trees and recursion to operate on said trees in the source.

In the app, you'll find a large range of tools for viewing the tree and testing certain functions on them.

## Tree Structure

The "tree" structure the algebra is parsed into is made of many different type of tree items, each of which are made of up of smaller parts.

Every tree item contains a `IsNegative` flag, which describes whether it's negated or not. For example, if you had the number `-5`, that would be a `Number` tree item but with the `IsNegative` flag set to `true`. Or, if you had `-xy` that would be a `TermLine` tree item with `IsNegative` set to `true`.

Below is a list of all the tree items:

- `Number`: This tree item represents a single, constant number. It has no children. E.g. `15`.
- `Variable`: This tree item represents a variable. It has no children. E.g. `x`.
- `TermLine`: This tree item represents a "line" of one or more multiplications. This has one or more children, it can be any number of children. If you have `xyz` or `x * y * z` in the input, for example, that is turned into *one* term line item with three children in it, each of the variables. This tree item also has a `Coefficient`, which represents any constant integer to multiply all the children by. A *normalized* tree (described later) is **NOT** allowed to have any numbers in the `TermLine`, they should only be present in its coefficient. `2x`, for example, when normalized, would be a term line with a coefficient of `2` and one child: The `x`.
- `AdditiveLine`: This tree item represents a "line" of one or more additions. This has two or more children. If you have `x + y + z` in the input, for example, that is turned into *one* addition line.
- `Division`: Represents two numbers divided together. E.g. `x / y`.
- `Power`: Represents a base to the power of an exponent. A *normalized* tree (described later) is **NOT** allowed to have an integer as the exponent. E.g. `x^y`
- `Root`: Represents a root of some form, contains an `Index`, representing what type of root it is (square root, cube root etc.) as well as an `Inner` containing the actual thing being rooted. E.g. `sqrt(x, 3)` represents a cube root of `x`.
- `Function`: Represents either `sin`, `cos` or `tan`. Has an `IsInverse` to describe whether they're inverse functions (like `sin-1`). E.g. `sin(5)`

Subtraction is represented by an `AdditiveLine` with the second item marked as `IsNegative`.

## Normalized Trees

There are so many different ways to represent the same thing. For example, you could have `x^2` or you could have `xx`. In order for the system to work effectively and to simplify the processors to not need to worry about all these situations, it's important to decide upon a standardised way the trees will always be during processing.

A normalized tree is a tree that follows a set of rules, described below, about how exactly it should be structured. 

The only thing capable of producing an un-normalized tree should be the parser, which will immediately have its result put into a **normalizer** before any processing starts to ensure the tree is normalized for the processors. No processor should ever output an un-normalized tree. In debug mode, TechLink will constantly be running the tree through a verifier to confirm it's definitely normalized, as it is a bug if it doesn't output one.

There are some rules described here that the parser's output would never break. The **normalizer** does not try to normalize these things as they should literally never appear in a tree in the first place. The verifier does detect when these rules are broken, however.

The term "constant integer" is used repeatedly when describing these rules, this term refers to when a certain item does not depend on any variables that may be known at processing-time, the following things can be constant integers:

- `Number`s
- `sin`, `cos`, `tan` if everything within them is constant and their value is an integer
- `sqrt` if its value reduces to an integer.

### No constant powers

The first rule all normalized trees follow is there will **never** be a power with a constant integer as an exponent. For example, the following tree:
```
Power
|
|-> x
|-> 2
```

Will become
```
TermLine
|
|-> x
|-> x
|-> x
```

The **normalizer** *will* normalize this if the first is detected.

### No numbers in TermLines

There should be **no** `Number`s directly within `TermLine`s.

This is *not* normalized:
```
TermLine 
|
|-> 2
|-> x
```

The `2` should instead be moved to the coefficient, and represented as the following:
```
TermLine (Coefficient: 2)
|
|-> x
```

Similarly, this is *not* normalized:
```
TermLine 
|
|-> 2
|-> x
|-> 4
```

And should be represented as the following instead:
```
TermLine (Coefficient: 8)
|
|-> x
```

However, this *is* normalized, as there are no direct `Number` children:
```
TermLine
|
|-> x
|-> Addition
|   |
|   |-> y
|   |-> 2
|
```

The **normalizer** *will* normalize into the coefficient if any numbers are detected as direct children.

### No like terms

Addition lines should not contain any uncollected "like terms".

For two children to be considered "like", they need to either *be* the exact same variable, or be a term line with the exact same variable as the only child.

If the normalizer sees like terms in an additive line, it should join them together into a single term line with the coefficient counting how many were present.

For example, `x + x + x` should become `3x`.
And `2x + x` should become `3x`.

This only applies to variables

### Embedded TermLines

A `TermLine` should not have a `TermLine` as one its *direct children*.

The **normalizer** *will not* normalize this 

### Unevaluated constant expressionns

Any expression involving entirely 

## Techniques

- Fraction Combining
- Folding - 
- Root Factorization
- Rationalization

Check: How is `IsNegative` handled in term lines and such?