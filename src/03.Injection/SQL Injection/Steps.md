## Using ADO.NET

1. Show the filter functionality
2. Demonstrate sql injection by using the following search param

    `sql 
%'; CREATE TABLE User1(Name VARCHAR(20)); select * from sys.tables where 1 =1 or 1='
`

3. Page will execute correctly, but it will create a table in the DB. Show that from the object explorer

## Using EF

1. Show the index method, FromSqlRaw method statement is built using sql statements


# Fixing Issues

## EF

1. Remove the usage of `FromSqlRaw` method to `FromSqlInterpolated` in the action method `FundTransfersAsync` in the file `Controllers\HomeController.cs`

```c#
fundtransfer = _context.FundTransfer.FromSqlInterpolated($"Select * from FundTransfer Where Note Like {SearchString}");
```

The interpolated parameter, `SearchString`, will then be converted into a `DbParameter` object,
making the code safe from SQL injection

2. Parameterization is a proven secure coding practice that will prevent SQL injection. Another way
to rewrite the code in this recipe is to use the DbParameter classes.

```c#
    var searchParam = new SqlParameter("@SearchString", SearchString);

    fundtransfer = _context.FundTransfer.FromSqlRaw("Select * from FundTransfer Where Note Like '%@searchString%' ", searchParam);

    
```

## ADO.NET

You can rewrite the vulnerable ADO.NET code and make it secure by using query parameters.
The `AddWithValue` method from `SqlParametersCollection` of the `SqlCommand` object allows you to add query parameters and safely pass values into the query:

```sql
cmd.Parameters.AddWithValue("@searchparam", search);
```

Changing the search string into a placeholder makes the query parameterized:

```sql
SqlCommand cmd = new SqlCommand("Select * from FundTransfer where Note like @searchparam ", con);
```