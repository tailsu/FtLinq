Consider the following statement:

```
var myList = Enumerable.Range(0, 1000)
    .Where(i => i % 2 == 0)`\
    .Select(i => i * 3)`\
    .ToList();
```

This is a very nice, declarative way of transforming a sequence. The
problem - it's not very fast. It may actually [be fast
enough](http://stackoverflow.com/questions/3769989/should-linq-be-avoided-because-its-slow>)
but it's just not as fast as the imperative version:

```
var myList = new List<int>();
for (int i = 0; i < 1000; ++i)
{
    if (i % 2 == 0)
        myList.Add(i * 3);
}
```
The above is about 3x faster than the LINQ version, and for some
queries, the imperative version may be as much as 60x faster. 

The difference from the imperative version is two-fold:

-   the `Where` and `Select` operations incur a function call for
    every element in their input sequence.
-   the `Where` and `Select` extension methods take a plain
    `IEnumerable` as their source sequence, which makes iteration work
    through virtual calls - two per element.

Here's what this library does - it lets you write LINQ queries that are
recompiled into their imperative version with almost zero effort. You
get the readability of LINQ and performance of hand-coded loops.

We rewrite our original query as follows:
```
var myFilter = Ftl.Compile((IEnumerable<int> source) =>
 source
   .Where(i => i % 2 == 0)
   .Select(i => i * 3)
   .ToList());
var myList = myFilter(Enumerable.Range(0, 1000));
```
The magic in the `Compile` method produces the myFilter delegate do the stuff
inside the expression given to the `Compile` method but at the speed of a
hand-coded foreach statement. We could then cache the compiled function
like so:
```
static readonly Func<IEnumerable<int>, List<int>> myFilter
    = Ftl.Compile((IEnumerable<int>) source) =>
     source
       .Where(i => i % 2 == 0)
       .Select(i => i * 3)
       .ToList());

```

and later on use this delegate as a regular function:
```
List<int> myList = myFilter(Enumerable.Range(0, 1000));
```

This way commonly executed queries will run as fast as hand-coded
foreach statements, but will retain the expressiveness and
maintainability of LINQ queries. We could even make special versions of
the delegates that take an array or IList as their first argument and
implement iteration through a 'for' statement and indexing instead of
through GetEnumerator() for that extra bit of performance.

